using System;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    public interface IConceptSystemBuilder<out TKey, out TValue>
    {
        /// <summary>
        /// Builds cocept system with the given contexts (contextNumber), 
        /// every concept has binary vector of length equals to conceptVectorLength. 
        /// The vector has conceptMaskLength ones (Trues). This mask is the certain concept
        /// binary code.
        /// </summary>
        /// <param name="keysCount">Possible positions count for a consept.</param>
        /// <param name="conceptVectorLength">Vector length for a concept.</param>
        /// <param name="conceptMaskLength">Number of active bits in concept vector.</param>
        /// <returns></returns>
        IEnumerable<IConceptItem<TKey, TValue>> Build(int keysCount, int conceptVectorLength, int conceptMaskLength);
    }
}
