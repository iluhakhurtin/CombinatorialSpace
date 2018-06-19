using System;
using System.Collections.Generic;
using System.Text;

namespace CombinatorialSpace.Strategies
{
    interface ITrackingIndexes
    {
        HashSet<int> TrackingIndexes { get; }
    }

    static class TrackingIndexesEqualsStrategy
    {
        public static bool Equals<T>(object obj, T source) where T : class, ITrackingIndexes
        {
            T target = obj as T;
            if (target == null)
                return false;

            foreach (int trackingBitIdx in source.TrackingIndexes)
            {
                if (!target.TrackingIndexes.Contains(trackingBitIdx))
                    return false;
            }

            return true;
        }
    }
}
