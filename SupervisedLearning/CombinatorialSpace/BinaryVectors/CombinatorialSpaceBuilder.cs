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
            int trackingBinaryVectorLength)
        {
            for (int i = 0; i < combinatorialSpaceLength; i++)
            {
                IPoint result = new Point(
                    this.random, 
                    numberOfTrackingBits, 
                    clusterCreationThreshold, 
                    clusterActivationThreshold, 
                    trackingBinaryVectorLength);
                yield return result;
            }
        }
    }
}
