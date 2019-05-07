using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public delegate void ClusterDestroyedEventHandler(object sender, ClusterDestroyedEventArgs e);

    public class ClusterDestroyedEventArgs : EventArgs
    {
    }
}
