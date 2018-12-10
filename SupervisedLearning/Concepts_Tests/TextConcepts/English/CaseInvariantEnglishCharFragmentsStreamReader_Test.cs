using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;
using Concepts.TextConcepts.English;
using Concepts_Tests.BaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Concepts_Tests.TextConcepts.English
{
    public class CaseInvariantEnglishCharFragmentsStreamReader_Test: BaseTest
    {
        IConceptsFragmentsStreamReader<char, byte> conceptsFragmentsStreamReader;

        public CaseInvariantEnglishCharFragmentsStreamReader_Test()
        {
            IBinaryVectorBuilder binaryVectorBuilder = new RandomBinaryVectorBuilder();
            IConceptSystemBuilder<char, byte> conceptSystemBuilder = new CaseInvariantEnglishCharConceptSystemBuilder(binaryVectorBuilder);
            conceptsFragmentsStreamReader = new CaseInvariantEnglishCharFragmentsStreamReader(conceptSystemBuilder);
        }

        [Fact]
        public void Can_read_fragments_from_a_stream()
        {
            byte contextsCount = 10;
            int conceptVectorLength = 256;
            int conceptMaskLength = 8;
            int conceptsFragmentLength = 5;

            string fileName = this.GetTestFileName();

            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments = conceptsFragmentsStreamReader.GetConceptsFragments(fs, contextsCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength);
                foreach (var conceptFragment in conceptsFragments)
                {
                    Assert.NotNull(conceptFragment);
                }
            }
        }

        [Fact]
        public void Fragments_are_the_same_for_the_same_text_file_with_the_same_initial_contexts()
        {
            byte contextsCount = 10;
            int conceptVectorLength = 256;
            int conceptMaskLength = 8;
            int conceptsFragmentLength = 5;

            string fileName = this.GetTestFileName();

            using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (FileStream fs1 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, contextsCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength);
                var conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, contextsCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength);
                foreach (var conceptFragment0 in conceptsFragments0)
                {
                    foreach (var conceptFragment1 in conceptsFragments1)
                    {
                        Assert.NotEqual<BitArray>(conceptFragment0.Vector, conceptFragment1.Vector);
                    }
                }
            }
        }

        [Fact]
        public void Fragments_are_different_for_the_same_text_file_with_different_initial_contexts()
        {
            byte contextsCount = 10;
            int conceptVectorLength = 256;
            int conceptMaskLength = 8;
            int conceptsFragmentLength = 5;

            byte initialContext = 2;

            string fileName = this.GetTestFileName();

            using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, contextsCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength);
                var conceptsFragments2 = conceptsFragmentsStreamReader.GetConceptsFragments(fs2, contextsCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, initialContext);
                foreach (var conceptFragment0 in conceptsFragments0)
                {
                    foreach(var conceptFragment2 in conceptsFragments2)
                    {
                        Assert.NotEqual<BitArray>(conceptFragment0.Vector, conceptFragment2.Vector);
                    }
                }
            }
        }

        private string GetTestFileName()
        {
            return @"TextConcepts\English\jack_london_children_of_the_frost.txt";
        }
    }
}
