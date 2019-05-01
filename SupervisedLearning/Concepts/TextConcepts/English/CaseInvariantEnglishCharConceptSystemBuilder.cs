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


        public IEnumerable<IConceptItem<byte, char>> Build(int postionsCount, int conceptVectorLength, int conceptMaskLength)
        {
            for (char c = 'a'; c <= 'z'; c++)
            {
                for(byte position = 0; position < postionsCount; position++)
                {
                    BitArray vector = this.binaryVectorBuilder.BuildVector(conceptVectorLength, conceptMaskLength);
                    var conceptItem = new CharConceptItem(c, vector, position);
                    yield return conceptItem;
                }
            }
        }
    }
}
