using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public partial class SrsSettingsService : ObservableObject, ISrsSettings
{
	private IConfigurationRoot _configuration;

	[ObservableProperty] [NotifyPropertyChangedFor(nameof(CurrentProfile))]
	private GlobalSettingsModel _globalSettings = new GlobalSettingsModel();

	partial void OnGlobalSettingsChanged(GlobalSettingsModel value)
	{
		SaveSettings(_configuration);
	}

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProfileNames))]
	private List<ProfileSettingsModel> _profileSettings = new List<ProfileSettingsModel>();
	
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
		if (!File.Exists("settings.json")) { CreateNewAppSettings(); }			
		if (File.ReadAllBytes("appsettings.json").Length <= 10) { CreateNewAppSettings(); }
		
		_configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", reloadOnChange: false, optional: false)
			.Build();

		_configuration.GetSection("GlobalSettings").Bind(GlobalSettings);
		_configuration.GetSection("ProfileSettings").Bind(ProfileSettings);
		
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
	
	public void SaveSettings(IConfigurationRoot configuration)
	{
		try
		{
			File.WriteAllText("appsettings.json", JsonConvert.SerializeObject(configuration, Formatting.Indented));
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}
	
	public void CreateNewAppSettings()
	{
		string newSettings = JsonConvert.SerializeObject(new SettingsModel());
		
		File.WriteAllText("appsettings.json", JsonConvert.SerializeObject(newSettings, Formatting.Indented));
	}
	
	class SettingsModel
	{
		private string SettingsVersion= string.Empty;
		private GlobalSettingsModel GlobalSettings = new GlobalSettingsModel();
		private List<ProfileSettingsModel> ProfileSettings = new List<ProfileSettingsModel>()
			{ new ProfileSettingsModel() };
	}
}

