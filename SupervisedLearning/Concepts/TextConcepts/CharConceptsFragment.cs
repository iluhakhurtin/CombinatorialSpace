using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Concepts.Concepts;

namespace Concepts.TextConcepts
{
    /// <summary>
    /// Concept fragment for chars concepts. It is mainly used to texts. 
    /// An example can be found in Alexey Redozubov's article here: https://habr.com/post/326334/
    /// Just find the description about the 'swed' text fragment. This
    /// class represents such a structure.
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

        /// <summary>
        /// Every concept has a binary vector representing it. Concept fragment has a binary vector
        /// which is a combination of every concept vector and works as a Bloom Filter according to Alexey Redozubov's idea. 
        /// In the article https://habr.com/post/326334/ there is a fragment:
        /// Создадим бинарный код фрагмента логическим сложением кодов составляющих его понятий. Как уже писалось ранее, 
        /// такой бинарный код фрагмента будет обладать свойствами фильтра Блума.
        /// </summary>
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

        /// <summary>
        /// Use this method to add a new concept to the fragment. 
        /// It keeps all the data in the fragment consistent and the 
        /// Vector can be used a Bloom Filter.
        /// </summary>
        /// <param name="concept"></param>
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
