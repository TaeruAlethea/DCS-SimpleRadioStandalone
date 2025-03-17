namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public interface ISettingStore
{
    public GlobalSettingsStore GlobalSettingsStore { get; }
    public SyncedServerSettings SyncedServerSettings { get; }
}