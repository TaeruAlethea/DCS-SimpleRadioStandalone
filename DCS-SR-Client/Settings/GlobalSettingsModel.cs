using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public sealed partial class GlobalSettingsModel : ConfigurationSection
{
	[ConfigurationProperty("AutoConnect", DefaultValue = true, IsRequired = true)]
	public bool AutoConnect { get; set; }
	[ConfigurationProperty("AutoConnectPrompt", DefaultValue = false, IsRequired = true)]
	public bool AutoConnectPrompt { get; set; }
	[ConfigurationProperty("AutoConnectMismatchPrompt", DefaultValue = true, IsRequired = true)]
	public bool AutoConnectMismatchPrompt { get; set; }
	[ConfigurationProperty("RadioOverlayTaskbarHide", DefaultValue = false, IsRequired = true)]
	public bool RadioOverlayTaskbarHide { get; set; }
	[ConfigurationProperty("RefocusDCS", DefaultValue = false, IsRequired = true)]
	public bool RefocusDCS { get; set; }
	[ConfigurationProperty("ExpandControls", DefaultValue = false, IsRequired = true)]
	public bool ExpandControls { get; set; }
	
	[ConfigurationProperty("MinimiseToTray", DefaultValue = false, IsRequired = true)]
	public bool MinimiseToTray { get; set; }
	[ConfigurationProperty("StartMinimised", DefaultValue = false, IsRequired = true)]
	public bool StartMinimised { get; set; }
	
	[ConfigurationProperty("AudioInputDeviceId", DefaultValue = "", IsRequired = true)]
	public string AudioInputDeviceId { get; set; }
	[ConfigurationProperty("AudioOutputDeviceId", DefaultValue = "", IsRequired = true)]
	public string AudioOutputDeviceId { get; set; }
	[ConfigurationProperty("MicAudioOutputDeviceId", DefaultValue = "", IsRequired = true)]
	public string MicAudioOutputDeviceId { get; set; }
	
	[ConfigurationProperty("LastServer", DefaultValue = "127.0.0.1", IsRequired = true)]
	public string LastServer { get; set; }
	
	[ConfigurationProperty("MicBoost", DefaultValue = (double)0.514, IsRequired = true)]
	public double MicBoost { get; set; }
	[ConfigurationProperty("SpeakerBoost", DefaultValue = (double)0.514, IsRequired = true)]
	public double SpeakerBoost { get; set; }
	
	[ConfigurationProperty("RadioX", DefaultValue = (double)300, IsRequired = true)]
	public double RadioX { get; set; }
	[ConfigurationProperty("RadioY", DefaultValue = (double)300, IsRequired = true)]
	public double RadioY { get; set; }
	[ConfigurationProperty("RadioSize", DefaultValue = (double)1.0, IsRequired = true)]
	public double RadioSize { get; set; }
	[ConfigurationProperty("RadioOpacity", DefaultValue = (double)1.0, IsRequired = true)]
	public double RadioOpacity { get; set; }
	
	[ConfigurationProperty("RadioWidth", DefaultValue = (double)122, IsRequired = true)]
	public double RadioWidth { get; set; }
	[ConfigurationProperty("RadioHeight", DefaultValue = (double)270, IsRequired = true)]
	public double RadioHeight { get; set; }
	
	[ConfigurationProperty("ClientX", DefaultValue = (double)200, IsRequired = true)]
	public double ClientX { get; set; }
	[ConfigurationProperty("ClientY", DefaultValue = (double)200, IsRequired = true)]
	public double ClientY { get; set; }
	
	[ConfigurationProperty("AwacsX", DefaultValue = (double)300, IsRequired = true)]
	public double AwacsX { get; set; }
	[ConfigurationProperty("AwacsY", DefaultValue = (double)300, IsRequired = true)]
	public double AwacsY { get; set; }
	
	
	[ConfigurationProperty("CliendIdShort", DefaultValue = "", IsRequired = false)]
	public string CliendIdShort { get; set; }
	[ConfigurationProperty("ClientIdLong", DefaultValue = "", IsRequired = true)]
	public string ClientIdLong { get; set; }
	
	[ConfigurationProperty("DCSLOSOutgoingUDP", DefaultValue = (int)9086, IsRequired = true)]
	public int DCSLOSOutgoingUDP { get; set; }
	[ConfigurationProperty("DCSIncomingUDP", DefaultValue = (int)9084, IsRequired = true)]
	public int DCSIncomingUDP { get; set; }
	[ConfigurationProperty("CommandListenerUDP", DefaultValue = (int)9040, IsRequired = true)]
	public int CommandListenerUDP { get; set; }
	[ConfigurationProperty("OutgoingDCSUDPInfo", DefaultValue = (int)7080, IsRequired = true)]
	public int OutgoingDCSUDPInfo { get; set; }
	[ConfigurationProperty("OutgoingDCSUDPOther", DefaultValue = (int)7082, IsRequired = true)]
	public int OutgoingDCSUDPOther { get; set; }
	[ConfigurationProperty("DCSIncomingGameGUIUDP", DefaultValue = (int)5068, IsRequired = true)]
	public int DCSIncomingGameGUIUDP { get; set; }
	[ConfigurationProperty("DCSLOSIncomingUDP", DefaultValue = (int)9085, IsRequired = true)]
	public int DCSLOSIncomingUDP { get; set; }
	[ConfigurationProperty("DCSAutoConnectUDP", DefaultValue = (int)5069, IsRequired = true)]
	public int DCSAutoConnectUDP { get; set; }
	
	[ConfigurationProperty("AutomaticGainControl", DefaultValue = true, IsRequired = true)]
	public bool AutomaticGainControl { get; set; }
	[ConfigurationProperty("AGCTarget", DefaultValue = (int)30000, IsRequired = true)]
	public int AGCTarget { get; set; }
	[ConfigurationProperty("AGCDecrement", DefaultValue = (int)-60, IsRequired = true)]
	public int AGCDecrement { get; set; }
	[ConfigurationProperty("AGCLevelMax", DefaultValue = (int)68, IsRequired = true)]
	public int AGCLevelMax { get; set; }
	
	[ConfigurationProperty("Denoise", DefaultValue = true, IsRequired = true)]
	public bool Denoise { get; set; }
	[ConfigurationProperty("DenoiseAttenuation", DefaultValue = (int)-30, IsRequired = true)]
	public int DenoiseAttenuation { get; set; }
	
	[ConfigurationProperty("LastSeenName", DefaultValue = "", IsRequired = true)]
	public string LastSeenName { get; set; }
	
	[ConfigurationProperty("CheckForBetaUpdates", DefaultValue = false, IsRequired = true)]
	public bool CheckForBetaUpdates { get; set; }
	
	[ConfigurationProperty("AllowMultipleInstances", DefaultValue = false, IsRequired = true)]
	public bool AllowMultipleInstances { get; set; }
	
	[ConfigurationProperty("DisableWindowVisibilityCheck", DefaultValue = false, IsRequired = true)]
	public bool DisableWindowVisibilityCheck { get; set; }
	[ConfigurationProperty("PlayConnectionSounds", DefaultValue = true, IsRequired = true)]
	public bool PlayConnectionSounds { get; set; }
	
	[ConfigurationProperty("RequireAdmin", DefaultValue = true, IsRequired = true)]
	public bool RequireAdmin { get; set; }
	
	[ConfigurationProperty("CurrentProfileName", DefaultValue = "Default", IsRequired = true)]
	public string CurrentProfileName  { get; set; }
	
	[ConfigurationProperty("AutoSelectSettingsProfile", DefaultValue = false, IsRequired = true)]
	public bool AutoSelectSettingsProfile { get; set; }
	
	[ConfigurationProperty("LotATCIncomingUDP", DefaultValue = (int)10710, IsRequired = true)]
	public int LotATCIncomingUDP { get; set; }
	[ConfigurationProperty("LotATCOutgoingUDP", DefaultValue = (int)10711, IsRequired = true)]
	public int LotATCOutgoingUDP { get; set; }
	[ConfigurationProperty("LotATCHeightOffset", DefaultValue = (int)50, IsRequired = true)]
	public int LotATCHeightOffset { get; set; }
	
	[ConfigurationProperty("VAICOMIncomingUDP", DefaultValue = (int)33501, IsRequired = true)]
	public int VAICOMIncomingUDP { get; set; }
	[ConfigurationProperty("VAICOMTXInhibitEnabled", DefaultValue = false, IsRequired = true)]
	public bool VAICOMTXInhibitEnabled { get; set; }
	[ConfigurationProperty("ShowTransmitterName", DefaultValue = true, IsRequired = true)]
	public bool ShowTransmitterName { get; set; }
	
	[ConfigurationProperty("IdleTimeOut", DefaultValue = (int)600, IsRequired = true)]
	public int IdleTimeOut { get; set; }
	
	[ConfigurationProperty("AllowRecording", DefaultValue = false, IsRequired = true)]
	public bool AllowRecording { get; set; }
	[ConfigurationProperty("RecordAudio", DefaultValue = false, IsRequired = true)]
	public bool RecordAudio { get; set; }
	[ConfigurationProperty("SingleFileMixdown", DefaultValue = false, IsRequired = true)]
	public bool SingleFileMixdown { get; set; }
	[ConfigurationProperty("RecordingQuality", DefaultValue = 3, IsRequired = true)]
	public int RecordingQuality { get; set; }
	[ConfigurationProperty("DisallowedAudioTone", DefaultValue = false, IsRequired = true)]
	public bool DisallowedAudioTone { get; set; }
	
	[ConfigurationProperty("VOX", DefaultValue = false, IsRequired = true)]
	public bool VOX { get; set; }
	[ConfigurationProperty("VOXMode", DefaultValue = (int)3, IsRequired = true)]
	public int VOXMode { get; set; }
	[ConfigurationProperty("VOXMinimumTime", DefaultValue = (int)300, IsRequired = true)]
	public int VOXMinimumTime { get; set; }
	[ConfigurationProperty("VOXMinimumDB", DefaultValue = (double)-59.0, IsRequired = true)]
	public double VOXMinimumDB { get; set; }
	
	[ConfigurationProperty("AllowXInputController", DefaultValue = false, IsRequired = true)]
	public bool AllowXInputController { get; set; }
	[ConfigurationProperty("LastPresetsFolder", DefaultValue = "", IsRequired = true)]
	public string LastPresetsFolder { get; set; }
}