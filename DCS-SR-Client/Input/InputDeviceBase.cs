﻿using System;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;

public abstract class InputDeviceBase
{
	public InputBinding InputBind { get; set; }
	public string DeviceName { get; set; }
	public Guid InstanceGuid { get; internal set; }
	public abstract bool IsSameBind(InputDeviceBase compare);
}