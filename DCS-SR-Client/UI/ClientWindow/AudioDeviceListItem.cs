﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow
{
    public class AudioDeviceListItem
    {
        public string Text { get; set; } // Name?
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}