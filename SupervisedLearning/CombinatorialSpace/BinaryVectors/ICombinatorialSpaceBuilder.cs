﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public interface ICombinatorialSpaceBuilder
    {
        IEnumerable<IPoint> Build(int combinatorialSpaceLength, int numberOfTrackingBits, int trackingBinaryVectorLength);
    }
}