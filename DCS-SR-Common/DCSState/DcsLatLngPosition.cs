﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ciribob.DCS.SimpleRadio.Standalone.Common.DCSState;

public class DCSLatLngPosition
{
    public double lat;
    public double lng;
    public double alt;

    public bool isValid()
    {
        return lat != 0 && lng != 0;
    }

    public override string ToString()
    {
        return $"Pos:[{lat},{lng},{alt}]";
    }

    public string PrettyPrint => $"Lat/Lng: {lat:0.###},{lng:0.###} - Alt: {alt:0}";
}

