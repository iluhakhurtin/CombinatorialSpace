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
    /// <typeparam name="TValue">Type of underlying concept item value.</typeparam>
    /// <typeparam name="TContext">Type of context. Basically it is numeric identifier.</typeparam>
    public interface IConceptsFragment<TValue, TContext>
    {
        int ConceptsCount { get; }

        IEnumerable<IConceptItem<TValue, TContext>> Concepts { get;  }
        
        BitArray Vector { get; }

        void AddConcept(IConceptItem<TValue, TContext> concept);
    }
}
