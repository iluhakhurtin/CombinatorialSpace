using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public delegate void ClusterActivatedEventHandler(object sender, EventArgs e);

    public class ClusterActivatedEventArgs : EventArgs
    {

    }

    public interface ICluster : IBitArrayChecker
    {
        event ClusterActivatedEventHandler ClusterActivated;
    }
}
