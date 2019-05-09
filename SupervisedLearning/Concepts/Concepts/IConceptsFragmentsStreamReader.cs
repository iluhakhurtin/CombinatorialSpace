using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Concepts.Concepts
{
    public interface IConceptsFragmentsStreamReader<TKey, TValue> where TKey : struct
    {
        IEnumerable<IConceptsFragment<TKey, TValue>> GetConceptsFragments(Stream stream, int conceptsFragmentLength, TKey initialKey = default(TKey));
    }
}
