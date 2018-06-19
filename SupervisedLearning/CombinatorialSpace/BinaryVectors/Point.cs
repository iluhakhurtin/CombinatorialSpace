using CombinatorialSpace.Strategies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public class Point : IPoint, ITrackingIndexes
    {
        private HashSet<int> trackingBitsIndexes;
        private int clusterCreationThreshold;
        private int clusterActivationThreshold;
        private HashSet<ICluster> clusters;

        public bool Out { get; private set; }

        HashSet<int> ITrackingIndexes.TrackingIndexes => this.trackingBitsIndexes;

        #region Constructor

        /// <summary>
        /// Creates a new object of a point - container for clusters
        /// </summary>
        /// <param name="random">Is necessary to randomly initialize tracking bits.</param>
        /// <param name="numberOfTrackingBits">How many bits in a vector the point checks.</param>
        /// <param name="clusterCreationThreshold">How many activated bits in the vector should force creation of a new cluster.</param>
        /// <param name="clusterActivationThreshold">How many activated bits in the vector should trigger activation of a cluster.</param>
        /// <param name="trackingBinaryVectorLength">Length of an input vector to initialize randomly tracking bits.</param>
        public Point(
            Random random, 
            int numberOfTrackingBits, 
            int clusterCreationThreshold, 
            int clusterActivationThreshold, 
            int trackingBinaryVectorLength)
        {
            this.trackingBitsIndexes = new HashSet<int>();
            this.clusters = new HashSet<ICluster>();
            this.clusterCreationThreshold = clusterCreationThreshold;
            this.clusterActivationThreshold = clusterActivationThreshold;

            this.Initialize(random, numberOfTrackingBits, trackingBinaryVectorLength);
        }

        private void Initialize(Random random, int numberOfTrackingBits, int trackingBinaryVectorLength)
        {
            //maximum index in a bit vector is fewer than length in 1 because it starts from 0
            int maxBinaryVectorIdx = trackingBinaryVectorLength - 1;

            int currentTrackingBitNumber = 0;
            while (currentTrackingBitNumber < numberOfTrackingBits)
            {
                int trackingBitIdx = random.Next(0, maxBinaryVectorIdx);
                if (trackingBitsIndexes.Add(trackingBitIdx))
                    currentTrackingBitNumber++;
            }
        }

        #endregion

        #region Methods

        public void Check(BitArray vector)
        {
            BitArrayCheckStrategy.CheckBitArray(vector, this.trackingBitsIndexes, this.clusterCreationThreshold, this.PointActivatedCallback);
        }

        private void PointActivatedCallback(BitArray vector)
        {
            ICluster cluster = new Cluster(vector, this.clusterActivationThreshold);
            this.clusters.Add(cluster);
        }

        #endregion

        #region Equals and operators

        public override bool Equals(object obj)
        {
            bool result = TrackingIndexesEqualsStrategy.Equals<Point>(obj, this);
            return result;
        }

        public static bool operator ==(Point point1, Point point2)
        {
            if (object.ReferenceEquals(point1, null))
                return object.ReferenceEquals(point2, null);

            return point1.Equals(point2);
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }

        #endregion
    }
}
