using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.BinaryVectors
{
    public class Point : IPoint
    {
        private HashSet<int> trackingBitsIndexes;

        public Point(Random random, int numberOfTrackingBits, int trackingBinaryVectorLength)
        {
            this.trackingBitsIndexes = new HashSet<int>();
            this.Initialize(random, numberOfTrackingBits, trackingBinaryVectorLength);
        }

        private void Initialize(Random random, int numberOfTrackingBits, int trackingBinaryVectorLength)
        {
            //maximum index in a bit vector is fewer than length in 1 because it starts from 0
            int maxBinaryVectorIdx = trackingBinaryVectorLength - 1;

            int currentTrackingBitNumber = 0;
            while (currentTrackingBitNumber < numberOfTrackingBits)
            {
                int trackingBitIdx = random.Next(0, maxBinaryVectorIdx);
                if (trackingBitsIndexes.Add(trackingBitIdx))
                    currentTrackingBitNumber++;
            }
        }

        public override bool Equals(object obj)
        {
            Point point = obj as Point;
            if (point == null)
                return false;
            
            foreach (int trackingBitIdx in this.trackingBitsIndexes)
            {
                if (!point.trackingBitsIndexes.Contains(trackingBitIdx))
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
    }
}
