using Concepts.Concepts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.TextConcepts
{
    class CharContextItem : IConceptItem<char, byte>
    {
        #region Properties 

        public byte Context { get; set; }

        public char Value { get; set; }

        public BitArray Vector { get; set; }

        #endregion

        #region Constructors

        public CharContextItem()
        {
        }

        public CharContextItem(char value, BitArray vector)
            : this(value, vector, 0)
        {
            
        }

        public CharContextItem(char value, BitArray vector, byte context)
        {
            this.Value = value;
            this.Vector = vector;
            this.Context = context;
        }

        #endregion
    }
}
