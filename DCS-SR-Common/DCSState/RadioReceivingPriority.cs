namespace Ciribob.DCS.SimpleRadio.Standalone.Common;

public class RadioReceivingPriority
{
	public bool Decryptable;
	public byte Encryption;
	public double Frequency;
	public float LineOfSightLoss;
	public short Modulation;
	public double ReceivingPowerLossPercent;
	public RadioInformation ReceivingRadio;

	public RadioReceivingState ReceivingState;
}