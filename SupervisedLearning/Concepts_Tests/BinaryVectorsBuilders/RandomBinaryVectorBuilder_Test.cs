using Concepts.BinaryVectorsBuilders;
using Concepts_Tests.BaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace Concepts_Tests.BinaryVectorsBuilders
{
    public class RandomBinaryVectorBuilder_Test: BaseTest
    {
        RandomBinaryVectorBuilder randomBinaryVectorBuilder;

        public RandomBinaryVectorBuilder_Test()
        {
            randomBinaryVectorBuilder = new RandomBinaryVectorBuilder();
        }

        [Fact]
        public void Can_build_random_vector()
        {
            int expectedVectorLength = 256;
            int expectedTruesCount = 8;

            BitArray actual = this.randomBinaryVectorBuilder.BuildVector(expectedVectorLength, expectedTruesCount);

            Assert.Equal(actual.Length, expectedVectorLength);

            int actualTruesCount = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i])
                    actualTruesCount++;
            }

            Assert.Equal(actualTruesCount, expectedTruesCount);
        }
    }
}
