namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public class SettingStore : ISettingStore
{
    public GlobalSettingsStore GlobalSettingsStore => GlobalSettingsStore.Instance;
    public SyncedServerSettings SyncedServerSettings => SyncedServerSettings.Instance;
}