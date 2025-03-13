using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public interface ISrsSettings
{
	public GlobalSettingsModel GlobalSettings { get; }
	public List<ProfileSettingsModel> ProfileSettings { get; }
	
	public ProfileSettingsModel CurrentProfile { get; }
	public List<string> ProfileNames { get; }
	
	Task SaveAsync();
	
	IRelayCommand<string> CreateProfileCommand { get; }
	IRelayCommand<string> RenameProfileCommand { get; }
	IRelayCommand<string> DuplicateProfileCommand { get; }
	IRelayCommand<string> DeleteProfileCommand { get; }
}