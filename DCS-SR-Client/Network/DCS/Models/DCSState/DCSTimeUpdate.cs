using System;
using System.Diagnostics;
using System.Globalization;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Network.DCS.Models.DCSState;

public class DCSTimeUpdate
{
	public uint Year;
	public uint Month;
	public uint Day;
	public uint Start_time;
	public uint Model_time;
	
	public static DateTime Iso8691Builder(DCSTimeUpdate dcsTimeUpdate)
	{
		var stamp = new DateTime(
			(int)dcsTimeUpdate.Year, 
			(int)dcsTimeUpdate.Month, 
			(int)dcsTimeUpdate.Day, 
			0, 0, // Hours and Minutes
			second: (int)dcsTimeUpdate.Start_time)
			.AddSeconds((int)dcsTimeUpdate.Model_time);
		
		Debug.WriteLine(stamp.ToString(CultureInfo.InvariantCulture));

		return stamp;
	}
}