using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace CombinatorialSpace.BinaryVectors
{
    /// <summary>
    /// Implements IPoint interface.
    /// </summary>
    public class Point : IPoint
    {
        #region Fields

        private HashSet<int> trackingBitsIndexes;

        private HashSet<int> clusterBitsIndexes;

        private int clusterCreationThreshold;

        private int clusterActivationThreshold;

        private int outputBitIndex;

        #endregion

        #region Events

        public event PointActivatedEventHandler PointActivated;
        public event ClusterCreatedEventHandler ClusterCreated;
        public event ClusterDestroyedEventHandler ClusterDestroyed;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new object of a point. 
        /// </summary>
        /// <param name="random">Is necessary to randomly initialize tracking bits.</param>
        /// <param name="numberOfTrackingBits">How many bits in an input vector the point checks.</param>
        /// <param name="clusterCreationThreshold">How many activated bits in the vector should force creation of a new cluster.</param>
        /// <param name="clusterActivationThreshold">How many activated bits in the vector should trigger activation of a cluster.</param>
        /// <param name="trackingInputBinaryVectorLength">Length of an input vector to initialize randomly tracking bits.</param>
        /// <param name="outputBitIndex">Index of an active bit in the output vector.</param>
        public Point(
            Random random, 
            int numberOfTrackingBits, 
            int clusterCreationThreshold, 
            int clusterActivationThreshold, 
            int trackingInputBinaryVectorLength,
            int outputBitIndex)
        {
            this.trackingBitsIndexes = new HashSet<int>();
            this.clusterBitsIndexes = new HashSet<int>();

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
            this.trackingBitsIndexes = trackingBitsIndexes;
            this.clusterBitsIndexes = new HashSet<int>();
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
        
        public void Train(BitArray inputVector, BitArray outputVector)
        {
            if (inputVector == null || outputVector == null)
                return;

            if (outputVector[this.outputBitIndex])
            {
                this.CheckAndCreateCluster(inputVector);
            }
            else if (this.clusterBitsIndexes.Count > 0)
            {
                this.CheckAndDestroyCluster(inputVector);
            }
        }

        private void CheckAndDestroyCluster(BitArray inputVector)
        {
            //check active cluster bits

            HashSet<int> clusterActiveBitIndexes = new HashSet<int>();
            foreach (int clusterBitIdx in this.clusterBitsIndexes)
            {
                if (inputVector[clusterBitIdx])
                {
                    clusterActiveBitIndexes.Add(clusterBitIdx);
                }
            }

            //if the cluster is ready to trigger, but output training vector does not have
            //active tracking bit this means that cluster does not trigger right
            if (clusterActiveBitIndexes.Count >= this.clusterActivationThreshold)
            {
                this.DestroyCluster();
            }
        }

        private void CheckAndCreateCluster(BitArray inputVector)
        {
            HashSet<int> trackingActiveBitIndexes = new HashSet<int>();

            foreach (int trackingBitIdx in this.trackingBitsIndexes)
            {
                if (inputVector[trackingBitIdx])
                {
                    trackingActiveBitIndexes.Add(trackingBitIdx);
                }
            }

            if (trackingActiveBitIndexes.Count >= this.clusterCreationThreshold)
            {
                if (this.clusterBitsIndexes.Count == 0)
                    this.CreateCluster(trackingActiveBitIndexes);
                else
                    this.AdjustCluster(trackingActiveBitIndexes);
            }
        }

        private void CreateCluster(HashSet<int> trackingActiveBitIndexes)
        {
            this.clusterBitsIndexes = trackingActiveBitIndexes;

            if (this.ClusterCreated != null)
            {
                ClusterCreatedEventArgs e = new ClusterCreatedEventArgs(this.trackingBitsIndexes, this.clusterBitsIndexes);
                this.ClusterCreated(this, e);
            }
        }

        private void AdjustCluster(HashSet<int> trackingActiveBitIndexes)
        {
            //cut the cluster removing inactive cluster indexes
            this.clusterBitsIndexes.RemoveWhere(w => !trackingActiveBitIndexes.Contains(w));

            //if the cluster cannot trigger after the adjustment - destroy it.
            if (this.clusterBitsIndexes.Count < this.clusterActivationThreshold)
            {
                this.DestroyCluster();
            }
        }

        private void DestroyCluster()
        {
            this.clusterBitsIndexes.Clear();

            if (this.ClusterDestroyed != null)
            {
                ClusterDestroyedEventArgs e = new ClusterDestroyedEventArgs();
                this.ClusterDestroyed(this, e);
            }
        }

        public void Check(BitArray inputVector)
        {
            int activeTrackingBitsCount = 0;

            foreach (int clusterBitIdx in this.clusterBitsIndexes)
            {
                if (inputVector[clusterBitIdx])
                    activeTrackingBitsCount++;

                if (activeTrackingBitsCount >= this.clusterActivationThreshold)
                {
                    if (this.PointActivated != null)
                    {
                        PointActivatedEventArgs e = new PointActivatedEventArgs(this.outputBitIndex);
                        this.PointActivated(this, e);
                    }
                    break;
                }
            }
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
