using System;
using System.ComponentModel;
using System.Net;
using System.Runtime;
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
	
	[ObservableProperty]
	private GlobalSettingsStore _globalSettings = GlobalSettingsStore.Instance;
	
	public MainWindowViewModel(MainWindow mainWindowView)
	{
		ToBeDepricatedMainWindow = mainWindowView;
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		FavouriteServersViewModel = new FavouriteServersViewModel(new CsvFavouriteServerStore());
		
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
	private void Connect()
	{
		ToBeDepricatedMainWindow.Connect();
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