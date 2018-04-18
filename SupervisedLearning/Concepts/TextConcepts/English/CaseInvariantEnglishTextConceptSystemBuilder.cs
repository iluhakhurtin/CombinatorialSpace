using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;

namespace Concepts.TextConcepts.English
{
    public class CaseInvariantEnglishTextConceptSystemBuilder : ITextConceptSystemBuilder
    {
        IBinaryVectorBuilder binaryVectorBuilder;

        public CaseInvariantEnglishTextConceptSystemBuilder(IBinaryVectorBuilder binaryVectorBuilder)
        {
            this.binaryVectorBuilder = binaryVectorBuilder;
        }


        public IEnumerable<IConceptItem<string>> Build(int contextsNumber, int conceptVectorLength, int conceptMaskLength)
        {
            for (char c = 'a'; c <= 'z'; c++)
            {
                for(byte contextIdx = 0; contextIdx < contextsNumber; contextIdx++)
                {
                    BitArray vector = this.binaryVectorBuilder.BuildVector(conceptVectorLength, conceptMaskLength);
                    var conceptItem = new TextContextItem(c.ToString(), vector);
                    yield return conceptItem;
                }
            }
        }
    }
}
