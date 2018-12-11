using CombinatorialSpace.Strategies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public class Point : IPoint
    {
        private HashSet<int> trackingBitsIndexes;
        private int clusterCreationThreshold;
        private int clusterActivationThreshold;
        private HashSet<ICluster> clusters;
        private int outputBitIndex;

        public IEnumerable<ICluster> Clusters
        {
            get { return this.clusters; }
        }

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
            int trackingInputBinaryVectorLength,
            int outputBitIndex)
        {
            this.trackingBitsIndexes = new HashSet<int>();
            this.clusters = new HashSet<ICluster>();
            this.clusterCreationThreshold = clusterCreationThreshold;
            this.clusterActivationThreshold = clusterActivationThreshold;
            this.outputBitIndex = outputBitIndex;

            this.Initialize(random, numberOfTrackingBits, trackingInputBinaryVectorLength);
        }

        public Point(
            HashSet<int> trackingBitsIndexes, 
            int outputBitIndex, 
            int clusterCreationThreshold,
            int clusterActivationThreshold)
        {
            this.clusters = new HashSet<ICluster>();
            this.trackingBitsIndexes = trackingBitsIndexes;
            this.outputBitIndex = outputBitIndex;
            this.clusterCreationThreshold = clusterCreationThreshold;
            this.clusterActivationThreshold = clusterActivationThreshold;
        }

        private void Initialize(
            Random random, 
            int numberOfTrackingBits, 
            int trackingBinaryVectorLength)
        {
            //maximum index in a bit vector is fewer than length in 1 because it starts from 0
            int maxInputBinaryVectorIdx = trackingBinaryVectorLength - 1;

            int currentTrackingBitNumber = 0;
            while (currentTrackingBitNumber < numberOfTrackingBits)
            {
                int trackingBitIdx = random.Next(0, maxInputBinaryVectorIdx);
                if (this.trackingBitsIndexes.Add(trackingBitIdx))
                    currentTrackingBitNumber++;
            }
        }

        #endregion

        #region Methods

        public void Check(BitArray inputVector, BitArray outputVector)
        {
            if (inputVector == null || outputVector == null)
                return;

            if (outputVector[this.outputBitIndex])
            {
                BitArrayCheckStrategy.CheckBitArray(inputVector, this.trackingBitsIndexes, this.clusterCreationThreshold, this.PointActivatedCallback);
            }
        }

        /// <summary>
        /// This callback can be called only if the output bit in the vector is active, 
        /// see method Check(BitArray inputVector, BitArray outputVector).
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        private void PointActivatedCallback(BitArray inputVector)
        {
            ICluster cluster = new Cluster(inputVector, this.clusterActivationThreshold);
            this.clusters.Add(cluster);
        }

        #endregion

        #region Equals and operators

        public override bool Equals(object obj)
        {
            Point target = obj as Point;
            if (target == null)
                return false;

            if (target.outputBitIndex != this.outputBitIndex)
                return false;

            foreach (int trackingBitIdx in this.trackingBitsIndexes)
            {
                if (!target.trackingBitsIndexes.Contains(trackingBitIdx))
                    return false;
            }

            return true;
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
