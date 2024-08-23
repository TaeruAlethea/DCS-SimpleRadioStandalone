namespace Ciribob.DCS.SimpleRadio.Standalone.Common
{
	public class Link16Helper
	{
		public static readonly double link16BaseFreq = 1030.0 * 1000000; //Start at UHF 300
		public static readonly double link16FreqSeperation = 1.0 * 100000; //0.1 MHZ between MIDS channels

		public static double Link16ToFrequency(int channel)
		{
			return (link16BaseFreq + (link16FreqSeperation * channel));
		}
        
		public static int FrequencyToLink16(double frequency)
		{
			return (int)((frequency - link16BaseFreq) / link16FreqSeperation);
		}
	}
}