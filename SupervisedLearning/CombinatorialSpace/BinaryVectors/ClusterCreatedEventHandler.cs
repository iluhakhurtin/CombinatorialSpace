using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public delegate void ClusterCreatedEventHandler(object sender, ClusterCreatedEventArgs e);

    public class ClusterCreatedEventArgs : EventArgs
    {
        public HashSet<int> ClusterBitsIndexes { get; private set; }
        public HashSet<int> TrackingBitsIndexes { get; private set; }

        public ClusterCreatedEventArgs(HashSet<int> trackingBitsIndexes, HashSet<int> clusterBitsIndexes)
        {
            this.TrackingBitsIndexes = trackingBitsIndexes;
            this.ClusterBitsIndexes = clusterBitsIndexes;
        }
    }
}
