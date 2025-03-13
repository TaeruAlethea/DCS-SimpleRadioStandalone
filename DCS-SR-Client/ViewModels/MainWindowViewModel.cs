using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network.DCS;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Preferences;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.Favourites;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	[Obsolete("MainWindow Reference is not MVVM compliant.")]
	public MainWindow ToBeDepricatedMainWindow { get; init; }
	
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	
	[ObservableProperty] private AudioManager _audioManager;
	[ObservableProperty] private AudioPreview _audioPreview;
	
	[ObservableProperty] private ClientStateSingleton _clientState = ClientStateSingleton.Instance;
	[ObservableProperty] private ConnectedClientsSingleton _clients = ConnectedClientsSingleton.Instance;
	[ObservableProperty] private SRSClientSyncHandler _client;
	[ObservableProperty] private DCSAutoConnectHandler _dcsAutoConnectListener;
	
	private readonly DispatcherTimer _updateTimer;
	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
	private ServerAddress _serverAddress;
	[ObservableProperty] private IPAddress _resolvedIp;
	[ObservableProperty] private string _guid; // Does not need to be observable.
	[ObservableProperty] private int _port = 5002;
	
	/// <remarks>Used in the XAML for DataBinding many things</remarks>
	public AudioInputSingleton AudioInput { get; } = AudioInputSingleton.Instance;
	/// <remarks>Used in the XAML for DataBinding output audio related UI elements</remarks>
	public AudioOutputSingleton AudioOutput { get; } = AudioOutputSingleton.Instance;

	public FavouriteServersViewModel FavouriteServersViewModel { get; }
	
	[ObservableProperty]
	private SyncedServerSettings _serverSettings = SyncedServerSettings.Instance;
	
	[ObservableProperty] private GlobalSettingsStore _globalSettings = GlobalSettingsStore.Instance;
	[ObservableProperty] private GlobalSettingsModel _globalSettingsProperties;
	
	public MainWindowViewModel(MainWindow mainWindowView)
	{
		ToBeDepricatedMainWindow = mainWindowView;
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		FavouriteServersViewModel = new FavouriteServersViewModel(new CsvFavouriteServerStore());
		GlobalSettingsProperties = new GlobalSettingsModel(GlobalSettings);
		
		_audioManager = new AudioManager(AudioOutput.WindowsN);
		Guid = ClientStateSingleton.Instance.ShortGUID;
		
		InitDefaultAddress();
		
		AudioManager.SpeakerBoost = GlobalSettings.GetClientSetting(GlobalSettingsKeys.SpeakerBoost).FloatValue;
		
		DcsAutoConnectListener = new DCSAutoConnectHandler(ToBeDepricatedMainWindow.AutoConnect);
		
		_updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
		_updateTimer.Tick += ToBeDepricatedMainWindow.UpdateVUMeters;
		_updateTimer.Start();
	}

	[RelayCommand]
	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Connect()
	{
	
        if (ClientState.IsConnected)
        {
            Stop();
        }
        else
        {
            ToBeDepricatedMainWindow.SaveSelectedInputAndOutput();

            try
            {
                //process hostname
                var resolvedAddresses = Dns.GetHostAddresses(ServerAddress.HostName);
                var ip = resolvedAddresses.FirstOrDefault(xa => xa.AddressFamily == AddressFamily.InterNetwork); // Ensure we get an IPv4 address in case the host resolves to both IPv6 and IPv4

                if (ip != null)
                {
                    ResolvedIp = ip;
                    Port = ServerAddress.Port;

                    try
                    {
                        Client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                    }

                    if (Client == null)
                    {
                        Client = new SRSClientSyncHandler(Guid, ToBeDepricatedMainWindow.UpdateUICallback, delegate(string name, int seat)
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

                                        foreach (var profileName in GlobalSettings.ProfileSettingsStore
                                                     .ProfileNames)
                                        {
                                            //find matching seat
                                            var splitName = profileName.Trim().ToLowerInvariant().Split('_')
                                                .First();
                                            if (name.StartsWith(Regex.Replace(splitName, "[^a-zA-Z0-9]", "")) &&
                                                profileName.Trim().EndsWith(nameSeat))
                                            {
                                                ToBeDepricatedMainWindow.ControlsProfile.SelectedItem = profileName;
                                                return;
                                            }
                                        }

                                        foreach (var profileName in GlobalSettings.ProfileSettingsStore
                                                     .ProfileNames)
                                        {
                                            //find matching seat
                                            if (name.StartsWith(Regex.Replace(profileName.Trim().ToLower(),
                                                    "[^a-zA-Z0-9_]", "")))
                                            {
                                                ToBeDepricatedMainWindow.ControlsProfile.SelectedItem = profileName;
                                                return;
                                            }
                                        }

                                        ToBeDepricatedMainWindow.ControlsProfile.SelectedIndex = 0;

                                    }));
                            }
                            catch (Exception)
                            {
                            }

                        });
                    }

                    Client.TryConnect(new IPEndPoint(ResolvedIp, Port), ToBeDepricatedMainWindow.ConnectCallback);

                    ToBeDepricatedMainWindow.StartStop.Content = Properties.Resources.StartStopConnecting;
                    ToBeDepricatedMainWindow.StartStop.IsEnabled = false;
                    ToBeDepricatedMainWindow.Mic.IsEnabled = false;
                    ToBeDepricatedMainWindow.Speakers.IsEnabled = false;
                    ToBeDepricatedMainWindow.MicOutput.IsEnabled = false;
                    ToBeDepricatedMainWindow.Preview.IsEnabled = false;

                    if (AudioPreview != null)
                    {
                        ToBeDepricatedMainWindow.Preview.Content = Properties.Resources.PreviewAudio;
                        AudioPreview.StopEncoding();
                        AudioPreview = null;
                    }
                }
                else
                {
                    //invalid ID
                    MessageBox.Show(Properties.Resources.MsgBoxInvalidIPText, Properties.Resources.MsgBoxInvalidIP, MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    ClientState.IsConnected = false;
                    ToBeDepricatedMainWindow.ToggleServerSettings.IsEnabled = false;
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is ArgumentException)
            {
                MessageBox.Show(Properties.Resources.MsgBoxInvalidIPText, Properties.Resources.MsgBoxInvalidIP, MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ClientState.IsConnected = false;
                ToBeDepricatedMainWindow.ToggleServerSettings.IsEnabled = false;
            }
        }
	}
	
	public void Stop(bool connectionError = false)
	{
		if (ClientState.IsConnected && GlobalSettings.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds))
		{
			try
			{
				Sounds.BeepDisconnected.Play();
			}
			catch (Exception ex)
			{
				_logger.Warn(ex, "Failed to play disconnect sound");
			}
		}

		ClientState.IsConnectionErrored = connectionError;

		ToBeDepricatedMainWindow.StartStop.Content = Properties.Resources.StartStop;
		ToBeDepricatedMainWindow.StartStop.IsEnabled = true;
		ToBeDepricatedMainWindow.Mic.IsEnabled = true;
		ToBeDepricatedMainWindow.Speakers.IsEnabled = true;
		ToBeDepricatedMainWindow.MicOutput.IsEnabled = true;
		ToBeDepricatedMainWindow.Preview.IsEnabled = true;
		ClientState.IsConnected = false;
		ToBeDepricatedMainWindow.ToggleServerSettings.IsEnabled = false;

		ToBeDepricatedMainWindow.ConnectExternalAwacsMode.IsEnabled = false;
		ToBeDepricatedMainWindow.ConnectExternalAwacsMode.Content = Properties.Resources.ConnectExternalAWACSMode;

		if (!string.IsNullOrWhiteSpace(ClientState.LastSeenName) &&
		    GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastSeenName).StringValue != ClientState.LastSeenName)
		{
			GlobalSettings.SetClientSetting(GlobalSettingsKeys.LastSeenName, ClientState.LastSeenName);
		}

		try
		{
			AudioManager.StopEncoding();
		}
		catch (Exception)
		{
			// ignored
		}

		try
		{
			Client.Disconnect();
		}
		catch (Exception)
		{
			// ignored
		}

		ClientState.DcsPlayerRadioInfo.Reset();
		ClientState.PlayerCoaltionLocationMetadata.Reset();
	}
	
	private void InitDefaultAddress()
	{
		// legacy setting migration
		if (!string.IsNullOrEmpty(GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastServer).StringValue) &&
		    FavouriteServersViewModel.Addresses.Count == 0)
		{
			var oldAddress = new ServerAddress(GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastServer).StringValue,
				GlobalSettings.GetClientSetting(GlobalSettingsKeys.LastServer).StringValue, null, true);
			FavouriteServersViewModel.Addresses.Add(oldAddress);
		}

		ServerAddress = FavouriteServersViewModel.DefaultServerAddress;
	}

	protected void OnClosing(CancelEventArgs e)
	{
		//stop timer
		_updateTimer?.Stop();
	}
	
	// Used for Design DataContext
	public MainWindowViewModel()
	{
		ToBeDepricatedMainWindow = new MainWindow();
	}
}