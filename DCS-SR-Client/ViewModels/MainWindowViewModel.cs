using System;
using System.Runtime;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Preferences;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.Favourites;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	[Obsolete("MainWindow Reference is not MVVM compliant.", true)]
	public MainWindow ToBeDepricatedMainWindow { get; init; }
	
	/// <remarks>Used in the XAML for DataBinding many things</remarks>
	public ClientStateSingleton ClientState { get; } = ClientStateSingleton.Instance;
	/// <remarks>Used in the XAML for DataBinding the connected client count</remarks>
	public ConnectedClientsSingleton Clients { get; } = ConnectedClientsSingleton.Instance;
	/// <remarks>Used in the XAML for DataBinding many things</remarks>
	public AudioInputSingleton AudioInput { get; } = AudioInputSingleton.Instance;
	/// <remarks>Used in the XAML for DataBinding output audio related UI elements</remarks>
	public AudioOutputSingleton AudioOutput { get; } = AudioOutputSingleton.Instance;

	public FavouriteServersViewModel FavouriteServersViewModel { get; }

	public MainWindowViewModel(MainWindow mainWindowView)
	{
		ToBeDepricatedMainWindow = mainWindowView;
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		FavouriteServersViewModel = new FavouriteServersViewModel(new CsvFavouriteServerStore());
	}

	// Used for Design DataContext
	public MainWindowViewModel()
	{
		ToBeDepricatedMainWindow = new MainWindow();
	}
}