namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class GlobalSettingsModel
{
	public bool AutoConnect{ get; set; } = true;
	public bool AutoConnectPrompt { get; set; } = false;
	public bool AutoConnectMismatchPrompt { get; set; } = true;
	public bool RadioOverlayTaskbarHide { get; set; } = false;
	public bool RefocusDCS { get; set; } = false;
	public bool ExpandControls { get; set; } = false;
	
	public bool MinimiseToTray { get; set; } = false;
	public bool StartMinimised { get; set; } = false;
	
	public string AudioInputDeviceId { get; set; } = string.Empty;
	public string AudioOutputDeviceId { get; set; } = string.Empty;
	public string MicAudioOutputDeviceId { get; set; } = string.Empty;
	
	public string LastServer { get; set; } = "127.0.0.1";
	
	public double MicBoost { get; set; } = (double)0.514;
	public double SpeakerBoost { get; set; } = (double)0.514;
	
	public double RadioX { get; set; } = (double)300;
	public double RadioY { get; set; } = (double)300;
	public double RadioSize { get; set; } = (double)1.0;
	public double RadioOpacity { get; set; } = (double)1.0;
	
	public double RadioWidth { get; set; } = (double)122;
	public double RadioHeight { get; set; } = (double)270;
	
	public double ClientX { get; set; } = (double)200;
	public double ClientY { get; set; } = (double)200;
	
	public double AwacsX { get; set; } = 0;
	public double AwacsY { get; set; } = (double)300;
	
	
	public string CliendIdShort { get; set; } = string.Empty;
	public string ClientIdLong { get; set; } = string.Empty;
	
	public int DCSLOSOutgoingUDP { get; set; } = (int)9086;
	public int DCSIncomingUDP { get; set; } = (int)9084;
	public int CommandListenerUDP { get; set; } = (int)9040;
	public int OutgoingDCSUDPInfo { get; set; } = (int)7080;
	public int OutgoingDCSUDPOther { get; set; } = (int)7082;
	public int DCSIncomingGameGUIUDP { get; set; } = (int)5068;
	public int DCSLOSIncomingUDP { get; set; } = (int)9085;
	public int DCSAutoConnectUDP { get; set; } = (int)5069;
	
	public bool AutomaticGainControl { get; set; } = true;
	public int AGCTarget { get; set; } = (int)30000;
	public int AGCDecrement { get; set; } = (int)-60;
	public int AGCLevelMax { get; set; } = (int)68;
	
	public bool Denoise { get; set; } = true;
	public int DenoiseAttenuation { get; set; } = (int)-30;
	
	public string LastSeenName { get; set; } = string.Empty;
	
	public bool CheckForBetaUpdates { get; set; } = false;
	
	public bool AllowMultipleInstances { get; set; } = false;
	
	public bool DisableWindowVisibilityCheck { get; set; } = false;

	public bool PlayConnectionSounds { get; set; } = true;

	public bool RequireAdmin { get; set; } = true;
	
	public string CurrentProfileName  { get; set; } = "Default";

	public bool AutoSelectSettingsProfile { get; set; } = false;
	
	public int LotATCIncomingUDP { get; set; } = (int)10710;
	public int LotATCOutgoingUDP { get; set; } = (int)10711;
	public int LotATCHeightOffset { get; set; } = (int)50;
	
	public int VAICOMIncomingUDP { get; set; } = (int)33501;
	public bool VAICOMTXInhibitEnabled { get; set; } = false;
	public bool ShowTransmitterName { get; set; } = true;
	
	public int IdleTimeOut { get; set; } = (int)600;
	
	public bool AllowRecording { get; set; } = false;
	public bool RecordAudio { get; set; } = false;
	public bool SingleFileMixdown { get; set; } = false;
	public int RecordingQuality { get; set; } = (int)3;
	public bool DisallowedAudioTone { get; set; } = false;
	
	public bool VOX { get; set; } = false;
	public int VOXMode { get; set; } = (int)3;
	public int VOXMinimumTime { get; set; } = (int)300;
	public double VOXMinimumDB { get; set; } = (double)-59.0;
	
	public bool AllowXInputController { get; set; } = false;
	public string LastPresetsFolder { get; set; } = string.Empty;
}