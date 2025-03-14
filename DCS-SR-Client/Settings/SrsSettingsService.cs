using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public partial class SrsSettingsService : ObservableRecipient, IRecipient<SettingChangedMessage>, ISrsSettings
{
	const string SettingsFileName = "./appsettings.json";
	
	private IConfigurationRoot _configuration;

	// All Settings
	[ObservableProperty] [NotifyPropertyChangedFor(nameof(CurrentProfile))]
	private GlobalSettingsModel _globalSettings = new GlobalSettingsModel();
	partial void OnGlobalSettingsChanged(GlobalSettingsModel value)
	{
		if (GlobalSettings.CurrentProfileName != value.CurrentProfileName)
		{
			ProfileSettingsModel profileOfIncomingName = _profileSettings.Find(p =>
				p.ProfileName == value.CurrentProfileName);
			CurrentProfile = profileOfIncomingName;
			OnPropertyChanged(nameof(CurrentProfile));
		}
		
		SaveSettings(GlobalSettings, _profileSettings);
	}
	private List<ProfileSettingsModel> _profileSettings = new List<ProfileSettingsModel>();
	
	// Only the Current Profile
	[ObservableProperty] private ProfileSettingsModel _currentProfile;
	
	public List<string> ProfileNames
	{
		get
		{
			return _profileSettings.Select(x => x.ProfileName).ToList();
		}
	}

	public SrsSettingsService()
	{
		if (!File.Exists(SettingsFileName)) { CreateNewAppSettings(); }			
		if (File.ReadAllBytes(SettingsFileName).Length <= 10) { CreateNewAppSettings(); }
		
		_configuration = new ConfigurationBuilder()
			.AddJsonFile(SettingsFileName, reloadOnChange: false, optional: false)
			.Build();

		_configuration.GetSection("GlobalSettings").Bind(GlobalSettings);
		_configuration.GetSection("ProfileSettings").Bind(_profileSettings);
	}
	
	public class SettingsModel
	{
		public string SettingsVersion = "1.0";
		public GlobalSettingsModel GlobalSettings = new GlobalSettingsModel();
		public List<ProfileSettingsModel> ProfileSettings = new List<ProfileSettingsModel>()
			{ new ProfileSettingsModel() };
	}
	
	[RelayCommand] private void CreateProfile(string profileName)
	{
		var newProfile = new ProfileSettingsModel{ ProfileName = profileName };
		_profileSettings.Add(newProfile);
		GlobalSettings.CurrentProfileName = newProfile.ProfileName;
		OnPropertyChanged(nameof(ProfileNames));
	}

	[RelayCommand] private void RenameProfile(string profileName)
	{
		CurrentProfile.ProfileName = profileName;
		GlobalSettings.CurrentProfileName = profileName;
		OnPropertyChanged(nameof(ProfileNames));
	}

	[RelayCommand] private void DuplicateProfile(string profileName)
	{
		ProfileSettingsModel newProfile = (ProfileSettingsModel)CurrentProfile.Clone();
		newProfile.ProfileName = profileName;
		
		_profileSettings.Add(newProfile);
		GlobalSettings.CurrentProfileName = profileName;
		OnPropertyChanged(nameof(ProfileNames));
	}

	[RelayCommand] private void DeleteProfile(string profileName)
	{
		GlobalSettings.CurrentProfileName = "default";
		
		var targetProfile = _profileSettings.Find(p => p.ProfileName == profileName);
		_profileSettings.Remove(targetProfile);
		OnPropertyChanged(nameof(ProfileNames));
	}

	private bool isSaving = false; // quick and dirty locking to avoid spamming saves.
	public void SaveSettings(GlobalSettingsModel globalSettings, List<ProfileSettingsModel> profileSettings)
	{
		if (isSaving) { return; }
		try
		{
			isSaving = true;
			SettingsModel temp = new SettingsModel() { GlobalSettings = globalSettings, ProfileSettings = profileSettings };
			string json = JsonConvert.SerializeObject(temp, Formatting.Indented);
			File.WriteAllText(SettingsFileName, json, Encoding.UTF8);
			isSaving = false;
		}
		catch (Exception e)
		{
			
		}
	}
	
	public void CreateNewAppSettings()
	{
		string json = JsonConvert.SerializeObject(new SettingsModel(), Formatting.Indented);
		File.WriteAllText(SettingsFileName, json, Encoding.UTF8);
	}

	public void Receive(SettingChangedMessage message)
	{
		if (message.changeType == SettingChangedMessage.SettingChangeType.Global)
		{
			OnPropertyChanged(nameof(GlobalSettings));
		}

		if (message.changeType == SettingChangedMessage.SettingChangeType.Profile)
		{
			OnPropertyChanged(nameof(CurrentProfile));
		}
	}
}



