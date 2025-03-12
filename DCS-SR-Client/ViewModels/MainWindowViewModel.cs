using System;
using System.ComponentModel;
using System.Runtime;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;
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
	
	/// <remarks>Used in the XAML for DataBinding many things</remarks>
	public ClientStateSingleton ClientState { get; } = ClientStateSingleton.Instance;
	/// <remarks>Used in the XAML for DataBinding the connected client count</remarks>
	public ConnectedClientsSingleton Clients { get; } = ConnectedClientsSingleton.Instance;
	
	private readonly DispatcherTimer _updateTimer;
	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
	private ServerAddress _serverAddress;
	
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
		
		InitDefaultAddress();
		
		AudioManager.SpeakerBoost = GlobalSettings.GetClientSetting(GlobalSettingsKeys.SpeakerBoost).FloatValue;
		
		_updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
		_updateTimer.Tick += ToBeDepricatedMainWindow.UpdatePlayerLocationAndVUMeters;
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