using System;
using System.Collections;
using System.Collections.Generic;
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
                HashSet<int> activeBitsIndexes = new HashSet<int>();
                    
                foreach (int trackingBitIdx in this.trackingBitsIndexes)
                {
                    if (inputVector[trackingBitIdx])
                    {
                        activeBitsIndexes.Add(trackingBitIdx);
                    }
                }
                
                if (activeBitsIndexes.Count >= this.clusterCreationThreshold)
                {
                    //if the cluster has not been created before or
                    //has been destroyed because after a certain amount of repetitions 
                    //the dependency has not been found
                    if (this.clusterBitsIndexes.Count < this.clusterCreationThreshold)
                    {
                        this.clusterBitsIndexes = activeBitsIndexes;
                    }
                    else
                    {
                        //cut the cluster removing the tracking bits that are not active
                        this.clusterBitsIndexes.RemoveWhere(i => !activeBitsIndexes.Contains(i));
                    }

                    if (this.ClusterCreated != null)
                    {
                        //this.ClusterActivated.BeginInvoke is not supported by 
                        //.NET Core 2. That is why call handlers using Task.Run
                        object sender = this;
                        ClusterCreatedEventArgs e = new ClusterCreatedEventArgs(this.trackingBitsIndexes, this.clusterBitsIndexes);

                        Delegate[] eventHandlers = this.ClusterCreated.GetInvocationList();

                        foreach (var eventHandler in eventHandlers)
                        {
                            var clusterCreatedEventHandler = (ClusterCreatedEventHandler)eventHandler;
                            Task.Run(() =>
                            {
                                clusterCreatedEventHandler(sender, e);
                            });
                        }
                    }
                }
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
                        //this.ClusterActivated.BeginInvoke is not supported by 
                        //.NET Core 2. That is why call handlers using Task.Run
                        object sender = this;
                        PointActivatedEventArgs e = new PointActivatedEventArgs(this.outputBitIndex);

                        Delegate[] eventHandlers = this.PointActivated.GetInvocationList();

                        foreach (var eventHandler in eventHandlers)
                        {
                            var pointActivatedEventHandler = (PointActivatedEventHandler)eventHandler;
                            Task.Run(() =>
                            {
                                pointActivatedEventHandler(sender, e);
                            });
                        }
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
