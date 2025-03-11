using System;
using Newtonsoft.Json;

namespace Ciribob.DCS.SimpleRadio.Standalone.Common;

public class RadioReceivingState
{
	[JsonIgnore] public long LastReceviedAt { get; set; }

	public bool IsSecondary { get; set; }
	public bool IsSimultaneous { get; set; }
	public int ReceivedOn { get; set; }

	public string SentBy { get; set; }

	public bool IsReceiving => DateTime.Now.Ticks - LastReceviedAt < 3500000;
}