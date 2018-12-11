using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public interface IPoint
    {
        IEnumerable<ICluster> Clusters { get; }
        void Check(BitArray inputVector, BitArray outputVector);
    }
}
