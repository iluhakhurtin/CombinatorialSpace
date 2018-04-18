using Concepts.Concepts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.TextConcepts
{
    class TextContextItem : IConceptItem<string>
    {
        #region Properties 

        public string Value { get; set; }

        public BitArray Vector { get; set; }

        #endregion

        #region Constructors

        public TextContextItem()
        {
        }

        public TextContextItem(string value, BitArray vector)
        {
            this.Value = value;
            this.Vector = vector;
        }

        #endregion
    }
}
