using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public class CombinatorialSpaceBuilder : ICombinatorialSpaceBuilder
    {
        private Random random;

        public CombinatorialSpaceBuilder()
        {
            this.random = new Random((int)DateTime.Now.Ticks);
        }

        public IEnumerable<IPoint> Build(
            int combinatorialSpaceLength, 
            int numberOfTrackingBits, 
            int clusterCreationThreshold, 
            int clusterActivationThreshold, 
            int trackingInputBinaryVectorLength,
            int outputBinaryVectorLength,
            PointActivatedEventHandler pointActivatedEventHandler)
        {
            for (int i = 0; i < combinatorialSpaceLength; i++)
            {
                //to definitely cover all bits of the output vector
                //there is a loop along the indexes of the vector
                int outputBitIndex = i % outputBinaryVectorLength;

                IPoint point = new Point(
                    this.random, 
                    numberOfTrackingBits, 
                    clusterCreationThreshold, 
                    clusterActivationThreshold, 
                    trackingInputBinaryVectorLength,
                    outputBitIndex);

                point.PointActivated += pointActivatedEventHandler;

                yield return point;
            }
        }
    }
}
