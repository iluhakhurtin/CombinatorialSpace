using System;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    public interface IConceptSystemBuilder<out TValue, out TContext>
    {
        /// <summary>
        /// Builds cocept system with the given contexts (contextNumber), 
        /// every concept has binary vector of length equals to conceptVectorLength. 
        /// The vector has conceptMaskLength ones (Trues). This mask is the certain concept
        /// binary code.
        /// </summary>
        /// <param name="contextsCount"></param>
        /// <param name="vectorLength"></param>
        /// <param name="conceptMaskLength"></param>
        /// <returns></returns>
        IEnumerable<IConceptItem<TValue, TContext>> Build(int contextsCount, int conceptVectorLength, int conceptMaskLength);
    }
}
