namespace Ciribob.DCS.SimpleRadio.Standalone.Common
{
	public class Link16Helper
	{
		public static readonly double BaseFreq = 1030.0 * 1000000; //Start at UHF 300
		public static readonly double FreqSeperation = 1.0 * 100000; //0.1 MHZ between MIDS channels

		public static readonly int MinimumChannel = 0;
		public static readonly int MaximumChannel = 126;

		public static readonly double MinimumFreq = Link16ToFrequency(MinimumChannel);
		public static readonly double MaximumFreq = Link16ToFrequency(MaximumChannel);
		
		public static double Link16ToFrequency(int channel)
		{
			return (BaseFreq + (FreqSeperation * channel));
		}
        
		public static int FrequencyToLink16(double frequency)
		{
			return (int)((frequency - BaseFreq) / FreqSeperation);
		}
	}
}