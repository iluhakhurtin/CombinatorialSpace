using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Concepts.Concepts
{
    public interface IConceptItem<out TValue, out TContext>
    {
        TValue Value { get; }
        TContext Context { get; }
        BitArray Vector { get; }
    }
}
