using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;

namespace Concepts.TextConcepts.English
{
    public class CaseInvariantEnglishCharConceptSystemBuilder : ICharConceptSystemBuilder
    {
        IBinaryVectorBuilder binaryVectorBuilder;

        public CaseInvariantEnglishCharConceptSystemBuilder(IBinaryVectorBuilder binaryVectorBuilder)
        {
            this.binaryVectorBuilder = binaryVectorBuilder;
        }


        public IEnumerable<IConceptItem<char, byte>> Build(int contextsCount, int conceptVectorLength, int conceptMaskLength)
        {
            for (char c = 'a'; c <= 'z'; c++)
            {
                for(byte contextIdx = 0; contextIdx < contextsCount; contextIdx++)
                {
                    BitArray vector = this.binaryVectorBuilder.BuildVector(conceptVectorLength, conceptMaskLength);
                    var conceptItem = new CharContextItem(c, vector, contextIdx);
                    yield return conceptItem;
                }
            }
        }
    }
}
