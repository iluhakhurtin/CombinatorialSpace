using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.BinaryVectorsBuilders
{
    public interface IBinaryVectorBuilder
    {
        /// <summary>
        /// Creates BitArray of a given length and with 
        /// the number of 1 equals to maskSize
        /// </summary>
        /// <param name="length">Length of the array.</param>
        /// <param name="truesCount">Number of 'True' in the array.</param>
        /// <returns></returns>
        BitArray BuildVector(int length, int truesCount);
    }
}
