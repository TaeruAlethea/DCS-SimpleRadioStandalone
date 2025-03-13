using System.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

[ObservableObject]
public sealed partial class GlobalSettingsModel(GlobalSettingsStore store) : ConfigurationSection
{
	[ConfigurationProperty("AutoConnect", DefaultValue = true, IsRequired = true)]
	public bool AutoConnect
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AutoConnect);
		set => store.SetClientSetting(GlobalSettingsKeys.AutoConnect, value);
	}	
	[ConfigurationProperty("AutoConnectPrompt", DefaultValue = false, IsRequired = true)]
	public bool AutoConnectPrompt
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AutoConnectPrompt);
		set => store.SetClientSetting(GlobalSettingsKeys.AutoConnectPrompt, value);
	}
	[ConfigurationProperty("AutoConnectMismatchPrompt", DefaultValue = true, IsRequired = true)]
	public bool AutoConnectMismatchPrompt
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AutoConnectMismatchPrompt);
		set => store.SetClientSetting(GlobalSettingsKeys.AutoConnectMismatchPrompt, value);
	}
	[ConfigurationProperty("RadioOverlayTaskbarHide", DefaultValue = false, IsRequired = true)]
	public bool RadioOverlayTaskbarHide
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.RadioOverlayTaskbarHide);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioOverlayTaskbarHide, value);
	}
	[ConfigurationProperty("RefocusDCS", DefaultValue = false, IsRequired = true)]
	public bool RefocusDCS
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.RefocusDCS);
		set => store.SetClientSetting(GlobalSettingsKeys.RefocusDCS, value);
	}
	[ConfigurationProperty("ExpandControls", DefaultValue = false, IsRequired = true)]
	public bool ExpandControls
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.ExpandControls);
		set => store.SetClientSetting(GlobalSettingsKeys.ExpandControls, value);
	}
	
	[ConfigurationProperty("MinimiseToTray", DefaultValue = false, IsRequired = true)]
	public bool MinimiseToTray
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.MinimiseToTray);
		set => store.SetClientSetting(GlobalSettingsKeys.MinimiseToTray, value);
	}
	[ConfigurationProperty("StartMinimised", DefaultValue = false, IsRequired = true)]
	public bool StartMinimised
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.StartMinimised);
		set => store.SetClientSetting(GlobalSettingsKeys.StartMinimised, value);
	}
	
	[ConfigurationProperty("AudioInputDeviceId", DefaultValue = "", IsRequired = true)]
	public string AudioInputDeviceId
	{
		get => store.GetClientSetting(GlobalSettingsKeys.AudioInputDeviceId).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.AudioInputDeviceId, value);
	}
	[ConfigurationProperty("AudioOutputDeviceId", DefaultValue = "", IsRequired = true)]
	public string AudioOutputDeviceId
	{
		get => store.GetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.AudioOutputDeviceId, value);
	}
	[ConfigurationProperty("MicAudioOutputDeviceId", DefaultValue = "", IsRequired = true)]
	public string MicAudioOutputDeviceId
	{
		get => store.GetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.MicAudioOutputDeviceId, value);
	}
	
	[ConfigurationProperty("LastServer", DefaultValue = "127.0.0.1", IsRequired = true)]
	public string LastServer
	{
		get => store.GetClientSetting(GlobalSettingsKeys.LastServer).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.LastServer, value);
	}
	
	[ConfigurationProperty("MicBoost", DefaultValue = (double)0.514, IsRequired = true)]
	public double MicBoost
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.MicBoost);
		set => store.SetClientSetting(GlobalSettingsKeys.MicBoost, value);
	}
	[ConfigurationProperty("SpeakerBoost", DefaultValue = (double)0.514, IsRequired = true)]
	public double SpeakerBoost
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.SpeakerBoost);
		set => store.SetClientSetting(GlobalSettingsKeys.SpeakerBoost, value);
	}
	
	[ConfigurationProperty("RadioX", DefaultValue = (double)300, IsRequired = true)]
	public double RadioX
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioX);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioX, value);
	}
	[ConfigurationProperty("RadioY", DefaultValue = (double)300, IsRequired = true)]
	public double RadioY
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioY);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioY, value);
	}
	[ConfigurationProperty("RadioSize", DefaultValue = (double)1.0, IsRequired = true)]
	public double RadioSize
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioSize);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioSize, value);
	}
	[ConfigurationProperty("RadioOpacity", DefaultValue = (double)1.0, IsRequired = true)]
	public double RadioOpacity
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioOpacity);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioOpacity, value);
	}
	
	[ConfigurationProperty("RadioWidth", DefaultValue = (double)122, IsRequired = true)]
	public double RadioWidth
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioWidth);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioWidth, value);
	}
	[ConfigurationProperty("RadioHeight", DefaultValue = (double)270, IsRequired = true)]
	public double RadioHeight
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.RadioHeight);
		set => store.SetClientSetting(GlobalSettingsKeys.RadioHeight, value);
	}
	
	[ConfigurationProperty("ClientX", DefaultValue = (double)200, IsRequired = true)]
	public double ClientX
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.ClientX);
		set => store.SetClientSetting(GlobalSettingsKeys.ClientX, value);
	}
	[ConfigurationProperty("ClientY", DefaultValue = (double)200, IsRequired = true)]
	public double ClientY
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.ClientY);
		set => store.SetClientSetting(GlobalSettingsKeys.ClientY, value);
	}
	
	[ConfigurationProperty("AwacsX", DefaultValue = (double)300, IsRequired = true)]
	public double AwacsX
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.AwacsX);
		set => store.SetClientSetting(GlobalSettingsKeys.AwacsX, value);
	}
	[ConfigurationProperty("AwacsY", DefaultValue = (double)300, IsRequired = true)]
	public double AwacsY
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.AwacsY);
		set => store.SetClientSetting(GlobalSettingsKeys.AwacsY, value);
	}
	
	
	[ConfigurationProperty("CliendIdShort", DefaultValue = "", IsRequired = false)]
	public string CliendIdShort
	{
		get => store.GetClientSetting(GlobalSettingsKeys.CliendIdShort).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.CliendIdShort, value);
	}
	[ConfigurationProperty("ClientIdLong", DefaultValue = "", IsRequired = true)]
	public string ClientIdLong
	{
		get => store.GetClientSetting(GlobalSettingsKeys.ClientIdLong).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.ClientIdLong, value);
	}
	
	[ConfigurationProperty("DCSLOSOutgoingUDP", DefaultValue = (int)9086, IsRequired = true)]
	public int DCSLOSOutgoingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DCSLOSOutgoingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.DCSLOSOutgoingUDP, value);
	}
	[ConfigurationProperty("DCSIncomingUDP", DefaultValue = (int)9084, IsRequired = true)]
	public int DCSIncomingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DCSIncomingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.DCSIncomingUDP, value);
	}
	[ConfigurationProperty("CommandListenerUDP", DefaultValue = (int)9040, IsRequired = true)]
	public int CommandListenerUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.CommandListenerUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.CommandListenerUDP, value);
	}
	[ConfigurationProperty("OutgoingDCSUDPInfo", DefaultValue = (int)7080, IsRequired = true)]
	public int OutgoingDCSUDPInfo
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.OutgoingDCSUDPInfo);
		set => store.SetClientSetting(GlobalSettingsKeys.OutgoingDCSUDPInfo, value);
	}
	[ConfigurationProperty("OutgoingDCSUDPOther", DefaultValue = (int)7082, IsRequired = true)]
	public int OutgoingDCSUDPOther
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.OutgoingDCSUDPOther);
		set => store.SetClientSetting(GlobalSettingsKeys.OutgoingDCSUDPOther, value);
	}
	[ConfigurationProperty("DCSIncomingGameGUIUDP", DefaultValue = (int)5068, IsRequired = true)]
	public int DCSIncomingGameGUIUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DCSIncomingGameGUIUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.DCSIncomingGameGUIUDP, value);
	}
	[ConfigurationProperty("DCSLOSIncomingUDP", DefaultValue = (int)9085, IsRequired = true)]
	public int DCSLOSIncomingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DCSLOSIncomingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.DCSLOSIncomingUDP, value);
	}
	[ConfigurationProperty("DCSAutoConnectUDP", DefaultValue = (int)5069, IsRequired = true)]
	public int DCSAutoConnectUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DCSAutoConnectUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.DCSAutoConnectUDP, value);
	}
	
	[ConfigurationProperty("AutomaticGainControl", DefaultValue = true, IsRequired = true)]
	public bool AutomaticGainControl
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AGC);
		set => store.SetClientSetting(GlobalSettingsKeys.AGC, value);
	}
	[ConfigurationProperty("AGCTarget", DefaultValue = (int)30000, IsRequired = true)]
	public int AGCTarget
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.AGCTarget);
		set => store.SetClientSetting(GlobalSettingsKeys.AGCTarget, value);
	}
	[ConfigurationProperty("AGCDecrement", DefaultValue = (int)-60, IsRequired = true)]
	public int AGCDecrement
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.AGCDecrement);
		set => store.SetClientSetting(GlobalSettingsKeys.AGCDecrement, value);
	}
	[ConfigurationProperty("AGCLevelMax", DefaultValue = (int)68, IsRequired = true)]
	public int AGCLevelMax
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.AGCLevelMax);
		set => store.SetClientSetting(GlobalSettingsKeys.AGCLevelMax, value);
	}
	
	[ConfigurationProperty("Denoise", DefaultValue = true, IsRequired = true)]
	public bool Denoise
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.Denoise);
		set => store.SetClientSetting(GlobalSettingsKeys.Denoise, value);
	}
	[ConfigurationProperty("DenoiseAttenuation", DefaultValue = (int)-30, IsRequired = true)]
	public int DenoiseAttenuation
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.DenoiseAttenuation);
		set => store.SetClientSetting(GlobalSettingsKeys.DenoiseAttenuation, value);
	}
	
	[ConfigurationProperty("LastSeenName", DefaultValue = "", IsRequired = true)]
	public string LastSeenName
	{
		get => store.GetClientSetting(GlobalSettingsKeys.LastSeenName).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.LastSeenName, value);
	}
	
	[ConfigurationProperty("CheckForBetaUpdates", DefaultValue = false, IsRequired = true)]
	public bool CheckForBetaUpdates
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.CheckForBetaUpdates);
		set => store.SetClientSetting(GlobalSettingsKeys.CheckForBetaUpdates, value);
	}
	
	[ConfigurationProperty("AllowMultipleInstances", DefaultValue = false, IsRequired = true)]
	public bool AllowMultipleInstances
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AllowMultipleInstances);
		set => store.SetClientSetting(GlobalSettingsKeys.AllowMultipleInstances, value);
	}
	
	[ConfigurationProperty("DisableWindowVisibilityCheck", DefaultValue = false, IsRequired = true)]
	public bool DisableWindowVisibilityCheck
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.DisableWindowVisibilityCheck);
		set => store.SetClientSetting(GlobalSettingsKeys.DisableWindowVisibilityCheck, value);
	}
	[ConfigurationProperty("PlayConnectionSounds", DefaultValue = true, IsRequired = true)]
	public bool PlayConnectionSounds
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.PlayConnectionSounds);
		set => store.SetClientSetting(GlobalSettingsKeys.PlayConnectionSounds, value);
	}
	
	[ConfigurationProperty("RequireAdmin", DefaultValue = true, IsRequired = true)]
	public bool RequireAdmin
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.RequireAdmin);
		set => store.SetClientSetting(GlobalSettingsKeys.RequireAdmin, value);
	}

	public ProfileSettingsModel ProfileSettingsProperties
	{
		get
		{
			return store.ProfileSettingsProperties;
		}
	}
	
	[ConfigurationProperty("AutoSelectSettingsProfile", DefaultValue = false, IsRequired = true)]
	public bool AutoSelectSettingsProfile
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AutoSelectSettingsProfile);
		set => store.SetClientSetting(GlobalSettingsKeys.AutoSelectSettingsProfile, value);
	}
	
	[ConfigurationProperty("LotATCIncomingUDP", DefaultValue = (int)10710, IsRequired = true)]
	public int LotATCIncomingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.LotATCIncomingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.LotATCIncomingUDP, value);
	}
	[ConfigurationProperty("LotATCOutgoingUDP", DefaultValue = (int)10711, IsRequired = true)]
	public int LotATCOutgoingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.LotATCOutgoingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.LotATCOutgoingUDP, value);
	}
	[ConfigurationProperty("LotATCHeightOffset", DefaultValue = (int)50, IsRequired = true)]
	public int LotATCHeightOffset
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.LotATCHeightOffset);
		set => store.SetClientSetting(GlobalSettingsKeys.LotATCHeightOffset, value);
	}
	
	[ConfigurationProperty("VAICOMIncomingUDP", DefaultValue = (int)33501, IsRequired = true)]
	public int VAICOMIncomingUDP
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.VAICOMIncomingUDP);
		set => store.SetClientSetting(GlobalSettingsKeys.VAICOMIncomingUDP, value);
	}
	[ConfigurationProperty("VAICOMTXInhibitEnabled", DefaultValue = false, IsRequired = true)]
	public bool VAICOMTXInhibitEnabled
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.VAICOMTXInhibitEnabled);
		set => store.SetClientSetting(GlobalSettingsKeys.VAICOMTXInhibitEnabled, value);
	}
	[ConfigurationProperty("ShowTransmitterName", DefaultValue = true, IsRequired = true)]
	public bool ShowTransmitterName
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.ShowTransmitterName);
		set => store.SetClientSetting(GlobalSettingsKeys.ShowTransmitterName, value);
	}
	
	[ConfigurationProperty("IdleTimeOut", DefaultValue = (int)600, IsRequired = true)]
	public int IdleTimeOut
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.IdleTimeOut);
		set => store.SetClientSetting(GlobalSettingsKeys.IdleTimeOut, value);
	}
	
	[ConfigurationProperty("AllowRecording", DefaultValue = false, IsRequired = true)]
	public bool AllowRecording
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AllowRecording);
		set => store.SetClientSetting(GlobalSettingsKeys.AllowRecording, value);
	}
	[ConfigurationProperty("RecordAudio", DefaultValue = false, IsRequired = true)]
	public bool RecordAudio
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.RecordAudio);
		set => store.SetClientSetting(GlobalSettingsKeys.RecordAudio, value);
	}
	[ConfigurationProperty("SingleFileMixdown", DefaultValue = false, IsRequired = true)]
	public bool SingleFileMixdown
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.SingleFileMixdown);
		set => store.SetClientSetting(GlobalSettingsKeys.SingleFileMixdown, value);
	}
	[ConfigurationProperty("RecordingQuality", DefaultValue = 3, IsRequired = true)]
	public int RecordingQuality
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.RecordingQuality);
		set => store.SetClientSetting(GlobalSettingsKeys.RecordingQuality, value);
	}
	[ConfigurationProperty("DisallowedAudioTone", DefaultValue = false, IsRequired = true)]
	public bool DisallowedAudioTone
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.DisallowedAudioTone);
		set => store.SetClientSetting(GlobalSettingsKeys.DisallowedAudioTone, value);
	}
	
	[ConfigurationProperty("VOX", DefaultValue = false, IsRequired = true)]
	public bool VOX
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.VOX);
		set => store.SetClientSetting(GlobalSettingsKeys.VOX, value);
	}
	[ConfigurationProperty("VOXMode", DefaultValue = (int)3, IsRequired = true)]
	public int VOXMode
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.VOXMode);
		set => store.SetClientSetting(GlobalSettingsKeys.VOXMode, value);
	}
	[ConfigurationProperty("VOXMinimumTime", DefaultValue = (int)300, IsRequired = true)]
	public int VOXMinimumTime
	{
		get => store.GetClientSettingInt(GlobalSettingsKeys.VOXMinimumTime);
		set => store.SetClientSetting(GlobalSettingsKeys.VOXMinimumTime, value);
	}
	[ConfigurationProperty("VOXMinimumDB", DefaultValue = (double)-59.0, IsRequired = true)]
	public double VOXMinimumDB
	{
		get => store.GetClientSettingDouble(GlobalSettingsKeys.VOXMinimumDB);
		set => store.SetClientSetting(GlobalSettingsKeys.VOXMinimumDB, value);
	}
	
	[ConfigurationProperty("AllowXInputController", DefaultValue = false, IsRequired = true)]
	public bool AllowXInputController
	{
		get => store.GetClientSettingBool(GlobalSettingsKeys.AllowXInputController);
		set => store.SetClientSetting(GlobalSettingsKeys.AllowXInputController, value);
	}
	[ConfigurationProperty("LastPresetsFolder", DefaultValue = "", IsRequired = true)]
	public string LastPresetsFolder
	{
		get => store.GetClientSetting(GlobalSettingsKeys.LastPresetsFolder).ToString();
		set => store.SetClientSetting(GlobalSettingsKeys.LastPresetsFolder, value);
	}
}