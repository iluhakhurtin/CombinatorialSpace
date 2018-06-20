using CombinatorialSpace.Strategies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CombinatorialSpace.BinaryVectors
{
    public class Cluster : ICluster
    {
        public event ClusterActivatedEventHandler ClusterActivated;

        private HashSet<int> trackingBitsIndexes;
        private int activationThreshold = 0;

        #region Constructors

        public Cluster(HashSet<int> trackingBitsIndexes, int activationThreshold)
        {
            this.trackingBitsIndexes = trackingBitsIndexes;
            this.activationThreshold = activationThreshold;
        }

        /// <summary>
        /// Creates an instance of cluster with snapshot of the given activated vector.
        /// </summary>
        /// <param name="activatedVector"></param>
        /// <param name="activationThreshold"></param>
        public Cluster(BitArray activatedVector, int activationThreshold)
        {
            this.trackingBitsIndexes = new HashSet<int>();
            for(var i = 0; i < activatedVector.Length; i++)
            {
                if (activatedVector[i])
                    this.trackingBitsIndexes.Add(i);
            }

            this.activationThreshold = activationThreshold;
        }

        #endregion

        #region Methods

        public void Check(BitArray vector)
        {
            BitArrayCheckStrategy.CheckBitArray(
                vector, 
                this.trackingBitsIndexes, 
                this.activationThreshold, 
                this.ActivateClusterCallback);
        }

        public void ActivateClusterCallback(BitArray vector)
        {
            if (this.ClusterActivated != null)
            {
                //this.ClusterActivated.BeginInvoke is not supported by 
                //.NET Core 2. That is why call handlers using Task.Run
                Delegate[] eventHandlers = this.ClusterActivated.GetInvocationList();
                var sender = this;
                var e = new ClusterActivatedEventArgs();
                foreach (var eventHandler in eventHandlers)
                {
                    var clusterActivatedEventHandler = (ClusterActivatedEventHandler)eventHandler;
                    Task.Run(() =>
                    {
                        clusterActivatedEventHandler(sender, e);
                    });
                }
            }
        }

        #endregion

        #region Equals and operators

        public override bool Equals(object obj)
        {
            Cluster target = obj as Cluster;
            if (target == null)
                return false;

            foreach (int trackingBitIdx in this.trackingBitsIndexes)
            {
                if (!target.trackingBitsIndexes.Contains(trackingBitIdx))
                    return false;
            }

            return true;
        }

        public static bool operator ==(Cluster cluster1, Cluster cluster2)
        {
            if (object.ReferenceEquals(cluster1, null))
                return object.ReferenceEquals(cluster2, null);

            return cluster1.Equals(cluster2);
        }

        public static bool operator !=(Cluster cluster1, Cluster cluster2)
        {
            return !(cluster1 == cluster2);
        }

        #endregion
    }
}
