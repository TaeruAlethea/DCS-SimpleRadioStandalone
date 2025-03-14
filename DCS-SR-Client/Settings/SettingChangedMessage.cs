namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class SettingChangedMessage(SettingChangedMessage.SettingChangeType settingChangeType)
{
	public readonly SettingChangeType changeType = settingChangeType;

	public enum SettingChangeType
	{
		Global, Profile
	}
}