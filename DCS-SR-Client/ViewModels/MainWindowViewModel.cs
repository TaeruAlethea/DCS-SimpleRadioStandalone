using System.Runtime;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	private MainWindow _ToBeDepricatedMainWindow { get; init; }
	
	public AudioInputSingleton AudioInput { get; } = AudioInputSingleton.Instance;

	public MainWindowViewModel(MainWindow mainWindowView = null)
	{
		_ToBeDepricatedMainWindow = mainWindowView;
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
	}
}