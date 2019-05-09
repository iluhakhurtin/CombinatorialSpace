using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;
using Concepts.TextConcepts.English;
using Concepts_Tests.BaseClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Concepts_Tests.TextConcepts.English
{
    public class CaseInvariantEnglishCharFragmentsStreamReader_Test: BaseTest
    {
        private const int conceptsFragmentLength = 5; //number of characters read at once
        private const string testFileName = @"TextConcepts\English\jack_london_children_of_the_frost.txt";

        private readonly CaseInvariantEnglishCharFragmentsStreamReader conceptsFragmentsStreamReader;

        public CaseInvariantEnglishCharFragmentsStreamReader_Test()
        {
            IBinaryVectorBuilder binaryVectorBuilder = new RandomBinaryVectorBuilder();
            IConceptSystemBuilder<byte, char> conceptSystemBuilder = new CaseInvariantEnglishCharConceptSystemBuilder(binaryVectorBuilder);

            byte contextsCount = 10;
            int conceptVectorLength = 256;
            int conceptMaskLength = 8;

            IEnumerable<IConceptItem<byte, char>> conceptSystem = conceptSystemBuilder.Build(contextsCount, conceptVectorLength, conceptMaskLength);

            this.conceptsFragmentsStreamReader = new CaseInvariantEnglishCharFragmentsStreamReader(conceptSystem, contextsCount);
        }

        [Fact]
        public void Can_read_fragments_from_a_stream()
        {
            string fileName = this.GetTestFileName();

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments = conceptsFragmentsStreamReader.GetConceptsFragments(fs, conceptsFragmentLength);
                foreach (var conceptFragment in conceptsFragments)
                {
                    Assert.NotNull(conceptFragment);
                }
            }
        }

        [Fact]
        public void Fragments_are_the_same_for_the_same_text_file_with_the_same_initial_contexts()
        {
            string fileName = this.GetTestFileName();

            using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (FileStream fs1 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, conceptsFragmentLength);
                var conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, conceptsFragmentLength);

                var conceptsFragments0Enumerator = conceptsFragments0.GetEnumerator();
                var conceptsFragments1Enumerator = conceptsFragments1.GetEnumerator();

                while (conceptsFragments0Enumerator.MoveNext() 
                       && conceptsFragments1Enumerator.MoveNext())
                {
                    var conceptFragment0 = conceptsFragments0Enumerator.Current;
                    var conceptFragment1 = conceptsFragments1Enumerator.Current;

                    if (conceptFragment0.Vector == null || conceptFragment1.Vector == null)
                    {
                        Assert.Equal(0, conceptFragment0.ConceptsCount);
                        Assert.Equal(0, conceptFragment1.ConceptsCount);
                        Assert.Null(conceptFragment0.Vector);
                        Assert.Null(conceptFragment1.Vector);
                    }
                    else
                        Assert.Equal<BitArray>(conceptFragment0.Vector, conceptFragment1.Vector);
                }
            }
        }

        [Fact]
        public void Fragments_are_different_for_the_same_text_file_with_different_initial_contexts()
        {
            // this test represents example given by Alexey Redozubov in his article here: https://habr.com/post/326334/
            // it refers to the place where he describes two different concepts fragment built on the same text fragment 
            // BUT with different position of the cyclic identifier:

            // Описание, составленное таким образом, хранит не только набор букв фрагмента текста, но и их последовательность. 

            // This description pertains IConceptsFragment in this application.

            // Однако и набор понятий, и суммарный код этой последовательности зависит от того, в какой позиции находился циклический
            // идентификатор на момент, когда последовательность появилась в тексте. На рисунке ниже приведен пример того, как описание 
            // одного и того же     фрагмента текста зависит от начального положения циклического идентификатора.
            // https://hsto.org/getpro/habr/post_images/ee1/2e8/c02/ee12e8c026e408847a4744469c2f96b1.png
            // Изменение кодировки текста при разном начальном смещении
            // Первому случаю будет соответствовать описание
            // { s1,w3,e4,d6}
            // Второй случай будет записан, как
            // { s9,w1,e2,d4}
            // В результате текст один и тот же, но совсем другой набор понятий и, соответственно, 
            // совсем другой описывающий его бинарный код.
            
            byte initialKey = 2;

            string fileName = this.GetTestFileName();

            using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (FileStream fs2 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, conceptsFragmentLength);
                var conceptsFragments2 = conceptsFragmentsStreamReader.GetConceptsFragments(fs2, conceptsFragmentLength, initialKey);

                var conceptsFragments0Enumerator = conceptsFragments0.GetEnumerator();
                var conceptsFragments2Enumerator = conceptsFragments2.GetEnumerator();

                while (conceptsFragments0Enumerator.MoveNext()
                       && conceptsFragments2Enumerator.MoveNext())
                {
                    var conceptFragment0 = conceptsFragments0Enumerator.Current;
                    var conceptFragment2 = conceptsFragments2Enumerator.Current;

                    if (conceptFragment0.Vector == null || conceptFragment2.Vector == null)
                    {
                        Assert.Equal(0, conceptFragment0.ConceptsCount);
                        Assert.Equal(0, conceptFragment2.ConceptsCount);
                        Assert.Null(conceptFragment0.Vector);
                        Assert.Null(conceptFragment2.Vector);
                    }
                    else
                        Assert.NotEqual<BitArray>(conceptFragment0.Vector, conceptFragment2.Vector);
                }
            }
        }

        private string GetTestFileName()
        {
            return testFileName;
        }
    }
}
