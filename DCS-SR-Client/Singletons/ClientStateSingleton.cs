using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network.VAICOM.Models;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings.RadioChannels;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.RadioOverlayWindow.PresetChannels;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.DCSState;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons
{
    public sealed partial class ClientStateSingleton : ObservableObject
    {
        private static volatile ClientStateSingleton _instance;
        private static object _lock = new Object();

        public delegate bool RadioUpdatedCallback();

        private List<RadioUpdatedCallback> _radioCallbacks = new List<RadioUpdatedCallback>();

        [ObservableProperty] private DCSPlayerRadioInfo _dcsPlayerRadioInfo;
        [ObservableProperty] private DCSPlayerSideInfo _playerCoalitionLocationMetadata;

        // Timestamp the last UDP Game GUI broadcast was received from DCS, used for determining active game connection
        public long DcsGameGuiLastReceived { get; set; }

        // Timestamp the last UDP Export broadcast was received from DCS, used for determining active game connection
        public long DcsExportLastReceived { get; set; }

        // Timestamp for the last time 
        public long LotATCLastReceived { get; set; }

        //store radio channels here?
        public PresetChannelsViewModel[] FixedChannels { get; }

        public long LastSent { get; set; }

        public long LastPostionCoalitionSent { get; set; }

        private static readonly DispatcherTimer _timer = new DispatcherTimer();

        public RadioSendingState RadioSendingState { get; set; }
        public  RadioReceivingState[] RadioReceivingState { get; }

        [ObservableProperty] private bool _isConnected;

        [ObservableProperty] private bool _isVoipConnected;

        [ObservableProperty] private bool _isConnectionErrored = false;
        public string ShortGUID { get; }
        
        // Indicates the user's desire to be in External Awacs Mode or not
        public bool ExternalAWACSModelSelected { get; set; }

        // Indicates whether we are *actually* connected in External Awacs Mode
        // Used by the Name and Password related UI elements to determine if they are editable or not
        public bool ExternalAWACSModeConnected
        {
            get
            { 
                bool EamEnabled = SyncedServerSettings.Instance.GetSettingAsBool(Common.Setting.ServerSettingsKeys.EXTERNAL_AWACS_MODE);
                return IsConnected && EamEnabled && ExternalAWACSModelSelected && !IsGameExportConnected;
            }
        }

        public bool IsLotATCConnected { get { return LotATCLastReceived >= DateTime.Now.Ticks - 50000000; } }

        public bool IsGameGuiConnected { get { return DcsGameGuiLastReceived >= DateTime.Now.Ticks - 100000000; } }
        public bool IsGameExportConnected { get { return DcsExportLastReceived >= DateTime.Now.Ticks - 100000000; } }
        // Indicates an active game connection has been detected (1 tick = 100ns, 100000000 ticks = 10s stale timer), not updated by EAM
        public bool IsGameConnected { get { return IsGameGuiConnected && IsGameExportConnected; } }

        public string LastSeenName { get; set; }

        public VAICOMMessageWrapper InhibitTX { get; set; } = new VAICOMMessageWrapper(); //used to temporarily stop PTT for VAICOM

        private ClientStateSingleton()
        {
            RadioSendingState = new RadioSendingState();
            RadioReceivingState = new RadioReceivingState[11];

            ShortGUID = ShortGuid.NewGuid();
            DcsPlayerRadioInfo = new DCSPlayerRadioInfo();
            PlayerCoalitionLocationMetadata = new DCSPlayerSideInfo();

            // The following members are not updated due to events. Therefore we need to setup a polling action so that they are
            // periodically checked.
            DcsGameGuiLastReceived = 0;
            DcsExportLastReceived = 0;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(DcsPlayerRadioInfo));
                OnPropertyChanged(nameof(PlayerCoalitionLocationMetadata));
                
                OnPropertyChanged(nameof(IsGameConnected));
                OnPropertyChanged(nameof(IsLotATCConnected));
                OnPropertyChanged(nameof(ExternalAWACSModeConnected));
            };
            _timer.Start();

            FixedChannels = new PresetChannelsViewModel[10];

            for (int i = 0; i < FixedChannels.Length; i++)
            {
                FixedChannels[i] = new PresetChannelsViewModel(new FilePresetChannelsStore(), i + 1);
            }

            LastSent = 0;

            IsConnected = false;
            ExternalAWACSModelSelected = false;

            LastSeenName = Settings.GlobalSettingsStore.Instance.GetClientSetting(Settings.GlobalSettingsKeys.LastSeenName).RawValue;
        }

        public static ClientStateSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ClientStateSingleton();
                    }
                }

                return _instance;
            }
        }

        public int IntercomOffset { get; set; }
        
        public bool ShouldUseLotATCPosition()
        {
            if (!IsLotATCConnected)
            {
                return false;
            }

            if (IsGameExportConnected)
            {
                if (DcsPlayerRadioInfo.inAircraft)
                {
                    return false;
                }
            }

            return true;
        }

        public void ClearPositionsIfExpired()
        {
            //not game or Lotatc - clear it!
            if (!IsLotATCConnected && !IsGameExportConnected)
            {
                PlayerCoalitionLocationMetadata.LngLngPosition = new DCSLatLngPosition();
            }
        }

        public void UpdatePlayerPosition( DCSLatLngPosition latLngPosition)
        {
            PlayerCoalitionLocationMetadata.LngLngPosition = latLngPosition;
            DcsPlayerRadioInfo.latLng = latLngPosition;
            
        }

    }
}