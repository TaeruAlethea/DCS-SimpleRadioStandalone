using System.Reflection;
using Avalonia.Controls;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		Title = string.Concat("SRS Server - ", Assembly.GetExecutingAssembly().GetName().Version!.ToString() );
		InitializeComponent();
	}
}