using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Concepts.Concepts
{
    public interface IConceptsFragmentsStreamReader<TValue, TConcept> where TConcept : struct
    {
        IEnumerable<IConceptsFragment<TValue, TConcept>> GetConceptsFragments(Stream stream, TConcept contextsCount, int conceptVectorLength, int conceptMaskLength, int conceptsFragmentLength, TConcept initialContext = default(TConcept));
    }
}
