using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public partial class SrsSettingsService : ObservableObject, ISrsSettings
{
	private IConfiguration _configuration;
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CurrentProfile))]
	private GlobalSettingsModel _globalSettings;
	
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProfileNames))]
	private List<ProfileSettingsModel> _profileSettings;
	
	public ProfileSettingsModel CurrentProfile
	{
		get
		{
			return ProfileSettings.Find(p => 
				p.ProfileName == GlobalSettings.CurrentProfileName);
		}
	}
	public List<string> ProfileNames
	{
		get
		{
			return ProfileSettings.Select(x => x.ProfileName).ToList();
		}
	}
	
	public SrsSettingsService()
	{
		_configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
			.Build();
		
		GlobalSettings = _configuration.GetSection("GlobalSettings").Get<GlobalSettingsModel>();
		ProfileSettings = _configuration.GetSection("ProfileSettings").Get<List<ProfileSettingsModel>>();
	}
	
	public Task SaveAsync()
	{
		throw new System.NotImplementedException();
	}

	[RelayCommand] private void CreateProfile(string profileName)
	{
		var newProfile = new ProfileSettingsModel{ ProfileName = profileName };
		ProfileSettings.Add(newProfile);
		GlobalSettings.CurrentProfileName = newProfile.ProfileName;
	}

	[RelayCommand] private void RenameProfile(string profileName)
	{
		CurrentProfile.ProfileName = profileName;
		GlobalSettings.CurrentProfileName = profileName;
	}

	[RelayCommand] private void DuplicateProfile(string profileName)
	{
		ProfileSettingsModel newProfile = (ProfileSettingsModel)CurrentProfile.Clone();
		newProfile.ProfileName = profileName;
		
		ProfileSettings.Add(newProfile);
		GlobalSettings.CurrentProfileName = profileName;
	}

	[RelayCommand] private void DeleteProfile(string profileName)
	{
		GlobalSettings.CurrentProfileName = "default";
		
		var targetProfile = ProfileSettings.Find(p => p.ProfileName == profileName);
		ProfileSettings.Remove(targetProfile);
	}
}

