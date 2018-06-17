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

        #endregion

        #region Methods

        public void Check(BitArray vector)
        {
            int activeTrackingBitsNumber = 0;

            foreach (int trackingBitIdx in this.trackingBitsIndexes)
            {
                if (vector[trackingBitIdx])
                    activeTrackingBitsNumber++;
                else
                    continue;

                if(activeTrackingBitsNumber >= this.activationThreshold)
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

                    break;
                }
            }
        }

        #endregion
    }
}
