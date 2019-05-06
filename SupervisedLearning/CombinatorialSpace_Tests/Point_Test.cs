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
    public class Point_Test : BaseTest
    {
        [Fact]
        public void Can_create_a_cluster()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;

            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            HashSet<int> actualClusterBitsIndexes = null;
            HashSet<int> actualTrackingBitsIndexes = null;

            point.ClusterCreated += (sender, e) =>
            {
                actualTrackingBitsIndexes = e.TrackingBitsIndexes;
                actualClusterBitsIndexes = e.ClusterBitsIndexes;
                autoResetEvent.Set();
            };

            bool[] inputTrainBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2    
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray inputTrainVector = new BitArray(inputTrainBitArray);

            int[] clusterBitsIndexesArray = new int[] { 0, 5, 6, 8, 11, 12 };
            HashSet<int> clusterBitsIndexes = new HashSet<int>(clusterBitsIndexesArray);

            bool[] outputTrainBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                true,   // 3    +
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainVector = new BitArray(outputTrainBitArray);

            point.Train(inputTrainVector, outputTrainVector);
            
            Assert.True(autoResetEvent.WaitOne());
            Assert.Equal(trackingBitsIndexes, actualTrackingBitsIndexes);
            Assert.Equal(clusterBitsIndexes, actualClusterBitsIndexes);
        }

        [Fact]
        public void Can_create_and_activate_a_cluster()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;
            
            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            int actualOutputBitIndex = -1;
            point.PointActivated += (sender, e) =>
            {
                actualOutputBitIndex = e.OutputBitIndex;
                autoResetEvent.Set();
            };

            bool[] inputTrainBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2    
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray inputTrainVector = new BitArray(inputTrainBitArray);

            bool[] outputTrainBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                true,   // 3    +
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainVector = new BitArray(outputTrainBitArray);

            point.Train(inputTrainVector, outputTrainVector);

            bool[] checkBitArray = new bool[]
            {
                false,  // 0    -
                false,  // 1
                false,  // 2
                false,  // 3
                false,  // 4
                true,   // 5    +
                false,  // 6    -
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                false,  // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray checkVector = new BitArray(checkBitArray);

            point.Check(checkVector);

            Assert.True(autoResetEvent.WaitOne());
            Assert.Equal(outputBitIndex, actualOutputBitIndex);
        }

        [Fact]
        public void Can_NOT_create_a_cluster_if_input_vector_does_not_have_enough_active_bits_for_activation_and_output_bit_is_active()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;

            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            bool isClusterCreated = false;
            point.ClusterCreated += (sender, e) =>
            {
                isClusterCreated = true;
            };

            bool[] inputTrainBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2    
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                false,  // 8    -
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray inputTrainVector = new BitArray(inputTrainBitArray);

            int[] clusterBitsIndexesArray = new int[] { 0, 5, 6, 8, 11, 12 };
            HashSet<int> clusterBitsIndexes = new HashSet<int>(clusterBitsIndexesArray);

            bool[] outputTrainBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                true,   // 3    +
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainVector = new BitArray(outputTrainBitArray);

            point.Train(inputTrainVector, outputTrainVector);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = new TimerCallback(delegate (object target)
            {
                autoResetEvent.Set();
            });

            Timer timer = new Timer(timerCallback, null, 0, 500);

            Assert.True(autoResetEvent.WaitOne());
            Assert.False(isClusterCreated); //This is the test, the callback is never called. Timer just waits for half a second.
        }

        [Fact]
        public void Can_NOT_create_and_activate_a_cluster_if_input_vector_does_not_have_enough_active_bits_for_activation_and_output_bit_is_active()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;

            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            bool pointActivated = false;
            point.PointActivated += (sender, e) =>
            {
                pointActivated = true;
            };

            bool[] inputTrainBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                false   // 12   -
            };
            BitArray inputTrainVector = new BitArray(inputTrainBitArray);

            bool[] outputTrainBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                true,   // 3    +
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainVector = new BitArray(outputTrainBitArray);

            point.Train(inputTrainVector, outputTrainVector);

            bool[] checkBitArray = new bool[]
            {
                false,  // 0    -
                false,  // 1
                false,  // 2
                false,  // 3
                false,  // 4
                true,   // 5    +
                false,  // 6    -
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                false,  // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray checkVector = new BitArray(checkBitArray);

            point.Check(checkVector);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = new TimerCallback(delegate (object target)
            {
                autoResetEvent.Set();
            });

            Timer timer = new Timer(timerCallback, null, 0, 500);

            Assert.True(autoResetEvent.WaitOne());
            Assert.False(pointActivated); //This is the test, the callback is never called. Timer just waits for half a second.
        }

        [Fact]
        public void Can_NOT_create_a_cluster_if_output_bit_is_not_active()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;

            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            bool isClusterCreated = false;
            point.ClusterCreated += (sender, e) =>
            {
                isClusterCreated = true;
            };

            bool[] inputTrainBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2    
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray inputTrainVector = new BitArray(inputTrainBitArray);

            int[] clusterBitsIndexesArray = new int[] { 0, 5, 6, 8, 11, 12 };
            HashSet<int> clusterBitsIndexes = new HashSet<int>(clusterBitsIndexesArray);

            bool[] outputTrainBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                false,  // 3    -
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainVector = new BitArray(outputTrainBitArray);

            point.Train(inputTrainVector, outputTrainVector);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = new TimerCallback(delegate (object target)
            {
                autoResetEvent.Set();
            });

            Timer timer = new Timer(timerCallback, null, 0, 500);

            Assert.True(autoResetEvent.WaitOne());
            Assert.False(isClusterCreated); //This is the test, the callback is never called. Timer just waits for half a second.
        }

        [Fact]
        public void Can_NOT_create_and_activate_a_cluster_if_output_bit_is_not_active()
        {
            int[] trackingBitsIndexesArray = new int[] { 0, 5, 6, 8, 9, 11, 12 };
            HashSet<int> trackingBitsIndexes = new HashSet<int>(trackingBitsIndexesArray);
            int clusterCreationThreshold = 6;
            int clusterActivationThreshold = 4;
            int outputBitIndex = 3;

            IPoint point = new Point(trackingBitsIndexes, outputBitIndex, clusterCreationThreshold, clusterActivationThreshold);

            bool pointActivated = false;
            point.PointActivated += (sender, e) =>
            {
                pointActivated = true;
            };

            bool[] inputTrainingBitArray = new bool[]
            {
                true,   // 0    +
                false,  // 1
                true,   // 2
                false,  // 3
                false,  // 4
                true,   // 5    +
                true,   // 6    +
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                true,   // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray inputTrainingVector = new BitArray(inputTrainingBitArray);

            bool[] outputTrainingBitArray = new bool[]
            {
                true,   // 0
                false,  // 1
                false,  // 2
                false,  // 3    +
                false,  // 4
                false,  // 5
                false,  // 6
                true,   // 7
                true,   // 8
                false,  // 9
                false,  // 10
                true,   // 11
                false   // 12
            };
            BitArray outputTrainingVector = new BitArray(outputTrainingBitArray);

            point.Train(inputTrainingVector, outputTrainingVector);

            bool[] checkBitArray = new bool[]
            {
                false,  // 0    -
                false,  // 1
                false,  // 2
                false,  // 3
                false,  // 4
                true,   // 5    +
                false,  // 6    -
                false,  // 7
                true,   // 8    +
                false,  // 9    -
                false,  // 10
                true,   // 11   +
                true    // 12   +
            };
            BitArray checkVector = new BitArray(checkBitArray);

            point.Check(checkVector);

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = new TimerCallback(delegate (object target)
            {
                autoResetEvent.Set();
            });

            Timer timer = new Timer(timerCallback, null, 0, 500);

            Assert.True(autoResetEvent.WaitOne());
            Assert.False(pointActivated); //This is the test, the callback is never called. Timer just waits for half a second.
        }
    }
}
