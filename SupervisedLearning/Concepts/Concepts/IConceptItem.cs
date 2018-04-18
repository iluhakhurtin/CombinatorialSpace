using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    public interface IConceptItem<out T>
    {
        T Value { get; }
        BitArray Vector { get; }
    }
}
