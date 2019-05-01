using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    public interface IConceptItem<out TKey, out TValue>
    {
        TKey Key { get; }
        TValue Value { get; }
        BitArray Vector { get; }
    }
}
