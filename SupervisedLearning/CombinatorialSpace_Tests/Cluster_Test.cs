using CombinatorialSpace.BinaryVectors;
using CombinatorialSpace_Tests.BaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace CombinatorialSpace_Tests
{
    public class Cluster_Test : BaseTest
    {
        [Fact]
        public void Can_activate_cluster()
        {
            int[] trackingBitsIndexesArray = new int[] { 2, 5, 7, 8, 9, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int activationThreshold = 4;
            
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            ICluster cluster = new Cluster(trackingBitsIndexes, activationThreshold);
            cluster.ClusterActivated += (sender, e) =>
            {
                autoResetEvent.Set();
            };

            bool[] bitArray = new bool[]
            {
                false,  // 0
                false,  // 1
                true,   // 2    +
                false,  // 3
                false,  // 4
                true,   // 5    +
                false,  // 6
                false,  // 7
                true,   // 8    +
                false,  // 9
                true,   // 10
                true,   // 11
                true    // 12   +
            };
            BitArray vector = new BitArray(bitArray);
            cluster.Check(vector);

            Assert.True(autoResetEvent.WaitOne());
        }


        [Fact]
        public void Can_NOT_activate_cluster()
        {
            int[] trackingBitsIndexesArray = new int[] { 2, 5, 7, 8, 9, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int activationThreshold = 4;

            ICluster cluster = new Cluster(trackingBitsIndexes, activationThreshold);
            bool clusterActivated = false;
            cluster.ClusterActivated += (sender, e) =>
            {
                clusterActivated = true;
            };

            bool[] bitArray = new bool[]
            {
                false,  // 0
                false,  // 1
                true,   // 2    +
                false,  // 3
                false,  // 4
                true,   // 5    +
                false,  // 6
                false,  // 7
                true,   // 8    +
                false,  // 9
                true,   // 10
                true,   // 11
                false   // 12   
            };
            BitArray vector = new BitArray(bitArray);
            cluster.Check(vector);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = new TimerCallback(delegate (object target)
            {
                autoResetEvent.Set();
            });

            Timer timer = new Timer(timerCallback, null, 0, 500);

            Assert.True(autoResetEvent.WaitOne());
            Assert.False(clusterActivated); //This is the test
        }
    }
}
