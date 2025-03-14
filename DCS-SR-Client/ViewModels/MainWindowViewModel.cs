using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network.DCS;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Preferences;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.Favourites;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IMainViewModel
{
	[ObservableProperty] private ISrsSettings _srsSettings;
	public AudioInputSingleton AudioInput { get; }
	public AudioOutputSingleton AudioOutput { get; }
	[ObservableProperty] private ClientStateSingleton _clientState;
	[ObservableProperty] private ConnectedClientsSingleton _clients;
	
	[Obsolete("MainWindow Reference is not MVVM compliant.")]
	//public MainWindow ToBeDepricatedMainWindow { get; init; }
	
	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	
	[ObservableProperty] private AudioManager _audioManager;
	[ObservableProperty] private AudioPreview _audioPreview;
	
	[ObservableProperty] private SRSClientSyncHandler _client;
	[ObservableProperty] private DCSAutoConnectHandler _dcsAutoConnectListener;
	
	private readonly DispatcherTimer _updateTimer;
	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
	private ServerAddress _serverAddress;
	[ObservableProperty] private IPAddress _resolvedIp;
	[ObservableProperty] private string _guid; // Does not need to be observable.
	[ObservableProperty] private int _port = 5002;
	
	public FavouriteServersViewModel FavouriteServersViewModel { get; }
	
	[ObservableProperty]
	private SyncedServerSettings _serverSettings = SyncedServerSettings.Instance;
	
	public MainWindowViewModel()
	{
		// Grab Dependencies from DI Container
		SrsSettings = Ioc.Default.GetRequiredService<ISrsSettings>();
		AudioInput = Ioc.Default.GetRequiredService<AudioInputSingleton>();
		AudioOutput = Ioc.Default.GetRequiredService<AudioOutputSingleton>();
		ClientState = Ioc.Default.GetRequiredService<ClientStateSingleton>();
		Clients = Ioc.Default.GetRequiredService<ConnectedClientsSingleton>();
		
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		FavouriteServersViewModel = new FavouriteServersViewModel(new CsvFavouriteServerStore());
		
		UpdaterChecker.CheckForUpdate(SrsSettings.GlobalSettings.CheckForBetaUpdates);

		
		_audioManager = new AudioManager(AudioOutput.WindowsN);
		Guid = ClientStateSingleton.Instance.ShortGUID;
		
		AudioManager.SpeakerBoost = (float)SrsSettings.GlobalSettings.SpeakerBoost;
		
		//DcsAutoConnectListener = new DCSAutoConnectHandler(ToBeDepricatedMainWindow.AutoConnect);

		_updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
		//_updateTimer.Tick += ToBeDepricatedMainWindow.UpdateVUMeters;
		_updateTimer.Start();
	}
	
	[RelayCommand, MethodImpl(MethodImplOptions.Synchronized)]
	public void Connect()
	{
	
        if (ClientState.IsConnected)
        {
            Stop();
        }
        else
        {
            //ToBeDepricatedMainWindow.SaveSelectedInputAndOutput();
            /*
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

                                        foreach (var profile in SrsSettings.ProfileSettings)
                                        {
	                                        string profileName = profile.ProfileName;
                                            //find matching seat
                                            var splitName = profileName.Trim().ToLowerInvariant().Split('_')
                                                .First();
                                            if (name.StartsWith(Regex.Replace(splitName, "[^a-zA-Z0-9]", "")) &&
                                                profileName.Trim().EndsWith(nameSeat))
                                            {
	                                            SrsSettings.GlobalSettings.CurrentProfileName = profileName;
                                                return;
                                            }
                                        }

                                        foreach (var profile in SrsSettings.ProfileSettings)
                                        {
	                                        string profileName = profile.ProfileName;
                                            //find matching seat
                                            if (name.StartsWith(Regex.Replace(profileName.Trim().ToLower(),
                                                    "[^a-zA-Z0-9_]", "")))
                                            {
	                                            SrsSettings.GlobalSettings.CurrentProfileName = profileName;
                                                return;
                                            }
                                        }
                                        SrsSettings.GlobalSettings.CurrentProfileName = SrsSettings.ProfileSettings[0].ProfileName;
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
            */
        }
	}
	
	[RelayCommand]
	public void Stop(bool connectionError = false)
	{
		if (ClientState.IsConnected && SrsSettings.GlobalSettings.PlayConnectionSounds)
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
		/* Todo: Fix this
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
		    SrsSettings.GlobalSettings.LastSeenName != ClientState.LastSeenName)
		{
			SrsSettings.GlobalSettings.LastSeenName = ClientState.LastSeenName;
		}
		*/
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
		ClientState.PlayerCoalitionLocationMetadata.Reset();
	}
	
	private void InitDefaultAddress()
	{
		// legacy setting migration
		if (!string.IsNullOrEmpty(SrsSettings.GlobalSettings.LastServer) &&
		    FavouriteServersViewModel.Addresses.Count == 0)
		{
			var oldAddress = new ServerAddress(SrsSettings.GlobalSettings.LastServer,
				SrsSettings.GlobalSettings.LastServer, null, true);
			FavouriteServersViewModel.Addresses.Add(oldAddress);
		}

		ServerAddress = FavouriteServersViewModel.DefaultServerAddress;
	}

	protected void OnClosing(CancelEventArgs e)
	{
		//stop timer
		_updateTimer?.Stop();
	}
}