using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		Properties.Resources.Culture = CultureInfo.CurrentUICulture;
		ServiceProvider services = ConfigureServices();
		
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			// Line below is needed to remove Avalonia data validation.
			// Without this line you will get duplicate validations from both Avalonia and CT
			BindingPlugins.DataValidators.RemoveAt(0);
			desktop.MainWindow = new MainWindow
			{
				DataContext = services.GetRequiredService<MainViewModel>()
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView
			{
				DataContext = new MainViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

	public static ServiceProvider ConfigureServices()
	{
		var services = new ServiceCollection();
		
		services.AddTransient<MainViewModel>();
		
		return services.BuildServiceProvider();
	}
}