using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Setting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons
{
    public sealed partial class ConnectedClientsSingleton : ObservableObject
    {
        [ObservableProperty] private ConcurrentDictionary<string, SRClient> _clients = new ConcurrentDictionary<string, SRClient>();
        private static volatile ConnectedClientsSingleton _instance;
        private static object _lock = new Object();
        private readonly string _guid = ClientStateSingleton.Instance.ShortGUID;
        private readonly SyncedServerSettings _serverSettings = SyncedServerSettings.Instance;

        private ConnectedClientsSingleton() { }

        public static ConnectedClientsSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new ConnectedClientsSingleton();
                    }
                }

                return _instance;
            }
        }

        public void NotifyAll()
        {
            OnPropertyChanged(nameof(Total));
        }

        public SRClient this[string key]
        {
            get
            {
                return Clients[key];
            }
            set
            {
                Clients[key] = value;
               NotifyAll();
            }
        }

        public ICollection<SRClient> Values
        {
            get
            {
                return Clients.Values;
            }
        }

        public int Total
        {
            get
            {
                return Clients.Count();
            }
        }

        public bool TryRemove(string key, out SRClient value)
        {
            bool result = Clients.TryRemove(key, out value);
            if (result)
            {
                OnPropertyChanged(nameof(Total));
            }
            return result;
        }

        public void Clear()
        {
            Clients.Clear();
            OnPropertyChanged(nameof(Total));
        }

        public bool TryGetValue(string key, out SRClient value)
        {
            return Clients.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return Clients.ContainsKey(key);
        }

        public int ClientsOnFreq(double freq, RadioInformation.Modulation modulation)
        {
            if (!_serverSettings.GetSettingAsBool(ServerSettingsKeys.SHOW_TUNED_COUNT))
            {
                return 0;
            }
            var currentClientPos = ClientStateSingleton.Instance.PlayerCoaltionLocationMetadata;
            var currentUnitId = ClientStateSingleton.Instance.DcsPlayerRadioInfo.unitId;
            var coalitionSecurity = SyncedServerSettings.Instance.GetSettingAsBool(ServerSettingsKeys.COALITION_AUDIO_SECURITY);
            var globalFrequencies = _serverSettings.GlobalFrequencies;
            var global = globalFrequencies.Contains(freq);
            int count = 0;

            foreach (var client in Clients)
            {
                if (!client.Key.Equals(_guid))
                {
                    // check that either coalition radio security is disabled OR the coalitions match
                    if (global|| (!coalitionSecurity || (client.Value.Coalition == currentClientPos.side)))
                    {

                        var radioInfo = client.Value.RadioInfo;

                        if (radioInfo != null)
                        {
                            RadioReceivingState radioReceivingState = null;
                            bool decryptable;
                            var receivingRadio = radioInfo.CanHearTransmission(freq,
                                modulation,
                                0,
                                false,
                                currentUnitId,
                                new List<int>(),
                                out radioReceivingState,
                                out decryptable);

                            //only send if we can hear!
                            if (receivingRadio != null)
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }
    }
}
