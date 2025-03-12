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
        private readonly MainWindowViewModel ViewModel;
        
        public delegate void ReceivedAutoConnect(string address, int port);
        public delegate void ToggleOverlayCallback(bool uiButton, bool awacs);

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private Overlay.RadioOverlayWindow _radioOverlayWindow;
        private AwacsRadioOverlayWindow.RadioOverlayWindow _awacsRadioOverlay;
        private ServerSettingsWindow _serverSettingsWindow;
        private ClientListWindow _clientListWindow;

        //used to debounce toggle
        private long _toggleShowHide;
        

        public MainWindow()
        {
            DataContext = ViewModel = new MainWindowViewModel(this);
            InitializeComponent();

            // Initialize ToolTip controls
            ToolTips.Init();

            // Initialize images/icons
            Images.Init();

            // Initialise sounds
            Sounds.Init();

            // Set up tooltips that are always defined
            InitToolTips();

            var client = ClientStateSingleton.Instance;


            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.ClientX).DoubleValue;
            Top = ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.ClientY).DoubleValue;

            Title = Title + " - " + UpdaterChecker.VERSION;

            CheckWindowVisibility();

            if (ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.StartMinimised))
            {
                Hide();
                WindowState = WindowState.Minimized;

                Logger.Info("Started DCS-SimpleRadio Client " + UpdaterChecker.VERSION + " minimized");
            }
            else
            {
                Logger.Info("Started DCS-SimpleRadio Client " + UpdaterChecker.VERSION);
            }

            Analytics.Log("Client", "Startup", ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.ClientIdLong).RawValue);

            InitSettingsScreen();

            InitSettingsProfiles();
            ReloadProfile();

            InitInput();
            
            SpeakerVu.Value = -100;
            MicVu.Value = -100;

            ExternalAwacsModeName.Text = ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastSeenName).RawValue;
            UpdatePresetsFolderLabel();

            if ((SpeakerBoostLabel != null) && (SpeakerBoost != null))
            {
                SpeakerBoostLabel.Content = VolumeConversionHelper.ConvertLinearDiffToDB( ViewModel.AudioManager.SpeakerBoost);
            }

            UpdaterChecker.CheckForUpdate(ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.CheckForBetaUpdates));

            InitFlowDocument();

        }

        private void CheckWindowVisibility()
        {
            if (ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.DisableWindowVisibilityCheck))
            {
                Logger.Info("Window visibility check is disabled, skipping");
                return;
            }

            bool mainWindowVisible = false;
            bool radioWindowVisible = false;
            bool awacsWindowVisible = false;

            int mainWindowX = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.ClientX).DoubleValue;
            int mainWindowY = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.ClientY).DoubleValue;
            int radioWindowX = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.RadioX).DoubleValue;
            int radioWindowY = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.RadioY).DoubleValue;
            int awacsWindowX = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.AwacsX).DoubleValue;
            int awacsWindowY = (int)ViewModel.GlobalSettings.GetPositionSetting(GlobalSettingsKeys.AwacsY).DoubleValue;

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

                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.ClientX, 200);
                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.ClientY, 200);

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

                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioX, 300);
                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioY, 300);

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

                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.AwacsX, 300);
                ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.AwacsY, 300);

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



        private void InitSettingsProfiles()
        {
            ControlsProfile.IsEnabled = false;
            ControlsProfile.Items.Clear();
            foreach (var profile in ViewModel.GlobalSettings.ProfileSettingsStore.InputProfiles.Keys)
            {
                ControlsProfile.Items.Add(profile);
            }
            ControlsProfile.IsEnabled = true;
            ControlsProfile.SelectedIndex = 0;

            CurrentProfile.Content = ViewModel.GlobalSettings.ProfileSettingsStore.CurrentProfileName;

        }

        void ReloadProfile()
        {
            //switch profiles
            Logger.Info(ControlsProfile.SelectedValue as string + " - Profile now in use");
            ViewModel.GlobalSettings.ProfileSettingsStore.CurrentProfileName = ControlsProfile.SelectedValue as string;

            //redraw UI
            ReloadInputBindings();
            ReloadProfileSettings();
            ReloadRadioAudioChannelSettings();

            CurrentProfile.Content = ViewModel.GlobalSettings.ProfileSettingsStore.CurrentProfileName;
        }

        private void InitInput()
        {
            InputManager = new InputDeviceManager(this, ToggleOverlay);

            InitSettingsProfiles();

            ControlsProfile.SelectionChanged += OnProfileDropDownChanged;

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

        private void OnProfileDropDownChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ControlsProfile.IsEnabled)
                ReloadProfile();
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
            ExternalAwacsModeName.ToolTip = ToolTips.ExternalAWACSModeName;
            ConnectExternalAwacsMode.ToolTip = ToolTips.ExternalAWACSMode;
        }

        public InputDeviceManager InputManager { get; set; }

        public void UpdatePlayerLocationAndVUMeters(object sender, EventArgs e)
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

            try
            {
                var pos = ViewModel.ClientState.PlayerCoaltionLocationMetadata.LngLngPosition;
                CurrentPosition.Text = $"Lat/Lng: {pos.lat:0.###},{pos.lng:0.###} - Alt: {pos.alt:0}";
                CurrentUnit.Text = $"{ViewModel.ClientState?.DcsPlayerRadioInfo?.unit}";
            }
            catch { }

            ConnectedClientsSingleton.Instance.NotifyAll();

        }

        private void InitSettingsScreen()
        {
            AutoConnectEnabledToggle.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnect);
            AutoConnectPromptToggle.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnectPrompt);
            AutoConnectMismatchPromptToggle.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnectMismatchPrompt);
            RadioOverlayTaskbarItem.IsChecked =
                ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
            RefocusDcs.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RefocusDCS);
            ExpandInputDevices.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.ExpandControls);

            MinimiseToTray.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.MinimiseToTray);
            StartMinimised.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.StartMinimised);

            MicAutoGainControl.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AGC);
            MicDenoise.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.Denoise);

            CheckForBetaUpdates.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.CheckForBetaUpdates);
            PlayConnectionSounds.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds);

            RequireAdminToggle.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RequireAdmin);

            AutoSelectInputProfile.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoSelectSettingsProfile);

            VaicomtxInhibitEnabled.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.VAICOMTXInhibitEnabled);

            ShowTransmitterName.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.ShowTransmitterName);

            AllowTransmissionsRecord.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AllowRecording);
            RecordTransmissions.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RecordAudio);
            SingleFileMixdown.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.SingleFileMixdown);
            DisallowedAudioTone.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.DisallowedAudioTone);
            RecordTransmissions_IsEnabled();

            RecordingQuality.IsEnabled = false;
            RecordingQuality.ValueChanged += RecordingQuality_ValueChanged;
            RecordingQuality.Value = double.Parse(
                ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.RecordingQuality).StringValue[1].ToString());
            RecordingQuality.IsEnabled = true;

            var objValue = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\DCS-SR-Standalone", "SRSAnalyticsOptOut", "FALSE");
            if (objValue == null || (string)objValue == "TRUE")
            {
                AllowAnonymousUsage.IsChecked = false;
            }
            else
            {
                AllowAnonymousUsage.IsChecked = true;
            }

            VoxEnabled.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.VOX);

            VoxMode.IsEnabled = false;
            VoxMode.Value =
                ViewModel.GlobalSettings.GetClientSettingInt(GlobalSettingsKeys.VOXMode);
            VoxMode.ValueChanged += VOXMode_ValueChanged;
            VoxMode.IsEnabled = true;

            VoxMinimimumTxTime.IsEnabled = false;
            VoxMinimimumTxTime.Value =
                ViewModel.GlobalSettings.GetClientSettingInt(GlobalSettingsKeys.VOXMinimumTime);
            VoxMinimimumTxTime.ValueChanged += VOXMinimumTime_ValueChanged;
            VoxMinimimumTxTime.IsEnabled = true;

            VoxMinimumRms.IsEnabled = false;
            VoxMinimumRms.Value =
                ViewModel.GlobalSettings.GetClientSettingDouble(GlobalSettingsKeys.VOXMinimumDB);
            VoxMinimumRms.ValueChanged += VOXMinimumRMS_ValueChanged;
            VoxMinimumRms.IsEnabled = true;

            AllowXInputController.IsChecked = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AllowXInputController);
        }

        private void ReloadProfileSettings()
        {
            RadioEncryptionEffectsToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioEncryptionEffects);
            RadioSwitchIsPtt.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTT);

            RadioTxStartToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_Start);
            RadioTxEndToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_End);

            RadioRxStartToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_Start);
            RadioRxEndToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_End);

            RadioMidsToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.MIDSRadioEffect);

            RadioSoundEffects.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioEffects);
            RadioSoundEffectsClipping.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RadioEffectsClipping);
            NatoRadioToneToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.NATOTone);
            HqEffectToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.HAVEQUICKTone);
            BackgroundRadioNoiseToggle.IsChecked =
                ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(
                    ProfileSettingsKeys.RadioBackgroundNoiseEffect);

            AutoSelectChannel.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AutoSelectPresetChannel);

            AlwaysAllowHotas.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AlwaysAllowHotasControls);
            AllowDcsPtt.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AllowDCSPTT);
            AllowRotaryIncrement.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.RotaryStyleIncrement);
            AlwaysAllowTransponderOverlay.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AlwaysAllowTransponderOverlay);

            //disable to set without triggering onchange
            PttReleaseDelay.IsEnabled = false;
            PttReleaseDelay.ValueChanged += PushToTalkReleaseDelay_ValueChanged;
            PttReleaseDelay.Value =
                ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.PTTReleaseDelay);
            PttReleaseDelay.IsEnabled = true;

            DisableExpansionRadios.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.DisableExpansionRadios);

            PttStartDelay.IsEnabled = false;
            PttStartDelay.ValueChanged += PushToTalkStartDelay_ValueChanged;
            PttStartDelay.Value =
                ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.PTTStartDelay);
            PttStartDelay.IsEnabled = true;

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

            NatoToneVolume.IsEnabled = false;
            NatoToneVolume.ValueChanged += (sender, e) =>
            {
                if (NatoToneVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.NATOToneVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.NATOToneVolume, (float)vol);
                }

            };
            NatoToneVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.NATOToneVolume)
                                    / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.NATOToneVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            NatoToneVolume.IsEnabled = true;

            HqToneVolume.IsEnabled = false;
            HqToneVolume.ValueChanged += (sender, e) =>
            {
                if (HqToneVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.HQToneVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.HQToneVolume, (float)vol);
                }

            };
            HqToneVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.HQToneVolume)
                                  / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.HQToneVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            HqToneVolume.IsEnabled = true;

            FmEffectVolume.IsEnabled = false;
            FmEffectVolume.ValueChanged += (sender, e) =>
            {
                if (FmEffectVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.FMNoiseVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.FMNoiseVolume, (float)vol);
                }

            };
            FmEffectVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.FMNoiseVolume)
                                    / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.FMNoiseVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            FmEffectVolume.IsEnabled = true;

            VhfEffectVolume.IsEnabled = false;
            VhfEffectVolume.ValueChanged += (sender, e) =>
            {
                if (VhfEffectVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.VHFNoiseVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.VHFNoiseVolume, (float)vol);
                }

            };
            VhfEffectVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.VHFNoiseVolume)
                                     / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.VHFNoiseVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            VhfEffectVolume.IsEnabled = true;

            UhfEffectVolume.IsEnabled = false;
            UhfEffectVolume.ValueChanged += (sender, e) =>
            {
                if (UhfEffectVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.UHFNoiseVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.UHFNoiseVolume, (float)vol);
                }

            };
            UhfEffectVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.UHFNoiseVolume)
                                     / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.UHFNoiseVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            UhfEffectVolume.IsEnabled = true;

            HfEffectVolume.IsEnabled = false;
            HfEffectVolume.ValueChanged += (sender, e) =>
            {
                if (HfEffectVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.HFNoiseVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.HFNoiseVolume, (float)vol);
                }

            };
            HfEffectVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.HFNoiseVolume)
                                    / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.HFNoiseVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            HfEffectVolume.IsEnabled = true;


            AmbientCockpitEffectToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AmbientCockpitNoiseEffect);
            AmbientIntercomEffectToggle.IsChecked = ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingBool(ProfileSettingsKeys.AmbientCockpitIntercomNoiseEffect);

            AmbientCockpitEffectVolume.IsEnabled = false;
            AmbientCockpitEffectVolume.ValueChanged += (sender, e) =>
            {
                if (AmbientCockpitEffectVolume.IsEnabled)
                {
                    var orig = double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume.ToString()], CultureInfo.InvariantCulture);

                    var vol = orig * (e.NewValue / 100);

                    ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume, (float)vol);
                }

            };
            AmbientCockpitEffectVolume.Value = (ViewModel.GlobalSettings.ProfileSettingsStore.GetClientSettingFloat(ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume)
                                         / double.Parse(ProfileSettingsStore.DefaultSettingsProfileSettings[ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume.ToString()], CultureInfo.InvariantCulture)) * 100;
            AmbientCockpitEffectVolume.IsEnabled = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect()
        {
            if (ViewModel.ClientState.IsConnected)
            {
                Stop();
            }
            else
            {
                SaveSelectedInputAndOutput();

                try
                {
                    //process hostname
                    var resolvedAddresses = Dns.GetHostAddresses(GetAddressFromTextBox());
                    var ip = resolvedAddresses.FirstOrDefault(xa => xa.AddressFamily == AddressFamily.InterNetwork); // Ensure we get an IPv4 address in case the host resolves to both IPv6 and IPv4

                    if (ip != null)
                    {
                        ViewModel.ResolvedIp = ip;
                        ViewModel.Port = GetPortFromTextBox();

                        try
                        {
                            ViewModel.Client.Disconnect();
                        }
                        catch (Exception ex)
                        {
                        }

                        if (ViewModel.Client == null)
                        {
                            ViewModel.Client = new SRSClientSyncHandler(ViewModel.Guid, UpdateUICallback, delegate(string name, int seat)
                            {
                                try
                                {
                                    //on MAIN thread
                                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                        new ThreadStart(() =>
                                        {
                                            //Handle Aircraft Name - find matching profile and select if you can
                                            name = Regex.Replace(name.Trim().ToLower(), "[^a-zA-Z0-9]", "");
                                            //add one to seat so seat_2 is copilot
                                            var nameSeat = $"_{seat + 1}";

                                            foreach (var profileName in ViewModel.GlobalSettings.ProfileSettingsStore
                                                         .ProfileNames)
                                            {
                                                //find matching seat
                                                var splitName = profileName.Trim().ToLowerInvariant().Split('_')
                                                    .First();
                                                if (name.StartsWith(Regex.Replace(splitName, "[^a-zA-Z0-9]", "")) &&
                                                    profileName.Trim().EndsWith(nameSeat))
                                                {
                                                    ControlsProfile.SelectedItem = profileName;
                                                    return;
                                                }
                                            }

                                            foreach (var profileName in ViewModel.GlobalSettings.ProfileSettingsStore
                                                         .ProfileNames)
                                            {
                                                //find matching seat
                                                if (name.StartsWith(Regex.Replace(profileName.Trim().ToLower(),
                                                        "[^a-zA-Z0-9_]", "")))
                                                {
                                                    ControlsProfile.SelectedItem = profileName;
                                                    return;
                                                }
                                            }

                                            ControlsProfile.SelectedIndex = 0;

                                        }));
                                }
                                catch (Exception)
                                {
                                }

                            });
                        }

                        ViewModel.Client.TryConnect(new IPEndPoint(ViewModel.ResolvedIp, ViewModel.Port), ConnectCallback);

                        StartStop.Content = Properties.Resources.StartStopConnecting;
                        StartStop.IsEnabled = false;
                        Mic.IsEnabled = false;
                        Speakers.IsEnabled = false;
                        MicOutput.IsEnabled = false;
                        Preview.IsEnabled = false;

                        if (ViewModel.AudioPreview != null)
                        {
                            Preview.Content = Properties.Resources.PreviewAudio;
                            ViewModel.AudioPreview.StopEncoding();
                            ViewModel.AudioPreview = null;
                        }
                    }
                    else
                    {
                        //invalid ID
                        MessageBox.Show(Properties.Resources.MsgBoxInvalidIPText, Properties.Resources.MsgBoxInvalidIP, MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        ViewModel.ClientState.IsConnected = false;
                        ToggleServerSettings.IsEnabled = false;
                    }
                }
                catch (Exception ex) when (ex is SocketException || ex is ArgumentException)
                {
                    MessageBox.Show(Properties.Resources.MsgBoxInvalidIPText, Properties.Resources.MsgBoxInvalidIP, MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    ViewModel.ClientState.IsConnected = false;
                    ToggleServerSettings.IsEnabled = false;
                }
            }
        }

        private string GetAddressFromTextBox()
        {
            var addr = ServerIp.Text.Trim();

            if (addr.Contains(":"))
            {
                return addr.Split(':')[0];
            }

            return addr;
        }

        private int GetPortFromTextBox()
        {
            var addr = ServerIp.Text.Trim();

            if (addr.Contains(":"))
            {
                int port;
                if (int.TryParse(addr.Split(':')[1], out port))
                {
                    return port;
                }
                throw new ArgumentException("specified port is not valid");
            }

            return 5002;
        }

        private void Stop(bool connectionError = false)
        {
            if (ViewModel.ClientState.IsConnected && ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds))
            {
                try
                {
                    Sounds.BeepDisconnected.Play();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to play disconnect sound");
                }
            }

            ViewModel.ClientState.IsConnectionErrored = connectionError;

            StartStop.Content = Properties.Resources.StartStop;
            StartStop.IsEnabled = true;
            Mic.IsEnabled = true;
            Speakers.IsEnabled = true;
            MicOutput.IsEnabled = true;
            Preview.IsEnabled = true;
            ViewModel.ClientState.IsConnected = false;
            ToggleServerSettings.IsEnabled = false;

            ConnectExternalAwacsMode.IsEnabled = false;
            ConnectExternalAwacsMode.Content = Properties.Resources.ConnectExternalAWACSMode;

            if (!string.IsNullOrWhiteSpace(ViewModel.ClientState.LastSeenName) &&
                ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastSeenName).StringValue != ViewModel.ClientState.LastSeenName)
            {
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastSeenName, ViewModel.ClientState.LastSeenName);
            }

            try
            {
                ViewModel.AudioManager.StopEncoding();
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                ViewModel.Client.Disconnect();
            }
            catch (Exception)
            {
                // ignored
            }

            ViewModel.ClientState.DcsPlayerRadioInfo.Reset();
            ViewModel.ClientState.PlayerCoaltionLocationMetadata.Reset();
        }

        private void SaveSelectedInputAndOutput()
        {
            //save app settings
            // Only save selected microphone if one is actually available, resulting in a crash otherwise
            if (ViewModel.AudioInput.MicrophoneAvailable)
            {
                if (ViewModel.AudioInput.SelectedAudioInput.Value == null)
                {
                    ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AudioInputDeviceId, "default");

                }
                else
                {
                    var input = ((MMDevice)ViewModel.AudioInput.SelectedAudioInput.Value).ID;
                    ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AudioInputDeviceId, input);
                }
            }

            if (ViewModel.AudioOutput.SelectedAudioOutput.Value == null)
            {
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId, "default");
            }
            else
            {
                var output = (MMDevice)ViewModel.AudioOutput.SelectedAudioOutput.Value;
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId, output.ID);
            }

            //check if we have optional output
            if (ViewModel.AudioOutput.SelectedMicAudioOutput.Value != null)
            {
                var micOutput = (MMDevice)ViewModel.AudioOutput.SelectedMicAudioOutput.Value;
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId, micOutput.ID);
            }
            else
            {
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId, "");
            }

            ShowMicPassthroughWarning();
        }

        private void ShowMicPassthroughWarning()
        {
            if (ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId).RawValue
                .Equals(ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId).RawValue))
            {
                MessageBox.Show(Properties.Resources.MsgBoxMicPassthruText, Properties.Resources.MsgBoxMicPassthru, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ConnectCallback(bool result, bool connectionError, string connection)
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

                        if (ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds))
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

                        ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastServer, ServerIp.Text);

                        ViewModel.AudioManager.StartEncoding(ViewModel.Guid, InputManager,
                            ViewModel.ResolvedIp, ViewModel.Port);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex,
                            "Unable to get audio device - likely output device error - Pick another. Error:" +
                            ex.Message);
                        Stop();

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
                Stop(connectionError);
            }
            else
            {
                if (!ViewModel.ClientState.IsConnected)
                {
                    Stop(connectionError);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.ClientX, Left);
            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.ClientY, Top);

            if (!string.IsNullOrWhiteSpace(ViewModel.ClientState.LastSeenName) &&
                ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastSeenName).StringValue != ViewModel.ClientState.LastSeenName)
            {
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastSeenName, ViewModel.ClientState.LastSeenName);
            }

            //save window position
            base.OnClosing(e);
            
            Stop();

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
            if (WindowState == WindowState.Minimized && ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.MinimiseToTray))
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

        private void UpdateUICallback()
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

        private void SpeakerBoost_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var convertedValue = VolumeConversionHelper.ConvertVolumeSliderToScale((float)SpeakerBoost.Value);

            if (ViewModel.AudioPreview != null)
            {
                ViewModel.AudioPreview.SpeakerBoost = convertedValue;
            }

            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.SpeakerBoost,
                SpeakerBoost.Value.ToString(CultureInfo.InvariantCulture));

            if ((SpeakerBoostLabel != null) && (SpeakerBoost != null))
            {
                SpeakerBoostLabel.Content = VolumeConversionHelper.ConvertLinearDiffToDB(convertedValue);
            }
        }

        private void RadioEncryptionEffects_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioEncryptionEffects,
                (bool)RadioEncryptionEffectsToggle.IsChecked);
        }

        private void NATORadioTone_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.NATOTone,
                (bool)NatoRadioToneToggle.IsChecked);
        }

        private void RadioSwitchPTT_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTT, (bool)RadioSwitchIsPtt.IsChecked);
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


                        _radioOverlayWindow.ShowInTaskbar =
                            !ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
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
                _awacsRadioOverlay.ShowInTaskbar =
                    !ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
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

            var enabled = ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.AutoConnect).BoolValue;

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
                // Show auto connect prompt if client is not connected yet and setting has been enabled, otherwise automatically connect
                bool showPrompt = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnectPrompt);

                bool connectToServer = !showPrompt;
                if (ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnectPrompt))
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
                    Connect();
                }
            }
        }

        private async void HandleAutoConnectMismatch(string currentConnection, string advertisedConnection)
        {
            // Show auto connect mismatch prompt if setting has been enabled (default), otherwise automatically switch server
            bool showPrompt = ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.AutoConnectMismatchPrompt);

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
                Stop();

                StartStop.IsEnabled = false;
                StartStop.Content = Properties.Resources.StartStopConnecting;
                await Task.Delay(2000);
                StartStop.IsEnabled = true;
                ServerIp.Text = advertisedConnection;
                Connect();
            }
        }

        private void ResetRadioWindow_Click(object sender, RoutedEventArgs e)
        {
            //close overlay
            _radioOverlayWindow?.Close();
            _radioOverlayWindow = null;

            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioX, 300);
            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioY, 300);

            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioWidth, 122);
            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioHeight, 270);

            ViewModel.GlobalSettings.SetPositionSetting(GlobalSettingsKeys.RadioOpacity, 1.0);
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



        private void AutoConnectToggle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AutoConnect, (bool)AutoConnectEnabledToggle.IsChecked);
        }

        private void AutoConnectPromptToggle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AutoConnectPrompt, (bool)AutoConnectPromptToggle.IsChecked);
        }

        private void AutoConnectMismatchPromptToggle_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AutoConnectMismatchPrompt, (bool)AutoConnectMismatchPromptToggle.IsChecked);
        }

        private void RadioOverlayTaskbarItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.RadioOverlayTaskbarHide, (bool)RadioOverlayTaskbarItem.IsChecked);

            if (_radioOverlayWindow != null)
                _radioOverlayWindow.ShowInTaskbar = !ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
            else if (_awacsRadioOverlay != null) _awacsRadioOverlay.ShowInTaskbar = !ViewModel.GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
        }

        private void DCSRefocus_OnClick_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.RefocusDCS, (bool)RefocusDcs.IsChecked);
        }

        private void ExpandInputDevices_OnClick_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                Properties.Resources.MsgBoxRestartExpandText,
                Properties.Resources.MsgBoxRestart, MessageBoxButton.OK,
                MessageBoxImage.Warning);

            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.ExpandControls, (bool)ExpandInputDevices.IsChecked);
        }

        private void AllowXInputController_OnClick_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                Properties.Resources.MsgBoxRestartXInputText,
                Properties.Resources.MsgBoxRestart, MessageBoxButton.OK,
                MessageBoxImage.Warning);

            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AllowXInputController, (bool)AllowXInputController.IsChecked);
        }

        private void LaunchAddressTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedItem = FavouritesSeversTab;
        }

        private void MicAGC_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AGC, (bool)MicAutoGainControl.IsChecked);
        }

        private void MicDenoise_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.Denoise, (bool)MicDenoise.IsChecked);
        }

        private void RadioSoundEffects_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioEffects,
                (bool)RadioSoundEffects.IsChecked);
        }

        private void RadioTxStart_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_Start, (bool)RadioTxStartToggle.IsChecked);
        }

        private void RadioTxEnd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_End, (bool)RadioTxEndToggle.IsChecked);
        }

        private void RadioRxStart_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_Start, (bool)RadioRxStartToggle.IsChecked);
        }

        private void RadioRxEnd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_End, (bool)RadioRxEndToggle.IsChecked);
        }

        private void RadioMIDS_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.MIDSRadioEffect, (bool)RadioMidsToggle.IsChecked);
        }

        private void AudioSelectChannel_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AutoSelectPresetChannel, (bool)AutoSelectChannel.IsChecked);
        }

        private void RadioSoundEffectsClipping_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioEffectsClipping,
                (bool)RadioSoundEffectsClipping.IsChecked);

        }

        private void MinimiseToTray_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.MinimiseToTray, (bool)MinimiseToTray.IsChecked);
        }

        private void StartMinimised_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.StartMinimised, (bool)StartMinimised.IsChecked);
        }

        private void AllowDCSPTT_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AllowDCSPTT, (bool)AllowDcsPtt.IsChecked);
        }

        private void AllowRotaryIncrement_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RotaryStyleIncrement, (bool)AllowRotaryIncrement.IsChecked);
        }

        private void AlwaysAllowHotas_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AlwaysAllowHotasControls, (bool)AlwaysAllowHotas.IsChecked);
        }

        private void CheckForBetaUpdates_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.CheckForBetaUpdates, (bool)CheckForBetaUpdates.IsChecked);
        }

        private void PlayConnectionSounds_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.PlayConnectionSounds, (bool)PlayConnectionSounds.IsChecked);
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
                ViewModel.ClientState.LastSeenName = ExternalAwacsModeName.Text;
                ViewModel.Client.ConnectExternalAWACSMode(ExternalAwacsModePassword.Password.Trim(), ExternalAWACSModeConnectionChanged);
            }
        }

        private void ExternalAWACSModeConnectionChanged(bool result, int coalition)
        {
            if (result)
            {
                ViewModel.ClientState.ExternalAWACSModelSelected = true;
                ViewModel.ClientState.PlayerCoaltionLocationMetadata.side = coalition;
                ViewModel.ClientState.PlayerCoaltionLocationMetadata.name = ViewModel.ClientState.LastSeenName;
                ViewModel.ClientState.DcsPlayerRadioInfo.name = ViewModel.ClientState.LastSeenName;

                ConnectExternalAwacsMode.Content = Properties.Resources.DisconnectExternalAWACSMode;
            }
            else
            {
                ViewModel.ClientState.ExternalAWACSModelSelected = false;
                ViewModel.ClientState.PlayerCoaltionLocationMetadata.side = 0;
                ViewModel.ClientState.PlayerCoaltionLocationMetadata.name = "";
                ViewModel.ClientState.DcsPlayerRadioInfo.name = "";
                ViewModel.ClientState.DcsPlayerRadioInfo.LastUpdate = 0;
                ViewModel.ClientState.LastSent = 0;

                ConnectExternalAwacsMode.Content = Properties.Resources.ConnectExternalAWACSMode;
                ExternalAwacsModePassword.IsEnabled = ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);
                ExternalAwacsModeName.IsEnabled = ViewModel.ServerSettings.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);
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
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.RequireAdmin, (bool)RequireAdminToggle.IsChecked);
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
                        ViewModel.GlobalSettings.ProfileSettingsStore.AddNewProfile(name);
                        InitSettingsProfiles();

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
                    ControlsProfile.SelectedIndex = 0;
                    ViewModel.GlobalSettings.ProfileSettingsStore.RemoveProfile(current);
                    InitSettingsProfiles();
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
                        ViewModel.GlobalSettings.ProfileSettingsStore.RenameProfile(oldName, name);
                        InitSettingsProfiles();
                    }
                }, true, oldName);
                inputProfileWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                inputProfileWindow.Owner = this;
                inputProfileWindow.ShowDialog();
            }

        }

        private void UpdatePresetsFolderLabel()
        {
            var presetsFolder = ViewModel.GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastPresetsFolder).RawValue;
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

        private void AutoSelectInputProfile_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AutoSelectSettingsProfile, ((bool)AutoSelectInputProfile.IsChecked).ToString());
        }

        private void CopyProfile(object sender, RoutedEventArgs e)
        {
            var current = ControlsProfile.SelectedValue as string;
            var inputProfileWindow = new InputProfileWindow.InputProfileWindow(name =>
            {
                if (name.Trim().Length > 0)
                {
                    ViewModel.GlobalSettings.ProfileSettingsStore.CopyProfile(current, name);
                    InitSettingsProfiles();
                }
            });
            inputProfileWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inputProfileWindow.Owner = this;
            inputProfileWindow.ShowDialog();
        }

        private void VAICOMTXInhibit_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.VAICOMTXInhibitEnabled, ((bool)VaicomtxInhibitEnabled.IsChecked).ToString());
        }

        private void AlwaysAllowTransponderOverlay_OnClick(object sender, RoutedEventArgs e)
        {

            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AlwaysAllowTransponderOverlay, (bool)AlwaysAllowTransponderOverlay.IsChecked);
        }

        private void CurrentPosition_OnClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var pos = ViewModel.ClientState.PlayerCoaltionLocationMetadata.LngLngPosition;

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

        private void ShowTransmitterName_OnClick_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.ShowTransmitterName, ((bool)ShowTransmitterName.IsChecked).ToString());
        }

        private void PushToTalkReleaseDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PttReleaseDelay.IsEnabled)
                ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.PTTReleaseDelay, (float)e.NewValue);
        }

        private void PushToTalkStartDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PttStartDelay.IsEnabled)
                ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingFloat(ProfileSettingsKeys.PTTStartDelay, (float)e.NewValue);
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

        private void BackgroundRadioNoiseToggle_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.RadioBackgroundNoiseEffect, (bool)BackgroundRadioNoiseToggle.IsChecked);
        }

        private void HQEffect_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.HAVEQUICKTone, (bool)HqEffectToggle.IsChecked);
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

        private void AllowTransmissionsRecord_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.AllowRecording, (bool)AllowTransmissionsRecord.IsChecked);
        }

        private void RecordTransmissions_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.RecordAudio, (bool)RecordTransmissions.IsChecked);
            RecordTransmissions_IsEnabled();
        }

        private void RecordTransmissions_IsEnabled()
        {
            if ((bool)RecordTransmissions.IsChecked)
            {
                SingleFileMixdown.IsEnabled = false;
                RecordingQuality.IsEnabled = false;
            }
            else
            {
                SingleFileMixdown.IsEnabled = true;
                RecordingQuality.IsEnabled = true;
            }
        }

        private void SingleFileMixdown_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.SingleFileMixdown, (bool)SingleFileMixdown.IsChecked);
        }

        private void RecordingQuality_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.RecordingQuality, $"V{(int)e.NewValue}");
        }

        private void DisallowedAudioTone_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.DisallowedAudioTone, (bool)DisallowedAudioTone.IsChecked);
        }

        private void VoxEnabled_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.VOX, (bool)VoxEnabled.IsChecked);
        }

        private void VOXMode_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(VoxMode.IsEnabled)
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.VOXMode, (int)e.NewValue);
        }

        private void VOXMinimumTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VoxMinimimumTxTime.IsEnabled)
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.VOXMinimumTime, (int)e.NewValue);
        }

        private void VOXMinimumRMS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VoxMinimumRms.IsEnabled)
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.VOXMinimumDB, (double)e.NewValue);
        }

        private void AmbientCockpitEffectToggle_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AmbientCockpitNoiseEffect, (bool)AmbientCockpitEffectToggle.IsChecked);
        }

        private void AmbientCockpitEffectIntercomToggle_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.AmbientCockpitIntercomNoiseEffect, (bool)AmbientIntercomEffectToggle.IsChecked);
        }

        private void DisableExpansionRadios_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.ProfileSettingsStore.SetClientSettingBool(ProfileSettingsKeys.DisableExpansionRadios, (bool)DisableExpansionRadios.IsChecked);
        }

        private void PresetsFolderBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var selectPresetsFolder = new System.Windows.Forms.FolderBrowserDialog();
            selectPresetsFolder.SelectedPath = PresetsFolderLabel.ToolTip.ToString();
            if (selectPresetsFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastPresetsFolder, selectPresetsFolder.SelectedPath);
                UpdatePresetsFolderLabel();
            }
        }

        private void PresetsFolderResetButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastPresetsFolder, string.Empty);
            UpdatePresetsFolderLabel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // Special Case Binding
                ViewModel.ServerAddress.EAMCoalitionPassword = ((PasswordBox)sender).Password;
            } 
        }
    }
}