using Ciribob.DCS.SimpleRadio.Standalone.Common.DCSState;

namespace Ciribob.DCS.SimpleRadio.Standalone.Common;

public class DCSPlayerSideInfo
{
	public string name = "";
	public int seat; // 0 is front / normal - 1 is back seat
	public int side;

	public DCSLatLngPosition LngLngPosition { get; set; } = new();

	public override bool Equals(object obj)
	{
		if (obj == null) return false;

		return obj is DCSPlayerSideInfo info &&
		       name == info.name &&
		       side == info.side &&
		       seat == info.seat;
	}

	public void Reset()
	{
		name = "";
		side = 0;
		seat = 0;
		LngLngPosition = new DCSLatLngPosition();
	}
}