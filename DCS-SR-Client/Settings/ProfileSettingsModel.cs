using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class ProfileSettingsModel(ProfileSettingsStore store) : ConfigurationSection
{
	[ConfigurationProperty("RadioEffects", DefaultValue = true, IsRequired = true)]
	public bool RadioEffects
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioEffects);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioEffects, value);
	}
	[ConfigurationProperty("RadioEffectsClipping", DefaultValue = false, IsRequired = true)]
	public bool RadioEffectsClipping
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioEffectsClipping);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioEffectsClipping, value);
	}
	
	[ConfigurationProperty("RadioEncryptionEffects", DefaultValue = true, IsRequired = true)]
	public bool RadioEncryptionEffects
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioEncryptionEffects);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioEncryptionEffects, value);
	}
	[ConfigurationProperty("NatoFmTone", DefaultValue = true, IsRequired = true)]
	public bool NatoFmTone
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.NATOTone);
		set => store.SetClientSettingBool(ProfileSettingsKeys.NATOTone, value);
	}
	[ConfigurationProperty("HaveQuickTone", DefaultValue = true, IsRequired = true)]
	public bool HaveQuickTone
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.HAVEQUICKTone);
		set => store.SetClientSettingBool(ProfileSettingsKeys.HAVEQUICKTone, value);
	}
	
	[ConfigurationProperty("RadioRxEffects_Start", DefaultValue = true, IsRequired = true)]
	public bool RadioRxEffects_Start
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_Start);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_Start, value);
	}
	[ConfigurationProperty("RadioRxEffects_End", DefaultValue = true, IsRequired = true)]
	public bool RadioRxEffects_End
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_End);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioRxEffects_End, value);
	}
	
	//todo RadioTransmissionStartSelection
	//todo RadioTransmissionEndSelection
	
	[ConfigurationProperty("RadioTxEffects_Start", DefaultValue = true, IsRequired = true)]
	public bool RadioTxEffects_Start
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_Start);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_Start, value);
	}
	[ConfigurationProperty("RadioTxEffects_End", DefaultValue = true, IsRequired = true)]
	public bool RadioTxEffects_End
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_End);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioTxEffects_End, value);
	}
	[ConfigurationProperty("MidsRadioEffect", DefaultValue = true, IsRequired = true)]
	public bool MidsRadioEffect
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.MIDSRadioEffect);
		set => store.SetClientSettingBool(ProfileSettingsKeys.MIDSRadioEffect, value);
	}
	
	[ConfigurationProperty("AutoSelectPresetChannel", DefaultValue = true, IsRequired = true)]
	public bool AutoSelectPresetChannel
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.AutoSelectPresetChannel);
		set => store.SetClientSettingBool(ProfileSettingsKeys.AutoSelectPresetChannel, value);
	}
	
	[ConfigurationProperty("AlwaysAllowHotasControls", DefaultValue = false, IsRequired = true)]
	public bool AlwaysAllowHotasControls
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.AlwaysAllowHotasControls);
		set => store.SetClientSettingBool(ProfileSettingsKeys.AlwaysAllowHotasControls, value);
	}
	[ConfigurationProperty("AllowDcsPtt", DefaultValue = true, IsRequired = true)]
	public bool AllowDcsPtt
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.AllowDCSPTT);
		set => store.SetClientSettingBool(ProfileSettingsKeys.AllowDCSPTT, value);
	}
	[ConfigurationProperty("RadioSwitchIsPtt", DefaultValue = false, IsRequired = true)]
	public bool RadioSwitchIsPtt
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTT);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTT, value);
	}
	[ConfigurationProperty("RadioSwitchIsPttOnlyWhenValid", DefaultValue = false, IsRequired = true)]
	public bool RadioSwitchIsPttOnlyWhenValid
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTTOnlyWhenValid);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTTOnlyWhenValid, value);
	}
	[ConfigurationProperty("AlwaysAllowTransponderOverlay", DefaultValue = false, IsRequired = true)]
	public bool AlwaysAllowTransponderOverlay
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTTOnlyWhenValid);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioSwitchIsPTTOnlyWhenValid, value);
	}
	
	[ConfigurationProperty("PttReleaseDelay", DefaultValue = 0.0f, IsRequired = true)]
	public float PttReleaseDelay
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.PTTReleaseDelay);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.PTTReleaseDelay, value);
	}
	[ConfigurationProperty("PTTStartDelay", DefaultValue = 0.0f, IsRequired = true)]
	public float PTTStartDelay
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.PTTStartDelay);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.PTTStartDelay, value);
	}
	
	[ConfigurationProperty("RadioBackgroundNoiseEffect", DefaultValue = false, IsRequired = true)]
	public bool RadioBackgroundNoiseEffect
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RadioBackgroundNoiseEffect);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RadioBackgroundNoiseEffect, value);
	}
	
	[ConfigurationProperty("NatoFmToneVolume", DefaultValue = 1.2f, IsRequired = true)]
	public float NatoFmToneVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.NATOToneVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.NATOToneVolume, value);
	}
	[ConfigurationProperty("HqToneVolume", DefaultValue = 0.3f, IsRequired = true)]
	public float HqToneVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.HQToneVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.HQToneVolume, value);
	}
	
	[ConfigurationProperty("VhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float VhfNoiseVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.VHFNoiseVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.VHFNoiseVolume, value);
	}
	[ConfigurationProperty("HfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float HfNoiseVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.HFNoiseVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.HFNoiseVolume, value);
	}
	[ConfigurationProperty("UhfNoiseVolume", DefaultValue = 0.15f, IsRequired = true)]
	public float UhfNoiseVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.UHFNoiseVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.UHFNoiseVolume, value);
	}
	[ConfigurationProperty("FmNoiseVolume", DefaultValue = 0.4f, IsRequired = true)]
	public float FmNoiseVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.FMNoiseVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.FMNoiseVolume, value);
	}
	
	[ConfigurationProperty("AmCollisionToneVolume", DefaultValue = 1.0f, IsRequired = true)]
	public float AmCollisionToneVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.AMCollisionVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.AMCollisionVolume, value);
	}
	
	[ConfigurationProperty("RotaryStyleIncrement", DefaultValue = false, IsRequired = true)]
	public bool RotaryStyleIncrement
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.RotaryStyleIncrement);
		set => store.SetClientSettingBool(ProfileSettingsKeys.RotaryStyleIncrement, value);
	}
	
	[ConfigurationProperty("AmbientCockpitNoiseEffect", DefaultValue = true, IsRequired = true)]
	public bool AmbientCockpitNoiseEffect
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.AmbientCockpitNoiseEffect);
		set => store.SetClientSettingBool(ProfileSettingsKeys.AmbientCockpitNoiseEffect, value);
	}
	[ConfigurationProperty("AmbientCockpitNoiseEffectVolume", DefaultValue = 1.0f, IsRequired = true)]
	public float AmbientCockpitNoiseEffectVolume
	{
		get => store.GetClientSettingFloat(ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume);
		set => store.SetClientSettingFloat(ProfileSettingsKeys.AmbientCockpitNoiseEffectVolume, value);
	}
	[ConfigurationProperty("AmbientCockpitIntercomNoiseEffect", DefaultValue = false, IsRequired = true)]
	public bool AmbientCockpitIntercomNoiseEffect
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.AmbientCockpitIntercomNoiseEffect);
		set => store.SetClientSettingBool(ProfileSettingsKeys.AmbientCockpitIntercomNoiseEffect, value);
	}
	[ConfigurationProperty("DisableExpansionRadios", DefaultValue = false, IsRequired = true)]
	public bool DisableExpansionRadios
	{
		get => store.GetClientSettingBool(ProfileSettingsKeys.DisableExpansionRadios);
		set => store.SetClientSettingBool(ProfileSettingsKeys.DisableExpansionRadios, value);
	}
}