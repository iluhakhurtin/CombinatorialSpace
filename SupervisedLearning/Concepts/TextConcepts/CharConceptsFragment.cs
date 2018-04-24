using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Concepts.Concepts;

namespace Concepts.TextConcepts
{
    /// <summary>
    /// Concept fragment for chars concepts. It is mainly used to texts.
    /// </summary>
    public class CharConceptsFragment : IConceptsFragment<char, byte>
    {
        private List<CharContextItem> concepts;

        public IEnumerable<IConceptItem<char, byte>> Concepts
        {
            get
            {
                return this.concepts;
            }
        }

        public BitArray Vector { get; private set; }

        public int ConceptsCount
        {
            get
            {
                return this.concepts.Count;
            }
        }

        public CharConceptsFragment()
        {
            this.concepts = new List<CharContextItem>();
        }

        public void AddConcept(IConceptItem<char, byte> concept)
        {
            if (concept == null)
                throw new ArgumentNullException("concept");

            this.concepts.Add((CharContextItem)concept);

            if (this.Vector == null)
            {
                this.Vector = new BitArray(concept.Vector);
                return;
            }

            this.Vector.Or(concept.Vector);
        }
    }
}
