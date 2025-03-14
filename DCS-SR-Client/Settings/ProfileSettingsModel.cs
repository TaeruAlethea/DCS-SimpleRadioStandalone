using System;
using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class ProfileSettingsModel : ICloneable
{
	public string ProfileName { get; set; } = "Default";
	public bool RadioEffects { get; set; } = true;
	public bool RadioEffectsClipping { get; set; } = false;
	public bool RadioEncryptionEffects { get; set; } = true;
	public bool NatoFmTone { get; set; } = true;
	public bool HaveQuickTone { get; set; } = true;
	public bool RadioRxEffects_Start { get; set; } = true;
	public bool RadioRxEffects_End { get; set; } = true;
	
	//todo RadioTransmissionStartSelection
	//todo RadioTransmissionEndSelection
	
	public bool RadioTxEffects_Start { get; set; } = true;
	public bool RadioTxEffects_End { get; set; } = true;
	public bool MidsRadioEffect { get; set; } = true;
	
	public bool AutoSelectPresetChannel { get; set; } = true;
	public bool AlwaysAllowHotasControls { get; set; } = false;

	public bool AllowDcsPtt { get; set; } = true;
	public bool RadioSwitchIsPtt { get; set; } = false;
	public bool RadioSwitchIsPttOnlyWhenValid { get; set; } =  false;
	public bool AlwaysAllowTransponderOverlay { get; set; } = false;

	public float PttReleaseDelay { get; set; } = 0.0f;
	public float PTTStartDelay { get; set; } = 0.0f;
	public bool RadioBackgroundNoiseEffect { get; set; } = false;
	
	public float NatoFmToneVolume { get; set; } = 1.2f;
	public float HqToneVolume { get; set; } = 0.3f;
	public float VhfNoiseVolume { get; set; } = 0.15f;
	public float HfNoiseVolume { get; set; } = 0.15f;
	public float UhfNoiseVolume { get; set; } = 0.15f;
	public float FmNoiseVolume { get; set; } = 0.4f;
	
	public float AmCollisionToneVolume { get; set; } = 1.0f;
	public bool RotaryStyleIncrement { get; set; } = false;
	public bool AmbientCockpitNoiseEffect { get; set; } = true;
	public float AmbientCockpitNoiseEffectVolume { get; set; } = 1.0f;
	public bool AmbientCockpitIntercomNoiseEffect { get; set; } = false;
	public bool DisableExpansionRadios { get; set; } = false;
	
	public InputSettingsModel InputIntercom { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch01 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch02 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch03 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch04 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch05 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch06 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch07 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch08 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch09 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputSwitch10 { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel RadioNext { get; set; } = new InputSettingsModel();
	public InputSettingsModel RadioPrevious { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel InputIntercomPtt { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputPushToTalk { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel AwacsOverlayToggle { get; set; } = new InputSettingsModel();
	public InputSettingsModel OverlayToggle { get; set; } = new InputSettingsModel();

	public InputSettingsModel InputUp100 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputUp10 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputUp1 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputUp01 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputUp001 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputUp0001 { get; set; } = new InputSettingsModel();

	public InputSettingsModel InputDown100 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputDown10 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputDown1 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputDown01 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputDown001 { get; set; } = new InputSettingsModel();
	public InputSettingsModel InputDown0001 { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel TransponderIdent { get; set; } = new InputSettingsModel();
	public InputSettingsModel GuardToggle { get; set; } = new InputSettingsModel();
	public InputSettingsModel EncryptionToggle { get; set; } = new InputSettingsModel();
	public InputSettingsModel EncryptionKeyUp { get; set; } = new InputSettingsModel();
	public InputSettingsModel EncryptionKeyDown { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel RadioChannelUp { get; set; } = new InputSettingsModel();
	public InputSettingsModel RadioChannelDown { get; set; } = new InputSettingsModel();
	
	public InputSettingsModel RadioVolumeUp { get; set; } = new InputSettingsModel();
	public InputSettingsModel RadioVolumeDown { get; set; } = new InputSettingsModel();
	
	public object Clone()
	{
		return this.MemberwiseClone();
	}
}