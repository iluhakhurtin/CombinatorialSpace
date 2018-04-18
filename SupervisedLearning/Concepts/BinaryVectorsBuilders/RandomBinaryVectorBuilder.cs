using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.BinaryVectorsBuilders
{
    public class RandomBinaryVectorBuilder : IBinaryVectorBuilder
    {
        private Random random;

        public RandomBinaryVectorBuilder()
        {
            random = new Random((int)DateTime.Now.Ticks);
        }

        /// <summary>
        /// Randomly initializes binary vector of the given length with 
        /// the given number of True (truesCount). Use the same instance
        /// of the builder to receive different vectors. DO NOT create a 
        /// new instance for a new concept.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="truesCount"></param>
        /// <returns></returns>
        public BitArray BuildVector(int length, int truesCount)
        {
            BitArray result = new BitArray(length);
            int maxTrueIdx = length - 1; //maximum idx in the vector
            for (int i = 0; i < truesCount; i++)
            {
                int randomTrueIdx = this.random.Next(0, maxTrueIdx);

                while (result[randomTrueIdx])
                    randomTrueIdx = this.random.Next(0, maxTrueIdx);

                result.Set(randomTrueIdx, true);
            }
            return result;
        }
    }
}
