using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;
using Concepts.TextConcepts.English;
using Concepts_Tests.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Concepts_Tests.TextConcepts.English
{
    public class CaseInvariantEnglishCharConceptSystemBuilder_Test : BaseTest
    {
        CaseInvariantEnglishCharConceptSystemBuilder caseInvariantEnglishTextConceptSystemBuilder;

        public CaseInvariantEnglishCharConceptSystemBuilder_Test()
        {
            IBinaryVectorBuilder binaryVectorBuilder = new RandomBinaryVectorBuilder();
            caseInvariantEnglishTextConceptSystemBuilder = new CaseInvariantEnglishCharConceptSystemBuilder(binaryVectorBuilder);
        }


        [Fact]
        public void Can_build_concept_system()
        {
            int contextsNumber = 10;
            int conceptVectorLength = 256;
            int conceptMaskLength = 8;
            var concepts = this.caseInvariantEnglishTextConceptSystemBuilder.Build(contextsNumber, conceptVectorLength, conceptMaskLength);

            int count = 0;
            foreach (var concept in concepts)
            {
                Assert.Equal(conceptVectorLength, concept.Vector.Length);

                count++;
                foreach (var anotherConcept in concepts)
                {
                    bool equals = true;
                    for (int i = 0; i < concept.Vector.Length; i++)
                    {
                        equals &= concept.Vector[i] == anotherConcept.Vector[i];

                        if (!equals)
                            break;
                    }
                    Assert.False(equals);
                }
            }

            Assert.Equal(26 * contextsNumber, count);
        }
    }
}
