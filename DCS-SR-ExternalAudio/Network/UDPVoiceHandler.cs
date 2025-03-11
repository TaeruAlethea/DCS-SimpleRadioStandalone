using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Common;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.ExternalAudioClient.Network;

internal class UdpVoiceHandler
{
	private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	private readonly IPAddress _address;

	private readonly byte[] _guidAsciiBytes;
	private readonly CancellationTokenSource _pingStop = new();
	private readonly int _port;

	private readonly CancellationTokenSource _stopFlag = new();
	private readonly DCSPlayerRadioInfo gameState;

	//    private readonly JitterBuffer _jitterBuffer = new JitterBuffer();
	private UdpClient _listener;

	private ulong _packetNumber = 1;

	private readonly IPEndPoint _serverEndpoint;

	private volatile bool _stop;


	public UdpVoiceHandler(string guid, IPAddress address, int port, DCSPlayerRadioInfo gameState)
	{
		_guidAsciiBytes = Encoding.ASCII.GetBytes(guid);

		_address = address;
		_port = port;
		this.gameState = gameState;

		_serverEndpoint = new IPEndPoint(_address, _port);
	}


	public void Start()
	{
		_listener = new UdpClient();
		try
		{
			_listener.AllowNatTraversal(true);
		}
		catch
		{
		}

		_packetNumber = 1; //reset packet number


		var message = _guidAsciiBytes;

		Logger.Info("Sending UDP Ping");
		// Force immediate ping once to avoid race condition before starting to listen
		_listener.Send(message, message.Length, _serverEndpoint);

		Thread.Sleep(3000);
		Logger.Info("Ping Sent");
	}

	public void RequestStop()
	{
		_stop = true;
		try
		{
			_listener?.Close();
		}
		catch (Exception)
		{
		}

		_stopFlag.Cancel();
	}

	public bool Send(byte[] bytes, int len, double[] freq, byte[] modulation)
	{
		if (!_stop
		    && _listener != null
		    && bytes != null)
			//can only send if IL2 is connected
			try
			{
				//generate packet
				var udpVoicePacket = new UDPVoicePacket
				{
					GuidBytes = _guidAsciiBytes,
					AudioPart1Bytes = bytes,
					AudioPart1Length = (ushort)bytes.Length,
					Frequencies = freq,
					UnitId = gameState.unitId,
					Modulations = modulation,
					PacketNumber = _packetNumber++,
					OriginalClientGuidBytes = _guidAsciiBytes,
					RetransmissionCount = 0,
					Encryptions = new byte[] { 0 }
				};

				var encodedUdpVoicePacket = udpVoicePacket.EncodePacket();

				_listener?.Send(encodedUdpVoicePacket, encodedUdpVoicePacket.Length, new IPEndPoint(_address, _port));
			}
			catch (Exception e)
			{
				Logger.Error(e, "Exception Sending Audio Message " + e.Message);
			}

		//couldnt send
		return false;
	}
}