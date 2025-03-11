using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
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
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network.DCS;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Preferences;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Recording;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.ClientList;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.Favourites;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.InputProfileWindow;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Utils;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Helpers;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Overlay;
using DCS_SR_Client.ViewModels;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Dmo;
using NAudio.Wave;
using NLog;
using WPFCustomMessageBox;
using InputBinding = Ciribob.DCS.SimpleRadio.Standalone.Client.Settings.InputBinding;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	
	public delegate void ReceivedAutoConnect(string address, int port);

	public delegate void ToggleOverlayCallback(bool uiButton, bool awacs);

	public readonly AudioManager _audioManager;

	public readonly string _guid;
	private readonly Logger Logger = LogManager.GetCurrentClassLogger();
	public AudioPreview _audioPreview;
	public SRSClientSyncHandler _client;
	public DCSAutoConnectHandler _dcsAutoConnectListener;
	public int _port = 5002;

	public Overlay.RadioOverlayWindow _radioOverlayWindow;
	public AwacsRadioOverlayWindow.RadioOverlayWindow _awacsRadioOverlay;

	public IPAddress _resolvedIp;
	public ServerSettingsWindow _serverSettingsWindow;

	public ClientListWindow _clientListWindow;

	//used to debounce toggle
	public long _toggleShowHide;

	public readonly DispatcherTimer _updateTimer;
	public ServerAddress _serverAddress;
	public readonly DelegateCommand _connectCommand;

	public readonly GlobalSettingsStore _globalSettings = GlobalSettingsStore.Instance;

	/// <remarks>Used in the XAML for DataBinding many things</remarks>
	public ClientStateSingleton ClientState { get; } = ClientStateSingleton.Instance;

	/// <remarks>Used in the XAML for DataBinding the connected client count</remarks>
	public ConnectedClientsSingleton Clients { get; } = ConnectedClientsSingleton.Instance;

	/// <remarks>Used in the XAML for DataBinding input audio related UI elements</remarks>
	public AudioInputSingleton AudioInput { get; } = AudioInputSingleton.Instance;

	/// <remarks>Used in the XAML for DataBinding output audio related UI elements</remarks>
	public AudioOutputSingleton AudioOutput { get; } = AudioOutputSingleton.Instance;

	public readonly SyncedServerSettings _serverSettings = SyncedServerSettings.Instance;


	public void MainWindowViewModel()
	{
		
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            var client = ClientStateSingleton.Instance;

            _guid = ClientStateSingleton.Instance.ShortGUID;
            Analytics.Log("Client", "Startup", _globalSettings.GetClientSetting(GlobalSettingsKeys.ClientIdLong).RawValue);

            InitSettingsScreen();

            InitSettingsProfiles();
            ReloadProfile();

            InitInput();
            InputManager = new InputDeviceManager(this, ToggleOverlay);

            _connectCommand = new DelegateCommand(Connect, () => ServerAddress != null);
            FavouriteServersViewModel = new FavouriteServersViewModel(new CsvFavouriteServerStore());

            InitDefaultAddress();


            UpdatePresetsFolderLabel();

            _audioManager = new AudioManager(AudioOutput.WindowsN);
            _audioManager.SpeakerBoost = VolumeConversionHelper.ConvertVolumeSliderToScale((float)SpeakerBoost.Value);

            if ((SpeakerBoostLabel != null) && (SpeakerBoost != null))
            {
                SpeakerBoostLabel.Content = VolumeConversionHelper.ConvertLinearDiffToDB(_audioManager.SpeakerBoost);
            }

            UpdaterChecker.CheckForUpdate(_globalSettings.GetClientSettingBool(GlobalSettingsKeys.CheckForBetaUpdates));

            InitFlowDocument();

            _dcsAutoConnectListener = new DCSAutoConnectHandler(AutoConnect);

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _updateTimer.Tick += UpdatePlayerLocationAndVUMeters;
            _updateTimer.Start();
	}
}