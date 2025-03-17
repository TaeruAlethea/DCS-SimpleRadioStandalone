namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public interface ISettingStore
{
    public GlobalSettingsModel GlobalSettingsModel { get; }
    public ProfileSettingsModel ProfileSettingsModel { get; }
    public ServerSettingsModel ServerSettingsModel { get; }
}