using System;
using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public partial class ProfileSettingsModel : ConfigurationSection, ICloneable
{
	[ConfigurationProperty("ProfileName", DefaultValue = "Default", IsRequired = true)]
	public string ProfileName { get; set; }
	
	[ConfigurationProperty("RadioEffects", DefaultValue = true, IsRequired = true)]
	public bool RadioEffects { get; set; }
	[ConfigurationProperty("RadioEffectsClipping", DefaultValue = false, IsRequired = true)]
	public bool RadioEffectsClipping { get; set; }
	
	[ConfigurationProperty("RadioEncryptionEffects", DefaultValue = true, IsRequired = true)]
	public bool RadioEncryptionEffects { get; set; }
	[ConfigurationProperty("NatoFmTone", DefaultValue = true, IsRequired = true)]
	public bool NatoFmTone { get; set; }
	[ConfigurationProperty("HaveQuickTone", DefaultValue = true, IsRequired = true)]
	public bool HaveQuickTone { get; set; }
	
	[ConfigurationProperty("RadioRxEffects_Start", DefaultValue = true, IsRequired = true)]
	public bool RadioRxEffects_Start { get; set; }
	[ConfigurationProperty("RadioRxEffects_End", DefaultValue = true, IsRequired = true)]
	public bool RadioRxEffects_End { get; set; }
	
	//todo RadioTransmissionStartSelection
	//todo RadioTransmissionEndSelection
	
	[ConfigurationProperty("RadioTxEffects_Start", DefaultValue = true, IsRequired = true)]
	public bool RadioTxEffects_Start { get; set; }
	[ConfigurationProperty("RadioTxEffects_End", DefaultValue = true, IsRequired = true)]
	public bool RadioTxEffects_End { get; set; }
	[ConfigurationProperty("MidsRadioEffect", DefaultValue = true, IsRequired = true)]
	public bool MidsRadioEffect { get; set; }
	
	[ConfigurationProperty("AutoSelectPresetChannel", DefaultValue = true, IsRequired = true)]
	public bool AutoSelectPresetChannel { get; set; }
	
	[ConfigurationProperty("AlwaysAllowHotasControls", DefaultValue = false, IsRequired = true)]
	public bool AlwaysAllowHotasControls { get; set; }
	[ConfigurationProperty("AllowDcsPtt", DefaultValue = true, IsRequired = true)]
	public bool AllowDcsPtt { get; set; }
	[ConfigurationProperty("RadioSwitchIsPtt", DefaultValue = false, IsRequired = true)]
	public bool RadioSwitchIsPtt { get; set; }
	[ConfigurationProperty("RadioSwitchIsPttOnlyWhenValid", DefaultValue = false, IsRequired = true)]
	public bool RadioSwitchIsPttOnlyWhenValid { get; set; }
	[ConfigurationProperty("AlwaysAllowTransponderOverlay", DefaultValue = false, IsRequired = true)]
	public bool AlwaysAllowTransponderOverlay { get; set; }
	
	[ConfigurationProperty("PttReleaseDelay", DefaultValue = 0.0f, IsRequired = true)]
	public float PttReleaseDelay { get; set; }
	[ConfigurationProperty("PTTStartDelay", DefaultValue = 0.0f, IsRequired = true)]
	public float PTTStartDelay { get; set; }
	
	[ConfigurationProperty("RadioBackgroundNoiseEffect", DefaultValue = false, IsRequired = true)]
	public bool RadioBackgroundNoiseEffect { get; set; }
	
	[ConfigurationProperty("NatoFmToneVolume", DefaultValue = 1.2f, IsRequired = true)]
	public float NatoFmToneVolume { get; set; }
	[ConfigurationProperty("HqToneVolume", DefaultValue = 0.3f, IsRequired = true)]
	public float HqToneVolume { get; set; }
	
	[ConfigurationProperty("VhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float VhfNoiseVolume { get; set; }
	[ConfigurationProperty("HfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float HfNoiseVolume { get; set; }
	[ConfigurationProperty("UhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float UhfNoiseVolume { get; set; }
	[ConfigurationProperty("FmNoiseVolume", DefaultValue = 0.4f, IsRequired = true)]
	public float FmNoiseVolume { get; set; }
	
	[ConfigurationProperty("AmCollisionToneVolume", DefaultValue = 1.0f, IsRequired = true)]
	public float AmCollisionToneVolume { get; set; }
	
	[ConfigurationProperty("RotaryStyleIncrement", DefaultValue = false, IsRequired = true)]
	public bool RotaryStyleIncrement { get; set; }
	
	[ConfigurationProperty("AmbientCockpitNoiseEffect", DefaultValue = true, IsRequired = true)]
	public bool AmbientCockpitNoiseEffect { get; set; }
	[ConfigurationProperty("AmbientCockpitNoiseEffectVolume", DefaultValue = 1.0f, IsRequired = true)]
	public float AmbientCockpitNoiseEffectVolume { get; set; }
	[ConfigurationProperty("AmbientCockpitIntercomNoiseEffect", DefaultValue = false, IsRequired = true)]
	public bool AmbientCockpitIntercomNoiseEffect { get; set; }
	[ConfigurationProperty("DisableExpansionRadios", DefaultValue = false, IsRequired = true)]
	public bool DisableExpansionRadios { get; set; }

	public object Clone()
	{
		return this.MemberwiseClone();
	}
}