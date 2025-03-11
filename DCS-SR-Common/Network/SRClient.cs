﻿using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Media;
using Ciribob.DCS.SimpleRadio.Standalone.Common.DCSState;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Helpers;
using Newtonsoft.Json;

namespace Ciribob.DCS.SimpleRadio.Standalone.Common.Network;

public class SRClient : INotifyPropertyChanged
{
	private int _coalition;

	[JsonIgnore] private float _lineOfSightLoss; // 0.0 is NO Loss therefore Full line of sight

	private string _name = "";

	// Used by server client list to display last frequency client transmitted on
	private string _transmittingFrequency;

	public string ClientGuid { get; set; }

	public string Name
	{
		get => _name;
		set
		{
			if (value == null || value == "") value = "---";

			if (_name != value)
			{
				_name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
			}
		}
	}

	public int Seat { get; set; }

	public int Coalition
	{
		get => _coalition;
		set
		{
			_coalition = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Coalition"));
		}
	}

	public bool AllowRecord { get; set; }

	[JsonIgnore] public string AllowRecordingStatus => AllowRecord ? "R" : "-";

	[JsonIgnore]
	public SolidColorBrush ClientCoalitionColour
	{
		get
		{
			switch (Coalition)
			{
				case 0:
					return new SolidColorBrush(Colors.White);
				case 1:
					return new SolidColorBrush(Colors.Red);
				case 2:
					return new SolidColorBrush(Colors.Blue);
				default:
					return new SolidColorBrush(Colors.White);
			}
		}
	}

	[JsonIgnore] public bool Muted { get; set; }

	[JsonIgnore] public long LastUpdate { get; set; }

	[JsonIgnore] public IPEndPoint VoipPort { get; set; }

	[JsonIgnore] public long LastRadioUpdateSent { get; set; }

	public DCSPlayerRadioInfo RadioInfo { get; set; }

	[JsonDCSIgnoreSerialization] public DCSLatLngPosition LatLngPosition { get; set; }

	[JsonIgnore]
	public float LineOfSightLoss
	{
		get
		{
			if (_lineOfSightLoss == 0) return 0;
			if (LatLngPosition.lat == 0 && LatLngPosition.lng == 0) return 0;
			return _lineOfSightLoss;
		}
		set => _lineOfSightLoss = value;
	}

	[JsonIgnore]
	public string TransmittingFrequency
	{
		get => _transmittingFrequency;
		set
		{
			if (_transmittingFrequency != value)
			{
				_transmittingFrequency = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransmittingFrequency"));
			}
		}
	}

	// Used by server client list to remove last frequency client transmitted on after threshold
	[JsonIgnore] public DateTime LastTransmissionReceived { get; set; }

	//is an SRSClientSession but dont want to include the dependancy for now
	[JsonIgnore] public object ClientSession { get; set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public override string ToString()
	{
		string side;

		if (Coalition == 1)
			side = "Red";
		else if (Coalition == 2)
			side = "Blue";
		else
			side = "Spectator";
		return Name == "" ? "Unknown" : Name + " - " + side + " LOS Loss " + _lineOfSightLoss + " Pos" + LatLngPosition;
	}
}