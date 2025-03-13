using System;
using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class ProfileSettingsModel : ConfigurationSection, ICloneable
{
	[ConfigurationProperty("ProfileName", DefaultValue = "Default", IsRequired = true)] public string ProfileName { get; set; }
	[ConfigurationProperty("RadioEffects", DefaultValue = true, IsRequired = true)] public bool RadioEffects { get; set; }
	[ConfigurationProperty("RadioEffectsClipping", DefaultValue = false, IsRequired = true)] public bool RadioEffectsClipping { get; set; }
	[ConfigurationProperty("RadioEncryptionEffects", DefaultValue = true, IsRequired = true)] public bool RadioEncryptionEffects { get; set; }
	[ConfigurationProperty("NatoFmTone", DefaultValue = true, IsRequired = true)] public bool NatoFmTone { get; set; }
	[ConfigurationProperty("HaveQuickTone", DefaultValue = true, IsRequired = true)] public bool HaveQuickTone { get; set; }
	[ConfigurationProperty("RadioRxEffects_Start", DefaultValue = true, IsRequired = true)] public bool RadioRxEffects_Start { get; set; }
	[ConfigurationProperty("RadioRxEffects_End", DefaultValue = true, IsRequired = true)] public bool RadioRxEffects_End { get; set; }
	
	//todo RadioTransmissionStartSelection
	//todo RadioTransmissionEndSelection
	
	[ConfigurationProperty("RadioTxEffects_Start", DefaultValue = true, IsRequired = true)] public bool RadioTxEffects_Start { get; set; }
	[ConfigurationProperty("RadioTxEffects_End", DefaultValue = true, IsRequired = true)] public bool RadioTxEffects_End { get; set; }
	[ConfigurationProperty("MidsRadioEffect", DefaultValue = true, IsRequired = true)] public bool MidsRadioEffect { get; set; }
	
	[ConfigurationProperty("AutoSelectPresetChannel", DefaultValue = true, IsRequired = true)] public bool AutoSelectPresetChannel { get; set; }
	[ConfigurationProperty("AlwaysAllowHotasControls", DefaultValue = false, IsRequired = true)] public bool AlwaysAllowHotasControls { get; set; }
	[ConfigurationProperty("AllowDcsPtt", DefaultValue = true, IsRequired = true)] public bool AllowDcsPtt { get; set; }
	[ConfigurationProperty("RadioSwitchIsPtt", DefaultValue = false, IsRequired = true)] public bool RadioSwitchIsPtt { get; set; }
	[ConfigurationProperty("RadioSwitchIsPttOnlyWhenValid", DefaultValue = false, IsRequired = true)] public bool RadioSwitchIsPttOnlyWhenValid { get; set; }
	[ConfigurationProperty("AlwaysAllowTransponderOverlay", DefaultValue = false, IsRequired = true)] public bool AlwaysAllowTransponderOverlay { get; set; }
	[ConfigurationProperty("PttReleaseDelay", DefaultValue = 0.0f, IsRequired = true)] public float PttReleaseDelay { get; set; }
	[ConfigurationProperty("PTTStartDelay", DefaultValue = 0.0f, IsRequired = true)] public float PTTStartDelay { get; set; }
	[ConfigurationProperty("RadioBackgroundNoiseEffect", DefaultValue = false, IsRequired = true)] public bool RadioBackgroundNoiseEffect { get; set; }
	[ConfigurationProperty("NatoFmToneVolume", DefaultValue = 1.2f, IsRequired = true)] public float NatoFmToneVolume { get; set; }
	[ConfigurationProperty("HqToneVolume", DefaultValue = 0.3f, IsRequired = true)] public float HqToneVolume { get; set; }
	[ConfigurationProperty("VhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)] public float VhfNoiseVolume { get; set; }
	[ConfigurationProperty("HfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)] public float HfNoiseVolume { get; set; }
	[ConfigurationProperty("UhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)] public float UhfNoiseVolume { get; set; }
	[ConfigurationProperty("FmNoiseVolume", DefaultValue = 0.4f, IsRequired = true)] public float FmNoiseVolume { get; set; }
	
	[ConfigurationProperty("AmCollisionToneVolume", DefaultValue = 1.0f, IsRequired = true)] public float AmCollisionToneVolume { get; set; }
	[ConfigurationProperty("RotaryStyleIncrement", DefaultValue = false, IsRequired = true)] public bool RotaryStyleIncrement { get; set; }
	[ConfigurationProperty("AmbientCockpitNoiseEffect", DefaultValue = true, IsRequired = true)] public bool AmbientCockpitNoiseEffect { get; set; }
	[ConfigurationProperty("AmbientCockpitNoiseEffectVolume", DefaultValue = 1.0f, IsRequired = true)] public float AmbientCockpitNoiseEffectVolume { get; set; }
	[ConfigurationProperty("AmbientCockpitIntercomNoiseEffect", DefaultValue = false, IsRequired = true)] public bool AmbientCockpitIntercomNoiseEffect { get; set; }
	[ConfigurationProperty("DisableExpansionRadios", DefaultValue = false, IsRequired = true)] public bool DisableExpansionRadios { get; set; }
	
	[ConfigurationProperty("Input Intercom", IsRequired = true)] public InputSettingsModel InputIntercom { get; set; }
	[ConfigurationProperty("Input Switch01", IsRequired = true)] public InputSettingsModel InputSwitch01 { get; set; }
	[ConfigurationProperty("Input Switch02", IsRequired = true)] public InputSettingsModel InputSwitch02 { get; set; }
	[ConfigurationProperty("Input Switch03", IsRequired = true)] public InputSettingsModel InputSwitch03 { get; set; }
	[ConfigurationProperty("Input Switch04", IsRequired = true)] public InputSettingsModel InputSwitch04 { get; set; }
	[ConfigurationProperty("Input Switch05", IsRequired = true)] public InputSettingsModel InputSwitch05 { get; set; }
	[ConfigurationProperty("Input Switch06", IsRequired = true)] public InputSettingsModel InputSwitch06 { get; set; }
	[ConfigurationProperty("Input Switch07", IsRequired = true)] public InputSettingsModel InputSwitch07 { get; set; }
	[ConfigurationProperty("Input Switch08", IsRequired = true)] public InputSettingsModel InputSwitch08 { get; set; }
	[ConfigurationProperty("Input Switch09", IsRequired = true)] public InputSettingsModel InputSwitch09 { get; set; }
	[ConfigurationProperty("Input Switch10", IsRequired = true)] public InputSettingsModel InputSwitch10 { get; set; }
	
	[ConfigurationProperty("Radio Next", IsRequired = true)] public InputSettingsModel RadioNext { get; set; }
	[ConfigurationProperty("Radio Previous", IsRequired = true)] public InputSettingsModel RadioPrevious { get; set; }
	
	[ConfigurationProperty("Input Intercom Ptt", IsRequired = true)] public InputSettingsModel InputIntercomPtt { get; set; }
	[ConfigurationProperty("Input Push To Talk", IsRequired = true)] public InputSettingsModel InputPushToTalk { get; set; }
	
	[ConfigurationProperty("Awacs Overlay Toggle", IsRequired = true)] public InputSettingsModel AwacsOverlayToggle { get; set; }
	[ConfigurationProperty("Overlay Toggle", IsRequired = true)] public InputSettingsModel OverlayToggle { get; set; }

	[ConfigurationProperty("Input Up100", IsRequired = true)] public InputSettingsModel InputUp100 { get; set; }
	[ConfigurationProperty("Input Up10", IsRequired = true)] public InputSettingsModel InputUp10 { get; set; }
	[ConfigurationProperty("Input Up1", IsRequired = true)] public InputSettingsModel InputUp1 { get; set; }
	[ConfigurationProperty("Input Up01", IsRequired = true)] public InputSettingsModel InputUp01 { get; set; }
	[ConfigurationProperty("Input Up001", IsRequired = true)] public InputSettingsModel InputUp001 { get; set; }
	[ConfigurationProperty("Input Up0001", IsRequired = true)] public InputSettingsModel InputUp0001 { get; set; }

	[ConfigurationProperty("Input Down100", IsRequired = true)] public InputSettingsModel InputDown100 { get; set; }
	[ConfigurationProperty("Input Down10", IsRequired = true)] public InputSettingsModel InputDown10 { get; set; }
	[ConfigurationProperty("Input Down1", IsRequired = true)] public InputSettingsModel InputDown1 { get; set; }
	[ConfigurationProperty("Input Down01", IsRequired = true)] public InputSettingsModel InputDown01 { get; set; }
	[ConfigurationProperty("Input Down001", IsRequired = true)] public InputSettingsModel InputDown001 { get; set; }
	[ConfigurationProperty("Input Down0001", IsRequired = true)] public InputSettingsModel InputDown0001 { get; set; }
	
	[ConfigurationProperty("Transponder Ident", IsRequired = true)] public InputSettingsModel TransponderIdent { get; set; }
	[ConfigurationProperty("Guard Toggle", IsRequired = true)] public InputSettingsModel GuardToggle { get; set; }
	[ConfigurationProperty("Encryption Toggle", IsRequired = true)] public InputSettingsModel EncryptionToggle { get; set; }
	[ConfigurationProperty("Encryption Key Up", IsRequired = true)] public InputSettingsModel EncryptionKeyUp { get; set; }
	[ConfigurationProperty("Encryption Key Down", IsRequired = true)] public InputSettingsModel EncryptionKeyDown { get; set; }
	
	[ConfigurationProperty("Radio Channel Up", IsRequired = true)] public InputSettingsModel RadioChannelUp { get; set; }
	[ConfigurationProperty("Radio Channel Down", IsRequired = true)] public InputSettingsModel RadioChannelDown { get; set; }
	
	[ConfigurationProperty("Radio Volume Up", IsRequired = true)] public InputSettingsModel RadioVolumeUp { get; set; }
	[ConfigurationProperty("Radio Volume Down", IsRequired = true)] public InputSettingsModel RadioVolumeDown { get; set; }
	
	public object Clone()
	{
		return this.MemberwiseClone();
	}
}