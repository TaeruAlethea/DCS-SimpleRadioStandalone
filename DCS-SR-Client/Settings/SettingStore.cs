namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class SettingStore : ISettingStore
{
    public GlobalSettingsModel GlobalSettingsModel { get; }
    public ProfileSettingsModel ProfileSettingsModel { get; }
    public ServerSettingsModel ServerSettingsModel { get; }
}