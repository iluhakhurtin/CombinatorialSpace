using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    /// <summary>
    /// This interface embeds logic of a point which can work in 2 modes training (learning) 
    /// and checking (testing) the data.
    /// </summary>
    public interface IPoint
    {
        /// <summary>
        /// This method is used to train the point.
        /// </summary>
        /// <param name="inputVector"></param>
        /// <param name="outputVector"></param>
        void Train(BitArray inputVector, BitArray outputVector);
        void Check(BitArray inputVector);

        /// <summary>
        /// This event triggers if on method Check the point activates.
        /// </summary>
        event PointActivatedEventHandler PointActivated;
    }
}
