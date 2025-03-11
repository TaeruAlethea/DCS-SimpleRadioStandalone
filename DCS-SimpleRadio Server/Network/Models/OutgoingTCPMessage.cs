using System.Collections.Generic;
using System.Net.Sockets;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;

namespace Ciribob.DCS.SimpleRadio.Standalone.Server.Network.Models;

public class OutgoingTCPMessage
{
	public NetworkMessage NetworkMessage { get; set; }

	public List<Socket> SocketList { get; set; }
}