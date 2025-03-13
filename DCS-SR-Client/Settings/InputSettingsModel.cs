using System;
using System.Configuration;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;


public class InputSettingsModel : ConfigurationSection, ICloneable
{
	public InputModel Primary { get; set; }
	public InputModel Modifier { get; set; }
	
	public class InputModel : ConfigurationSection, ICloneable
	{
		[ConfigurationProperty("Device Guid", DefaultValue = "", IsRequired = true)]
		public Guid Guid { get; internal set; }
	
		[ConfigurationProperty("Device Name", DefaultValue = "", IsRequired = false)]
		public string DeviceName { get; set; }
	
		[ConfigurationProperty("Button", DefaultValue = (int)0, IsRequired = true)]
		public int Button { get; set; }
	
		[ConfigurationProperty("ButtonValue", DefaultValue = (int)0, IsRequired = false)]
		public int ButtonValue { get; internal set; }
	
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
