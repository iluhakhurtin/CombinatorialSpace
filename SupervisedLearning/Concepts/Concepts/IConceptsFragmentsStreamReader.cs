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
        IEnumerable<IConceptsFragment<TKey, TValue>> GetConceptsFragments(Stream stream, TKey keysCount, int conceptVectorLength, int conceptMaskLength, int conceptsFragmentLength, TKey initialPosition = default(TKey));
    }
}
