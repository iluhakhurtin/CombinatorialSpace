using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    /// <summary>
    /// This class represents a set of concepts and their resulting vector. 
    /// In Alexey Redozubov's book it is fragment of text in a sliding window. 
    /// </summary>
    /// <typeparam name="TKey">Type of a key. Basically it is numeric identifier.</typeparam>
    /// <typeparam name="TValue">Type of underlying concept item value.</typeparam>
    public interface IConceptsFragment<TKey, TValue>
    {
        int ConceptsCount { get; }

        IEnumerable<IConceptItem<TKey, TValue>> Concepts { get;  }
        
        BitArray Vector { get; }

        void AddConcept(IConceptItem<TKey, TValue> concept);
    }
}
