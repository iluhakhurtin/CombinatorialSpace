using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.Strategies
{
    static class BitArrayCheckStrategy
    {
        public static void CheckBitArray(BitArray vector, IEnumerable<int> trackingBitsIndexes, int activationThreshold, Action<BitArray> callback)
        {
            int activeTrackingBitsNumber = 0;

            foreach (int trackingBitIdx in trackingBitsIndexes)
            {
                if (vector[trackingBitIdx])
                    activeTrackingBitsNumber++;
                else
                    continue;

                if (activeTrackingBitsNumber >= activationThreshold)
                {
                    if (callback != null)
                        callback(vector);
                    break;
                }
            }
        }
    }
}
