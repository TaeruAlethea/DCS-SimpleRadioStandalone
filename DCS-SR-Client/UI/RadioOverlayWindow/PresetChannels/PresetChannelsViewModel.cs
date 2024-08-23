﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings.RadioChannels;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.PresetChannels;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Utils;
using Ciribob.DCS.SimpleRadio.Standalone.Common;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI.RadioOverlayWindow.PresetChannels
{
    public class PresetChannelsViewModel: INotifyPropertyChanged
    {
        private IPresetChannelsStore _channelsStore;
        private int _radioId;

        public DelegateCommand DropDownClosedCommand { get; set; }

        public DelegateCommand PresetCreateCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility ShowPresetCreate
        {
            get => _showPresetCreate;
            set
            {
                _showPresetCreate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowPresetCreate"));
            }
        }


        private readonly object _presetChannelLock = new object();
        private ObservableCollection<PresetChannel> _presetChannels;
        private Visibility _showPresetCreate = Visibility.Visible;

        public ObservableCollection<PresetChannel> PresetChannels
        {
            get { return _presetChannels; }
            set
            {
                _presetChannels = value;
                BindingOperations.EnableCollectionSynchronization(_presetChannels, _presetChannelLock);
            }
        }

        public int RadioId
        {
            private get { return _radioId; }
            set
            {
                _radioId = value;
                Reload();
            }
        }

        public PresetChannelsViewModel(IPresetChannelsStore channels, int radioId)
        {
            _radioId = radioId;
            _channelsStore = channels;
            ReloadCommand = new DelegateCommand(OnReload);
            DropDownClosedCommand = new DelegateCommand(DropDownClosed);
            PresetChannels = new ObservableCollection<PresetChannel>();
            PresetCreateCommand = new DelegateCommand(CreatePreset);
        }


        public ICommand ReloadCommand { get; }

        private void DropDownClosed(object args)
        {
            if (SelectedPresetChannel != null
                && SelectedPresetChannel.Value is Double
                && (Double) SelectedPresetChannel.Value > 0 && RadioId > 0)
            {
                RadioHelper.SelectRadioChannel(SelectedPresetChannel, RadioId);
            }
        }

        public PresetChannel SelectedPresetChannel { get; set; }

        public double Max { get; set; }
        public double Min { get; set; }

        public void Reload()
        {
            PresetChannels.Clear();
            ShowPresetCreate = Visibility.Collapsed;

            var radios = ClientStateSingleton.Instance.DcsPlayerRadioInfo.radios;

            var radio = radios[_radioId];

            if (radio.modulation != RadioInformation.Modulation.MIDS)
            {
                int i = 1;
                foreach (var channel in _channelsStore.LoadFromStore(radio.name))
                {
                    if (((double)channel.Value) <= Max
                        && ((double)channel.Value) >= Min)
                    {
                        channel.Channel = i++;
                        PresetChannels.Add(channel);
                    }
                }

                if (PresetChannels.Count > 0)
                {
                    ShowPresetCreate = Visibility.Collapsed;
                }
                else
                {
                    ShowPresetCreate = Visibility.Visible;
                }
            }
            else
            {
                
               int i = 1;
               foreach (var channel in _channelsStore.LoadFromStore(radio.name,true))
               {
                   channel.Channel = i++;
                   PresetChannels.Add(channel);
               }

               if (PresetChannels.Count == 0)
               {
                   for (int chn = 1; chn < 126; chn++)
                   {
                       PresetChannels.Add(new PresetChannel
                       {
                           Channel = chn,
                           Text = "MIDS " + chn,
                           Value = (chn * Link16Helper.FreqSeperation) + Link16Helper.BaseFreq
                       });
                   }

                   ShowPresetCreate = Visibility.Visible;
               }
               else
               {
                   ShowPresetCreate = Visibility.Collapsed;
               }
            }

        }

        private void OnReload()
        {
            Reload();
        }

        private void CreatePreset()
        {
            var radios = ClientStateSingleton.Instance.DcsPlayerRadioInfo.radios;

            var radio = radios[_radioId];

            if (radio.modulation != RadioInformation.Modulation.DISABLED && radio.modulation != RadioInformation.Modulation.INTERCOM)
            {
                var path = _channelsStore.CreatePresetFile(radio.name);
                if (path != null)
                {
                    var res = MessageBox.Show($"Created presets file at path:\n {path} \n\nOpen the file?","Created Preset File",MessageBoxButton.YesNo,MessageBoxImage.Information,MessageBoxResult.No);
                    if (res == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(path);
                        }
                        catch(Exception ex) { }
                    }
                }
            }
        }

        public void Clear()
        {
            PresetChannels.Clear();
        }
    }
}