using System;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;


public class InputSettingsModel : ICloneable
{
	public string InputName { get; set; } = "InputName";
	public InputModel Primary { get; set; } = new InputModel();
	public InputModel Modifier { get; set; } = new InputModel();
	
	public class InputModel : ICloneable
	{
		public Guid Guid { get; internal set; } = Guid.Empty;
		public string DeviceName { get; set; } = string.Empty;
		public int Button { get; set; } = (int)0;
		public int ButtonValue { get; internal set; } = (int)0;
	
		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
	
	public object Clone()
	{
		return this.MemberwiseClone();
	}
}
