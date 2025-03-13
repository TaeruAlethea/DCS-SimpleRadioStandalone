using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.ClientList;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Utils;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Helpers;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Overlay;
using Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NLog;
using WPFCustomMessageBox;
using InputBinding = Ciribob.DCS.SimpleRadio.Standalone.Client.Settings.InputBinding;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly IMainViewModel ViewModel;
        
        public delegate void ReceivedAutoConnect(string address, int port);
        public delegate void ToggleOverlayCallback(bool uiButton, bool awacs);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private Overlay.RadioOverlayWindow _radioOverlayWindow;
        private AwacsRadioOverlayWindow.RadioOverlayWindow _awacsRadioOverlay;
        private ServerSettingsWindow _serverSettingsWindow;
        private ClientListWindow _clientListWindow;

        //used to debounce toggle
        private long _toggleShowHide;
        

        public MainWindow(IMainViewModel viewModel)
        {
            DataContext = ViewModel = viewModel;
            InitializeComponent();

            // Initialize ToolTip controls
            ToolTips.Init();

            // Initialise sounds
            Sounds.Init();

            // Set up tooltips that are always defined
            InitToolTips();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Title = Title + " - " + UpdaterChecker.VERSION;

            CheckWindowVisibility();

            if (ViewModel.SrsSettings.GlobalSettings.StartMinimised)
            {
                Hide();
                WindowState = WindowState.Minimized;
                Logger.Info("Started DCS-SimpleRadio Client " + UpdaterChecker.VERSION + " minimized");
            }
            else
            {
                Logger.Info("Started DCS-SimpleRadio Client " + UpdaterChecker.VERSION);
            }

            Analytics.Log("Client", "Startup", ViewModel.SrsSettings.GlobalSettings.ClientIdLong);

            InitSettingsScreen();

            ReloadProfile();

            InitInput();
            
            UpdatePresetsFolderLabel();

            if ((SpeakerBoostLabel != null) && (SpeakerBoost != null))
            {
                SpeakerBoostLabel.Content = VolumeConversionHelper.ConvertLinearDiffToDB( ViewModel.AudioManager.SpeakerBoost);
            }
            
            InitFlowDocument();
        }

        private void CheckWindowVisibility()
        {
            if (ViewModel.SrsSettings.GlobalSettings.DisableWindowVisibilityCheck)
            {
                Logger.Info("Window visibility check is disabled, skipping");
                return;
            }

            bool mainWindowVisible = false;
            bool radioWindowVisible = false;
            bool awacsWindowVisible = false;

            int mainWindowX = (int)Left;
            int mainWindowY = (int)Top; 
            int radioWindowX = (int)ViewModel.SrsSettings.GlobalSettings.RadioX;
            int radioWindowY = (int)ViewModel.SrsSettings.GlobalSettings.RadioY;
            int awacsWindowX = (int)ViewModel.SrsSettings.GlobalSettings.AwacsX;
            int awacsWindowY = (int)ViewModel.SrsSettings.GlobalSettings.AwacsY;

            Logger.Info($"Checking window visibility for main client window {{X={mainWindowX},Y={mainWindowY}}}");
            Logger.Info($"Checking window visibility for radio overlay {{X={radioWindowX},Y={radioWindowY}}}");
            Logger.Info($"Checking window visibility for AWACS overlay {{X={awacsWindowX},Y={awacsWindowY}}}");

            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                Logger.Info($"Checking {(screen.Primary ? "primary " : "")}screen {screen.DeviceName} with bounds {screen.Bounds} for window visibility");

                if (screen.Bounds.Contains(mainWindowX, mainWindowY))
                {
                    Logger.Info($"Main client window {{X={mainWindowX},Y={mainWindowY}}} is visible on {(screen.Primary ? "primary " : "")}screen {screen.DeviceName} with bounds {screen.Bounds}");
                    mainWindowVisible = true;
                }
                if (screen.Bounds.Contains(radioWindowX, radioWindowY))
                {
                    Logger.Info($"Radio overlay {{X={radioWindowX},Y={radioWindowY}}} is visible on {(screen.Primary ? "primary " : "")}screen {screen.DeviceName} with bounds {screen.Bounds}");
                    radioWindowVisible = true;
                }
                if (screen.Bounds.Contains(awacsWindowX, awacsWindowY))
                {
                    Logger.Info($"AWACS overlay {{X={awacsWindowX},Y={awacsWindowY}}} is visible on {(screen.Primary ? "primary " : "")}screen {screen.DeviceName} with bounds {screen.Bounds}");
                    awacsWindowVisible = true;
                }
            }

            if (!mainWindowVisible)
            {
                MessageBox.Show(this,
                    Properties.Resources.MsgBoxNotVisibleText,
                    Properties.Resources.MsgBoxNotVisible,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Logger.Warn($"Main client window outside visible area of monitors, resetting position ({mainWindowX},{mainWindowY}) to defaults");
                
                Left = 200;
                Top = 200;
            }

            if (!radioWindowVisible)
            {
                MessageBox.Show(this,
                    Properties.Resources.MsgBoxNotVisibleText,
                    Properties.Resources.MsgBoxNotVisible,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Logger.Warn($"Radio overlay window outside visible area of monitors, resetting position ({radioWindowX},{radioWindowY}) to defaults");

                ViewModel.SrsSettings.GlobalSettings.RadioX = 300;
                ViewModel.SrsSettings.GlobalSettings.RadioY = 300;

                if (_radioOverlayWindow != null)
                {
                    _radioOverlayWindow.Left = 300;
                    _radioOverlayWindow.Top = 300;
                }
            }

            if (!awacsWindowVisible)
            {
                MessageBox.Show(this,
                    Properties.Resources.MsgBoxNotVisibleText,
                    Properties.Resources.MsgBoxNotVisible,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Logger.Warn($"AWACS overlay window outside visible area of monitors, resetting position ({awacsWindowX},{awacsWindowY}) to defaults");

                ViewModel.SrsSettings.GlobalSettings.AwacsX = 300;
                ViewModel.SrsSettings.GlobalSettings.AwacsY = 300;

                if (_awacsRadioOverlay != null)
                {
                    _awacsRadioOverlay.Left = 300;
                    _awacsRadioOverlay.Top = 300;
                }
            }
        }

        private void InitFlowDocument()
        {
            //make hyperlinks work
            var hyperlinks = WPFElementHelper.GetVisuals(AboutFlowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler((sender, args) =>
                {
                    Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri));
                    args.Handled = true;
                });
        }

        void ReloadProfile()
        {
            //switch profiles
            Logger.Info(ControlsProfile.SelectedValue as string + " - Profile now in use");

            //redraw UI
            ReloadInputBindings();
            ReloadProfileSettings();
            ReloadRadioAudioChannelSettings();
        }

        private void InitInput()
        {
            InputManager = new InputDeviceManager(this, ToggleOverlay);
            
            RadioStartTransmitEffect.SelectionChanged += OnRadioStartTransmitEffectChanged;
            RadioEndTransmitEffect.SelectionChanged += OnRadioEndTransmitEffectChanged;

            IntercomStartTransmitEffect.SelectionChanged += OnIntercomStartTransmitEffectChanged;
            IntercomEndTransmitEffect.SelectionChanged += OnIntercomEndTransmitEffectChanged;

            Radio1.InputName = Properties.Resources.InputRadio1;
            Radio1.ControlInputBinding = InputBinding.Switch1;
            Radio1.InputDeviceManager = InputManager;

            Radio2.InputName = Properties.Resources.InputRadio2;
            Radio2.ControlInputBinding = InputBinding.Switch2;
            Radio2.InputDeviceManager = InputManager;

            Radio3.InputName = Properties.Resources.InputRadio3;
            Radio3.ControlInputBinding = InputBinding.Switch3;
            Radio3.InputDeviceManager = InputManager;

            PushToTalk.InputName = Properties.Resources.InputPTT;
            PushToTalk.ControlInputBinding = InputBinding.Ptt;
            PushToTalk.InputDeviceManager = InputManager;

            Intercom.InputName = Properties.Resources.InputIntercom;
            Intercom.ControlInputBinding = InputBinding.Intercom;
            Intercom.InputDeviceManager = InputManager;

            IntercomPushToTalk.InputName = Properties.Resources.InputIntercomPTT;
            IntercomPushToTalk.ControlInputBinding = InputBinding.IntercomPTT;
            IntercomPushToTalk.InputDeviceManager = InputManager;

            RadioOverlay.InputName = Properties.Resources.InputRadioOverlay;
            RadioOverlay.ControlInputBinding = InputBinding.OverlayToggle;
            RadioOverlay.InputDeviceManager = InputManager;

            AwacsOverlayToggle.InputName = Properties.Resources.InputAwacsOverlay;
            AwacsOverlayToggle.ControlInputBinding = InputBinding.AwacsOverlayToggle;
            AwacsOverlayToggle.InputDeviceManager = InputManager;

            Radio4.InputName = Properties.Resources.InputRadio4;
            Radio4.ControlInputBinding = InputBinding.Switch4;
            Radio4.InputDeviceManager = InputManager;

            Radio5.InputName = Properties.Resources.InputRadio5;
            Radio5.ControlInputBinding = InputBinding.Switch5;
            Radio5.InputDeviceManager = InputManager;

            Radio6.InputName = Properties.Resources.InputRadio6;
            Radio6.ControlInputBinding = InputBinding.Switch6;
            Radio6.InputDeviceManager = InputManager;

            Radio7.InputName = Properties.Resources.InputRadio7;
            Radio7.ControlInputBinding = InputBinding.Switch7;
            Radio7.InputDeviceManager = InputManager;

            Radio8.InputName = Properties.Resources.InputRadio8;
            Radio8.ControlInputBinding = InputBinding.Switch8;
            Radio8.InputDeviceManager = InputManager;

            Radio9.InputName = Properties.Resources.InputRadio9;
            Radio9.ControlInputBinding = InputBinding.Switch9;
            Radio9.InputDeviceManager = InputManager;

            Radio10.InputName = Properties.Resources.InputRadio10;
            Radio10.ControlInputBinding = InputBinding.Switch10;
            Radio10.InputDeviceManager = InputManager;

            Up100.InputName = Properties.Resources.InputUp100;
            Up100.ControlInputBinding = InputBinding.Up100;
            Up100.InputDeviceManager = InputManager;

            Up10.InputName = Properties.Resources.InputUp10;
            Up10.ControlInputBinding = InputBinding.Up10;
            Up10.InputDeviceManager = InputManager;

            Up1.InputName = Properties.Resources.InputUp1;
            Up1.ControlInputBinding = InputBinding.Up1;
            Up1.InputDeviceManager = InputManager;

            Up01.InputName = Properties.Resources.InputUp01;
            Up01.ControlInputBinding = InputBinding.Up01;
            Up01.InputDeviceManager = InputManager;

            Up001.InputName = Properties.Resources.InputUp001;
            Up001.ControlInputBinding = InputBinding.Up001;
            Up001.InputDeviceManager = InputManager;

            Up0001.InputName = Properties.Resources.InputUp0001;
            Up0001.ControlInputBinding = InputBinding.Up0001;
            Up0001.InputDeviceManager = InputManager;


            Down100.InputName = Properties.Resources.InputDown100;
            Down100.ControlInputBinding = InputBinding.Down100;
            Down100.InputDeviceManager = InputManager;

            Down10.InputName = Properties.Resources.InputDown10;
            Down10.ControlInputBinding = InputBinding.Down10;
            Down10.InputDeviceManager = InputManager;

            Down1.InputName = Properties.Resources.InputDown1;
            Down1.ControlInputBinding = InputBinding.Down1;
            Down1.InputDeviceManager = InputManager;

            Down01.InputName = Properties.Resources.InputDown01;
            Down01.ControlInputBinding = InputBinding.Down01;
            Down01.InputDeviceManager = InputManager;

            Down001.InputName = Properties.Resources.InputDown001;
            Down001.ControlInputBinding = InputBinding.Down001;
            Down001.InputDeviceManager = InputManager;

            Down0001.InputName = Properties.Resources.InputDown0001;
            Down0001.ControlInputBinding = InputBinding.Down0001;
            Down0001.InputDeviceManager = InputManager;

            ToggleGuard.InputName = Properties.Resources.InputToggleGuard;
            ToggleGuard.ControlInputBinding = InputBinding.ToggleGuard;
            ToggleGuard.InputDeviceManager = InputManager;

            NextRadio.InputName = Properties.Resources.InputNextRadio;
            NextRadio.ControlInputBinding = InputBinding.NextRadio;
            NextRadio.InputDeviceManager = InputManager;

            PreviousRadio.InputName = Properties.Resources.InputPreviousRadio;
            PreviousRadio.ControlInputBinding = InputBinding.PreviousRadio;
            PreviousRadio.InputDeviceManager = InputManager;

            ToggleEncryption.InputName = Properties.Resources.InputToggleEncryption;
            ToggleEncryption.ControlInputBinding = InputBinding.ToggleEncryption;
            ToggleEncryption.InputDeviceManager = InputManager;

            EncryptionKeyIncrease.InputName = Properties.Resources.InputEncryptionIncrease;
            EncryptionKeyIncrease.ControlInputBinding = InputBinding.EncryptionKeyIncrease;
            EncryptionKeyIncrease.InputDeviceManager = InputManager;

            EncryptionKeyDecrease.InputName = Properties.Resources.InputEncryptionDecrease;
            EncryptionKeyDecrease.ControlInputBinding = InputBinding.EncryptionKeyDecrease;
            EncryptionKeyDecrease.InputDeviceManager = InputManager;

            RadioChannelUp.InputName = Properties.Resources.InputRadioChannelUp;
            RadioChannelUp.ControlInputBinding = InputBinding.RadioChannelUp;
            RadioChannelUp.InputDeviceManager = InputManager;

            RadioChannelDown.InputName = Properties.Resources.InputRadioChannelDown;
            RadioChannelDown.ControlInputBinding = InputBinding.RadioChannelDown;
            RadioChannelDown.InputDeviceManager = InputManager;

            TransponderIdent.InputName = Properties.Resources.InputTransponderIDENT;
            TransponderIdent.ControlInputBinding = InputBinding.TransponderIDENT;
            TransponderIdent.InputDeviceManager = InputManager;

            RadioVolumeUp.InputName = Properties.Resources.InputRadioVolumeUp;
            RadioVolumeUp.ControlInputBinding = InputBinding.RadioVolumeUp;
            RadioVolumeUp.InputDeviceManager = InputManager;

            RadioVolumeDown.InputName = Properties.Resources.InputRadioVolumeDown;
            RadioVolumeDown.ControlInputBinding = InputBinding.RadioVolumeDown;
            RadioVolumeDown.InputDeviceManager = InputManager;
        }

        private void OnRadioStartTransmitEffectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RadioStartTransmitEffect.IsEnabled)
            {
                GlobalSettingsStore.Instance.ProfileSettingsStore.SetClientSettingString(ProfileSettingsKeys.RadioTransmissionStartSelection, ((CachedAudioEffect)RadioStartTransmitEffect.SelectedItem).FileName);
            }
        }

        private void OnRadioEndTransmitEffectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RadioEndTransmitEffect.IsEnabled)
            {
                GlobalSettingsStore.Instance.ProfileSettingsStore.SetClientSettingString(ProfileSettingsKeys.RadioTransmissionEndSelection, ((CachedAudioEffect)RadioEndTransmitEffect.SelectedItem).FileName);
            }
        }

        private void OnIntercomStartTransmitEffectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IntercomStartTransmitEffect.IsEnabled)
            {
                GlobalSettingsStore.Instance.ProfileSettingsStore.SetClientSettingString(ProfileSettingsKeys.IntercomTransmissionStartSelection, ((CachedAudioEffect)IntercomStartTransmitEffect.SelectedItem).FileName);
            }
        }

        private void OnIntercomEndTransmitEffectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IntercomEndTransmitEffect.IsEnabled)
            {
                GlobalSettingsStore.Instance.ProfileSettingsStore.SetClientSettingString(ProfileSettingsKeys.IntercomTransmissionEndSelection, ((CachedAudioEffect)IntercomEndTransmitEffect.SelectedItem).FileName);
            }
        }

        private void ReloadInputBindings()
        {
            Radio1.LoadInputSettings();
            Radio2.LoadInputSettings();
            Radio3.LoadInputSettings();
            PushToTalk.LoadInputSettings();
            Intercom.LoadInputSettings();
            IntercomPushToTalk.LoadInputSettings();
            RadioOverlay.LoadInputSettings();
            Radio4.LoadInputSettings();
            Radio5.LoadInputSettings();
            Radio6.LoadInputSettings();
            Radio7.LoadInputSettings();
            Radio8.LoadInputSettings();
            Radio9.LoadInputSettings();
            Radio10.LoadInputSettings();
            Up100.LoadInputSettings();
            Up10.LoadInputSettings();
            Up1.LoadInputSettings();
            Up01.LoadInputSettings();
            Up001.LoadInputSettings();
            Up0001.LoadInputSettings();
            Down100.LoadInputSettings();
            Down10.LoadInputSettings();
            Down1.LoadInputSettings();
            Down01.LoadInputSettings();
            Down001.LoadInputSettings();
            Down0001.LoadInputSettings();
            ToggleGuard.LoadInputSettings();
            NextRadio.LoadInputSettings();
            PreviousRadio.LoadInputSettings();
            ToggleEncryption.LoadInputSettings();
            EncryptionKeyIncrease.LoadInputSettings();
            EncryptionKeyDecrease.LoadInputSettings();
            RadioChannelUp.LoadInputSettings();
            RadioChannelDown.LoadInputSettings();
            RadioVolumeUp.LoadInputSettings();
            RadioVolumeDown.LoadInputSettings();
            AwacsOverlayToggle.LoadInputSettings();
        }

        private void ReloadRadioAudioChannelSettings()
        {
            Radio1Config.Reload();
            Radio2Config.Reload();
            Radio3Config.Reload();
            Radio4Config.Reload();
            Radio5Config.Reload();
            Radio6Config.Reload();
            Radio7Config.Reload();
            Radio8Config.Reload();
            Radio9Config.Reload();
            Radio10Config.Reload();
            IntercomConfig.Reload();
        }

        private void InitToolTips()
        {
            ExternalAwacsModePassword.ToolTip = ToolTips.ExternalAWACSModePassword;
            //ExternalAwacsModeName.ToolTip = ToolTips.ExternalAWACSModeName;
            ConnectExternalAwacsMode.ToolTip = ToolTips.ExternalAWACSMode;
        }

        public InputDeviceManager InputManager { get; set; }

        public void UpdateVUMeters(object sender, EventArgs e)
        {
            if (ViewModel.AudioPreview != null)
            {
                // Only update mic volume output if an audio input device is available - sometimes the value can still change, leaving the user with the impression their mic is working after all
                if (ViewModel.AudioInput.MicrophoneAvailable)
                {
                    MicVu.Value = ViewModel.AudioPreview.MicMax;
                }
                SpeakerVu.Value = ViewModel.AudioPreview.SpeakerMax;
            }
            else if (ViewModel.AudioManager  != null)
            {
                // Only update mic volume output if an audio input device is available - sometimes the value can still change, leaving the user with the impression their mic is working after all
                if (ViewModel.AudioInput.MicrophoneAvailable)
                {
                    MicVu.Value = ViewModel.AudioManager.MicMax;
                }
                SpeakerVu.Value = ViewModel.AudioManager.SpeakerMax;
            }
            else
            {
                MicVu.Value = -100;
                SpeakerVu.Value = -100;
            }
            
            ConnectedClientsSingleton.Instance.NotifyAll();
        }

        private void InitSettingsScreen()
        {
            var objValue = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\DCS-SR-Standalone", "SRSAnalyticsOptOut", "FALSE");
            if (objValue == null || (string)objValue == "TRUE")
            {
                AllowAnonymousUsage.IsChecked = false;
            }
            else
            {
                AllowAnonymousUsage.IsChecked = true;
            }
        }

        private void ReloadProfileSettings()
        {
            RadioEndTransmitEffect.IsEnabled = false;
            RadioEndTransmitEffect.ItemsSource = CachedAudioEffectProvider.Instance.RadioTransmissionEnd;
            RadioEndTransmitEffect.SelectedItem = CachedAudioEffectProvider.Instance.SelectedRadioTransmissionEndEffect;
            RadioEndTransmitEffect.IsEnabled = true;

            RadioStartTransmitEffect.IsEnabled = false;
            RadioStartTransmitEffect.SelectedIndex = 0;
            RadioStartTransmitEffect.ItemsSource = CachedAudioEffectProvider.Instance.RadioTransmissionStart;
            RadioStartTransmitEffect.SelectedItem = CachedAudioEffectProvider.Instance.SelectedRadioTransmissionStartEffect;
            RadioStartTransmitEffect.IsEnabled = true;

            IntercomStartTransmitEffect.IsEnabled = false;
            IntercomStartTransmitEffect.ItemsSource = CachedAudioEffectProvider.Instance.IntercomTransmissionStart;
            IntercomStartTransmitEffect.SelectedItem = CachedAudioEffectProvider.Instance.SelectedIntercomTransmissionStartEffect;
            IntercomStartTransmitEffect.IsEnabled = true;

            IntercomEndTransmitEffect.IsEnabled = false;
            IntercomEndTransmitEffect.SelectedIndex = 0;
            IntercomEndTransmitEffect.ItemsSource = CachedAudioEffectProvider.Instance.IntercomTransmissionEnd;
            IntercomEndTransmitEffect.SelectedItem = CachedAudioEffectProvider.Instance.SelectedIntercomTransmissionEndEffect;
            IntercomEndTransmitEffect.IsEnabled = true;
        }
        

        public void SaveSelectedInputAndOutput()
        {
            //save app settings
            // Only save selected microphone if one is actually available, resulting in a crash otherwise
            if (ViewModel.AudioInput.MicrophoneAvailable)
            {
                if (ViewModel.AudioInput.SelectedAudioInput.Value == null)
                {
                    ViewModel.SrsSettings.GlobalSettings.AudioInputDeviceId = "default";
                }
                else
                {
                    var input = ((MMDevice)ViewModel.AudioInput.SelectedAudioInput.Value).ID;
                    ViewModel.SrsSettings.GlobalSettings.AudioInputDeviceId = input;
                }
            }

            if (ViewModel.AudioOutput.SelectedAudioOutput.Value == null)
            {
                ViewModel.SrsSettings.GlobalSettings.AudioOutputDeviceId = "default";
            }
            else
            {
                var output = (MMDevice)ViewModel.AudioOutput.SelectedAudioOutput.Value;
                ViewModel.SrsSettings.GlobalSettings.AudioOutputDeviceId = output.ID;
            }

            //check if we have optional output
            if (ViewModel.AudioOutput.SelectedMicAudioOutput.Value != null)
            {
                var micOutput = (MMDevice)ViewModel.AudioOutput.SelectedMicAudioOutput.Value;
                ViewModel.SrsSettings.GlobalSettings.MicAudioOutputDeviceId = micOutput.ID;
            }
            else
            {
                ViewModel.SrsSettings.GlobalSettings.MicAudioOutputDeviceId = "";
            }

            ShowMicPassthroughWarning();
        }

        private void ShowMicPassthroughWarning()
        {
            if (ViewModel.SrsSettings.GlobalSettings.MicAudioOutputDeviceId == 
                ViewModel.SrsSettings.GlobalSettings.AudioOutputDeviceId)
            {
                MessageBox.Show(Properties.Resources.MsgBoxMicPassthruText, Properties.Resources.MsgBoxMicPassthru, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public void ConnectCallback(bool result, bool connectionError, string connection)
        {
            string currentConnection = ServerIp.Text.Trim();
            if (!currentConnection.Contains(":"))
            {
                currentConnection += ":5002";
            }

            if (result)
            {
                if (!ViewModel.ClientState.IsConnected)
                {
                    try
                    {
                        StartStop.Content = Properties.Resources.StartStopDisconnect;
                        StartStop.IsEnabled = true;

                        ViewModel.ClientState.IsConnected = true;
                        ViewModel.ClientState.IsVoipConnected = false;

                        if (ViewModel.SrsSettings.GlobalSettings.PlayConnectionSounds)
                        {
                            try
                            {
                                Sounds.BeepConnected.Play();
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn(ex, "Failed to play connect sound");
                            }
                        }

                        ViewModel.SrsSettings.GlobalSettings.LastServer = ServerIp.Text;

                        ViewModel.AudioManager.StartEncoding(ViewModel.Guid, InputManager,
                            ViewModel.ResolvedIp, ViewModel.Port);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex,
                            "Unable to get audio device - likely output device error - Pick another. Error:" +
                            ex.Message);
                        ViewModel.StopCommand.Execute(null);

                        var messageBoxResult = CustomMessageBox.ShowYesNo(
                            Properties.Resources.MsgBoxAudioErrorText,
                            Properties.Resources.MsgBoxAudioError,
                            "OPEN PRIVACY SETTINGS",
                            "JOIN DISCORD SERVER",
                            MessageBoxImage.Error);

                        if (messageBoxResult == MessageBoxResult.Yes) Process.Start("https://discord.gg/baw7g3t");
                    }
                }
            }
            else if (string.Equals(currentConnection, connection, StringComparison.OrdinalIgnoreCase))
            {
                // Only stop connection/reset state if connection is currently active
                // Autoconnect mismatch will quickly disconnect/reconnect, leading to double-callbacks
                ViewModel.StopCommand.Execute(true);
            }
            else
            {
                if (!ViewModel.ClientState.IsConnected)
                {
                    ViewModel.StopCommand.Execute(connectionError);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //save window position
            base.OnClosing(e);
            
            ViewModel.StopCommand.Execute(true);

            ViewModel.AudioPreview?.StopEncoding();
            ViewModel.AudioPreview = null;

            _radioOverlayWindow?.Close();
            _radioOverlayWindow = null;

            _awacsRadioOverlay?.Close();
            _awacsRadioOverlay = null;

            ViewModel.DcsAutoConnectListener?.Stop();
            ViewModel.DcsAutoConnectListener = null;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && ViewModel.SrsSettings.GlobalSettings.MinimiseToTray)
            {
                Hide();
            }

            base.OnStateChanged(e);
        }

        private void PreviewAudio(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AudioPreview == null)
            {
                if (!ViewModel.AudioInput.MicrophoneAvailable)
                {
                    Logger.Info("Unable to preview audio, no valid audio input device available or selected");
                    return;
                }

                //get device
                try
                {
                    SaveSelectedInputAndOutput();

                    ViewModel.AudioPreview = new AudioPreview();
                    ViewModel.AudioPreview.SpeakerBoost = VolumeConversionHelper.ConvertVolumeSliderToScale((float)SpeakerBoost.Value);
                    ViewModel.AudioPreview.StartPreview(ViewModel.AudioOutput.WindowsN);

                    Preview.Content = Properties.Resources.PreviewAudioStop;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex,
                        "Unable to preview audio - likely output device error - Pick another. Error:" + ex.Message);

                }
            }
            else
            {
                Preview.Content = Preview.Content = Properties.Resources.PreviewAudio;
                ViewModel.AudioPreview.StopEncoding();
                ViewModel.AudioPreview = null;
            }
        }

        public void UpdateUICallback()
        {
            if (ViewModel.ClientState.IsConnected)
            {
                ToggleServerSettings.IsEnabled = true;

                bool eamEnabled = ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);

                ConnectExternalAwacsMode.IsEnabled = eamEnabled;
                ConnectExternalAwacsMode.Content = ViewModel.ClientState.ExternalAWACSModelSelected ? Properties.Resources.DisconnectExternalAWACSMode : Properties.Resources.ConnectExternalAWACSMode;
            }
            else
            {
                ToggleServerSettings.IsEnabled = false;
                ConnectExternalAwacsMode.IsEnabled = false;
                ConnectExternalAwacsMode.Content = Properties.Resources.ConnectExternalAWACSMode;
            }
        }

        private void ShowOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleOverlay(true, false);
        }

        private void ToggleOverlay(bool uiButton, bool awacs)
        {
            //debounce show hide (1 tick = 100ns, 6000000 ticks = 600ms debounce)
            if ((DateTime.Now.Ticks - _toggleShowHide > 6000000) || uiButton)
            {
                _toggleShowHide = DateTime.Now.Ticks;

                if (awacs)
                {
                    ShowAwacsOverlay_OnClick(null, null);
                }
                else
                {
                    if ((_radioOverlayWindow == null) || !_radioOverlayWindow.IsVisible ||
                        (_radioOverlayWindow.WindowState == WindowState.Minimized))
                    {
                        //hide awacs panel
                        _awacsRadioOverlay?.Close();
                        _awacsRadioOverlay = null;

                        _radioOverlayWindow?.Close();

                        _radioOverlayWindow = new Overlay.RadioOverlayWindow();


                        _radioOverlayWindow.ShowInTaskbar = !ViewModel.SrsSettings.GlobalSettings.RadioOverlayTaskbarHide;
                        _radioOverlayWindow.Show();
                    }
                    else
                    {
                        _radioOverlayWindow?.Close();
                        _radioOverlayWindow = null;
                    }
                }
                
            }
        }

        private void ShowAwacsOverlay_OnClick(object sender, RoutedEventArgs e)
        {
            if ((_awacsRadioOverlay == null) || !_awacsRadioOverlay.IsVisible ||
                (_awacsRadioOverlay.WindowState == WindowState.Minimized))
            {
                //close normal overlay
                _radioOverlayWindow?.Close();
                _radioOverlayWindow = null;

                _awacsRadioOverlay?.Close();

                _awacsRadioOverlay = new AwacsRadioOverlayWindow.RadioOverlayWindow();
                _awacsRadioOverlay.ShowInTaskbar = !ViewModel.SrsSettings.GlobalSettings.RadioOverlayTaskbarHide;
                _awacsRadioOverlay.Show();
            }
            else
            {
                _awacsRadioOverlay?.Close();
                _awacsRadioOverlay = null;
            }
        }

        public void AutoConnect(string address, int port)
        {
            string connection = $"{address}:{port}";

            Logger.Info($"Received AutoConnect DCS-SRS @ {connection}");

            var enabled = ViewModel.SrsSettings.GlobalSettings.AutoConnect;

            if (!enabled)
            {
                Logger.Info($"Ignored Autoconnect - not Enabled");
                return;
            }

            if (ViewModel.ClientState.IsConnected)
            {
                // Always show prompt about active/advertised SRS connection mismatch if client is already connected
                string[] currentConnectionParts = ServerIp.Text.Trim().Split(':');
                string currentAddress = currentConnectionParts[0];
                int currentPort = 5002;
                if (currentConnectionParts.Length >= 2)
                {
                    if (!int.TryParse(currentConnectionParts[1], out currentPort))
                    {
                        Logger.Warn($"Failed to parse port {currentConnectionParts[1]} of current connection, falling back to 5002 for autoconnect comparison");
                        currentPort = 5002;
                    }
                }
                string currentConnection = $"{currentAddress}:{currentPort}";

                if (string.Equals(address, currentAddress, StringComparison.OrdinalIgnoreCase) && port == currentPort)
                {
                    // Current connection matches SRS server advertised by DCS, all good
                    Logger.Info($"Current SRS connection {currentConnection} matches advertised server {connection}, ignoring autoconnect");
                    return;
                }
                else if (port != currentPort)
                {
                    // Port mismatch, will always be a different server, no need to perform hostname lookups
                    HandleAutoConnectMismatch(currentConnection, connection);
                    return;
                }

                // Perform DNS lookup of advertised and current hostnames to find hostname/resolved IP matches
                List<string> currentIPs = new List<string>();

                if (IPAddress.TryParse(currentAddress, out IPAddress currentIP))
                {
                    currentIPs.Add(currentIP.ToString());
                }
                else
                {
                    try
                    {
                        foreach (IPAddress ip in Dns.GetHostAddresses(currentConnectionParts[0]))
                        {
                            // SRS currently only supports IPv4 (due to address/port parsing)
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                currentIPs.Add(ip.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(e, $"Failed to resolve current SRS host {currentConnectionParts[0]} to IP addresses, ignoring autoconnect advertisement");
                    }
                }

                if (currentIPs.Count == 0)
                {
                    Logger.Warn( $"Failed to resolve current SRS host {currentConnectionParts[0]} to IP addresses, ignoring autoconnect advertisement");
                    return;
                }

                List<string> advertisedIPs = new List<string>();

                if (IPAddress.TryParse(address, out IPAddress advertisedIP))
                {
                    advertisedIPs.Add(advertisedIP.ToString());
                }
                else
                {
                    try
                    {
                        foreach (IPAddress ip in Dns.GetHostAddresses(connection))
                        {
                            // SRS currently only supports IPv4 (due to address/port parsing)
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                advertisedIPs.Add(ip.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(e, $"Failed to resolve advertised SRS host {address} to IP addresses, ignoring autoconnect advertisement");
                        return;
                    }
                }

                if (!currentIPs.Intersect(advertisedIPs).Any())
                {
                    // No resolved IPs match, display mismatch warning
                    HandleAutoConnectMismatch(currentConnection, connection);
                }
            }
            else
            {
                //todo: This Prompt logic can probably be simplified
                
                // Show auto connect prompt if client is not connected yet and setting has been enabled, otherwise automatically connect
                bool showPrompt = ViewModel.SrsSettings.GlobalSettings.AutoConnectPrompt;
                bool connectToServer = !showPrompt;
                if (showPrompt)
                {
                    WindowHelper.BringProcessToFront(Process.GetCurrentProcess());

                    var result = MessageBox.Show(this,
                        $"{Properties.Resources.MsgBoxAutoConnectText} {address}:{port}? ", "Auto Connect",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    connectToServer = (result == MessageBoxResult.Yes) && (StartStop.Content.ToString().ToLower() == "connect");
                }

                if (connectToServer)
                {
                    ServerIp.Text = connection;
                    ViewModel.ConnectCommand.Execute(null);
                }
            }
        }

        private async void HandleAutoConnectMismatch(string currentConnection, string advertisedConnection)
        {
            // Show auto connect mismatch prompt if setting has been enabled (default), otherwise automatically switch server
            bool showPrompt = ViewModel.SrsSettings.GlobalSettings.AutoConnectMismatchPrompt;

            Logger.Info($"Current SRS connection {currentConnection} does not match advertised server {advertisedConnection}, {(showPrompt ? "displaying mismatch prompt" : "automatically switching server")}");

            bool switchServer = !showPrompt;
            if (showPrompt)
            {
                WindowHelper.BringProcessToFront(Process.GetCurrentProcess());

                var result = MessageBox.Show(this,
                    $"{Properties.Resources.MsgBoxMismatchText1} {advertisedConnection} {Properties.Resources.MsgBoxMismatchText2} {currentConnection} {Properties.Resources.MsgBoxMismatchText3}\n\n" +
                    $"{Properties.Resources.MsgBoxMismatchText4}",
                    Properties.Resources.MsgBoxMismatch,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                switchServer = result == MessageBoxResult.Yes;
            }

            if (switchServer)
            {
                ViewModel.StopCommand.Execute(null);

                StartStop.IsEnabled = false;
                StartStop.Content = Properties.Resources.StartStopConnecting;
                await Task.Delay(2000);
                StartStop.IsEnabled = true;
                ServerIp.Text = advertisedConnection;
                ViewModel.ConnectCommand.Execute(null);
            }
        }

        //Todo: See if there is a more elegant reset method.
        private void ResetRadioWindow_Click(object sender, RoutedEventArgs e)
        {
            //close overlay
            _radioOverlayWindow?.Close();
            _radioOverlayWindow = null;

            ViewModel.SrsSettings.GlobalSettings.RadioX = 300;
            ViewModel.SrsSettings.GlobalSettings.RadioY = 300;
            
            ViewModel.SrsSettings.GlobalSettings.RadioWidth = 122;
            ViewModel.SrsSettings.GlobalSettings.RadioHeight = 270;
            
            ViewModel.SrsSettings.GlobalSettings.RadioOpacity = 1.0;
        }

        private void ToggleServerSettings_OnClick(object sender, RoutedEventArgs e)
        {
            if ((_serverSettingsWindow == null) || !_serverSettingsWindow.IsVisible ||
                (_serverSettingsWindow.WindowState == WindowState.Minimized))
            {
                _serverSettingsWindow?.Close();

                _serverSettingsWindow = new ServerSettingsWindow();
                _serverSettingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _serverSettingsWindow.Owner = this;
                _serverSettingsWindow.Show();
            }
            else
            {
                _serverSettingsWindow?.Close();
                _serverSettingsWindow = null;
            }
        }

        private void RadioOverlayTaskbarItem_Click(object sender, RoutedEventArgs e)
        {
            if (_radioOverlayWindow != null)
                _radioOverlayWindow.ShowInTaskbar = !ViewModel.SrsSettings.GlobalSettings.RadioOverlayTaskbarHide;
            else if (_awacsRadioOverlay != null) _awacsRadioOverlay.ShowInTaskbar = !ViewModel.SrsSettings.GlobalSettings.RadioOverlayTaskbarHide;
        }

        private void ExpandInputDevices_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                Properties.Resources.MsgBoxRestartExpandText,
                Properties.Resources.MsgBoxRestart, MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void AllowXInputController_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                Properties.Resources.MsgBoxRestartXInputText,
                Properties.Resources.MsgBoxRestart, MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void LaunchAddressTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedItem = FavouritesSeversTab;
        }

        
        private void ConnectExternalAWACSMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Client == null ||
                !ViewModel.ClientState.IsConnected ||
                !ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE) ||
                (!ViewModel.ClientState.ExternalAWACSModelSelected &&
                 string.IsNullOrWhiteSpace(ExternalAwacsModePassword.Password)))
            {
                return;
            }

            // Already connected, disconnect
            if (ViewModel.ClientState.ExternalAWACSModelSelected)
            {
                ViewModel.Client.DisconnectExternalAWACSMode();
            }
            else if (!ViewModel.ClientState.IsGameExportConnected) //only if we're not in game
            {
                ViewModel.Client.ConnectExternalAWACSMode(ExternalAwacsModePassword.Password.Trim(), ExternalAWACSModeConnectionChanged);
            }
        }

        private void ExternalAWACSModeConnectionChanged(bool result, int coalition)
        {
            if (result)
            {
                ViewModel.ClientState.ExternalAWACSModelSelected = true;
                ViewModel.ClientState.PlayerCoalitionLocationMetadata.side = coalition;
                ViewModel.ClientState.PlayerCoalitionLocationMetadata.name = ViewModel.ClientState.LastSeenName;
                ViewModel.ClientState.DcsPlayerRadioInfo.name = ViewModel.ClientState.LastSeenName;

                ConnectExternalAwacsMode.Content = Properties.Resources.DisconnectExternalAWACSMode;
            }
            else
            {
                ViewModel.ClientState.ExternalAWACSModelSelected = false;
                ViewModel.ClientState.PlayerCoalitionLocationMetadata.side = 0;
                ViewModel.ClientState.PlayerCoalitionLocationMetadata.name = "";
                ViewModel.ClientState.DcsPlayerRadioInfo.name = "";
                ViewModel.ClientState.DcsPlayerRadioInfo.LastUpdate = 0;
                ViewModel.ClientState.LastSent = 0;

                ConnectExternalAwacsMode.Content = Properties.Resources.ConnectExternalAWACSMode;
                ExternalAwacsModePassword.IsEnabled = ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);
                //ExternalAwacsModeName.IsEnabled = ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);
            }
        }

        private void RescanInputDevices(object sender, RoutedEventArgs e)
        {
            InputManager.InitDevices();
            MessageBox.Show(this,
                Properties.Resources.MsgBoxRescanText,
                Properties.Resources.MsgBoxRescan,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SetSRSPath_Click(object sender, RoutedEventArgs e)
        {
            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\DCS-SR-Standalone", "SRPathStandalone", Directory.GetCurrentDirectory());

            MessageBox.Show(this,
                Properties.Resources.MsgBoxSetSRSPathText + Directory.GetCurrentDirectory(),
                Properties.Resources.MsgBoxSetSRSPath,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RequireAdminToggle_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this,
                Properties.Resources.MsgBoxAdminText,
                Properties.Resources.MsgBoxAdmin, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void CreateProfile(object sender, RoutedEventArgs e)
        {
            var inputProfileWindow = new InputProfileWindow.InputProfileWindow(name =>
                {
                    if (name.Trim().Length > 0)
                    {
                        ViewModel.SrsSettings.CreateProfileCommand.Execute(name); ;
                    }
                });
            inputProfileWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inputProfileWindow.Owner = this;
            inputProfileWindow.ShowDialog();
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            var current = ControlsProfile.SelectedValue as string;

            if (current.Equals("default"))
            {
                MessageBox.Show(this,
                    Properties.Resources.MsgBoxErrorInputText,
                    Properties.Resources.MsgBoxError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                var result = MessageBox.Show(this,
                    $"{Properties.Resources.MsgBoxConfirmDeleteText} {current} ?",
                    Properties.Resources.MsgBoxConfirm,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ViewModel.SrsSettings.DeleteProfileCommand.Execute(current);
                }

            }

        }

        private void RenameProfile(object sender, RoutedEventArgs e)
        {
            var current = ControlsProfile.SelectedValue as string;
            if (current.Equals("default"))
            {
                MessageBox.Show(this,
                    Properties.Resources.MsgBoxErrorRenameText,
                    Properties.Resources.MsgBoxError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                var oldName = current;
                var inputProfileWindow = new InputProfileWindow.InputProfileWindow(name =>
                {
                    if (name.Trim().Length > 0)
                    {
                        ViewModel.SrsSettings.RenameProfileCommand.Execute(name);
                    }
                }, true, oldName);
                inputProfileWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                inputProfileWindow.Owner = this;
                inputProfileWindow.ShowDialog();
            }

        }

        private void UpdatePresetsFolderLabel()
        {
            var presetsFolder = ViewModel.SrsSettings.GlobalSettings.LastPresetsFolder;
            if (!string.IsNullOrWhiteSpace(presetsFolder))
            {
                PresetsFolderLabel.Content = Path.GetFileName(presetsFolder);
                PresetsFolderLabel.ToolTip = presetsFolder;
            }
            else
            {
                PresetsFolderLabel.Content = "(default)";
                PresetsFolderLabel.ToolTip = Directory.GetCurrentDirectory();
            }
        }

        private void CopyProfile(object sender, RoutedEventArgs e)
        {
            var current = ControlsProfile.SelectedValue as string;
            var inputProfileWindow = new InputProfileWindow.InputProfileWindow(name =>
            {
                if (name.Trim().Length > 0)
                {
                    ViewModel.SrsSettings.DuplicateProfileCommand.Execute(name);
                }
            });
            inputProfileWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inputProfileWindow.Owner = this;
            inputProfileWindow.ShowDialog();
        }

        private void CurrentPosition_OnClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var pos = ViewModel.ClientState.PlayerCoalitionLocationMetadata.LngLngPosition;

                Process.Start($"https://maps.google.com/maps?q=loc:{pos.lat},{pos.lng}");
            }
            catch { }
        }

        private void ShowClientList_OnClick(object sender, RoutedEventArgs e)
        {
            if ((_clientListWindow == null) || !_clientListWindow.IsVisible ||
                (_clientListWindow.WindowState == WindowState.Minimized))
            {
                _clientListWindow?.Close();

                _clientListWindow = new ClientListWindow();
                _clientListWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _clientListWindow.Owner = this;
                _clientListWindow.Show();
            }
            else
            {
                _clientListWindow?.Close();
                _clientListWindow = null;
            }
        }
        
        private void Donate_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.patreon.com/ciribob",
                    UseShellExecute = true
                } );
            }
            catch (Exception)
            {
            }
        }

        private void AllowAnonymousUsage_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(bool)AllowAnonymousUsage.IsChecked)
            {
                MessageBox.Show(
                    Properties.Resources.MsgBoxPleaseTickText,
                    Properties.Resources.MsgBoxPleaseTick, MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\DCS-SR-Standalone", "SRSAnalyticsOptOut", "TRUE");
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.MsgBoxThankYouText,
                    Properties.Resources.MsgBoxThankYou, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\DCS-SR-Standalone", "SRSAnalyticsOptOut", "FALSE");
            }
        }

        private void PresetsFolderBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var selectPresetsFolder = new System.Windows.Forms.FolderBrowserDialog();
            selectPresetsFolder.SelectedPath = PresetsFolderLabel.ToolTip.ToString();
            if (selectPresetsFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.SrsSettings.GlobalSettings.LastPresetsFolder = selectPresetsFolder.SelectedPath;
                UpdatePresetsFolderLabel();
            }
        }

        private void PresetsFolderResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SrsSettings.GlobalSettings.LastPresetsFolder = string.Empty;
            UpdatePresetsFolderLabel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // Special Case Binding
                ViewModel.ServerAddress.EamCoalitionPassword = ((PasswordBox)sender).Password;
            } 
        }

        private void ControlsProfile_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}