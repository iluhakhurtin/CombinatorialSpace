using Concepts.Concepts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Concepts.TextConcepts.English
{
    public class CaseInvariantEnglishCharFragmentsStreamReader : IConceptsFragmentsStreamReader<byte, char>
    {
        private IConceptSystemBuilder<byte, char> conceptSystemBuilder;

        public CaseInvariantEnglishCharFragmentsStreamReader(IConceptSystemBuilder<byte, char> conceptSystemBuilder)
        {
            this.conceptSystemBuilder = conceptSystemBuilder;
        }

        /// <summary>
        /// Reads concepts from the given stream.
        /// </summary>
        /// <param name="stream">Input stream, must be readable.</param>
        /// <param name="identifiersCount">Number of possible contexts. 
        /// Here <see cref="https://habrahabr.ru/post/326334/"/> the number of concepts is 10.</param>
        /// <param name="conceptVectorLength">Length for a concept vector. In the given article 
        /// the length is 256 bit.</param>
        /// <param name="conceptMaskLength">In <see cref="https://habrahabr.ru/post/326334/"/> it is 8 bits. 
        /// In other words this is the number of trues in the concept vector.</param>
        /// <param name="conceptsFragmentLength">The number of concepts (characters, speaking about text) in
        /// one fragment. Here <see cref="https://habrahabr.ru/post/326334/"/> it pertains to a string length.</param>
        /// <returns></returns>
        public IEnumerable<IConceptsFragment<byte, char>> GetConceptsFragments(Stream stream, byte identifiersCount, int conceptVectorLength, int conceptMaskLength, int conceptsFragmentLength, byte initialIdentifier)
        {
            if (initialIdentifier > identifiersCount)
                throw new Exception("Initial Context cannot be greater than contexts count.");

            var conceptSystem = conceptSystemBuilder.Build(identifiersCount, conceptVectorLength, conceptMaskLength);
            StreamReader streamReader = new StreamReader(stream);

            //buffer is a frame of size N in Alexey Redozubov terminology here:
            //https://habrahabr.ru/post/326334/
            char[] buffer = new char[conceptsFragmentLength];

            byte contextIdx = 0;
            while(!streamReader.EndOfStream)
            {
                int readCharsNumber = streamReader.Read(buffer, 0, buffer.Length);

                CharConceptsFragment charConceptsFragment = new CharConceptsFragment();

                for (int i = 0; i < readCharsNumber; i++)
                {
                    char symbol = buffer[i];
                    if(Char.IsLetter(symbol))
                    {
                        char lowerCaseChar = Char.ToLower(symbol);
                        //this is a cyclic identifier. its maximum value is contextsCount - 1 because 
                        //indexes starts from  0. initialContext is a shift from the initial context index (0)
                        //For example, if initialContext is 2, then the first character is considered to be in the 
                        //2nd context, and all characters positions for the same stream are shifted in 2 positions.
                        int refinedContextIdx = (contextIdx + initialIdentifier) % (identifiersCount - 1);
                        //find appropriate concept system for it by the character and symbol position
                        var conceptItem = (from ci in conceptSystem.AsParallel()
                                          where ci.Key == refinedContextIdx && ci.Value == lowerCaseChar
                                          select ci).FirstOrDefault();

                        charConceptsFragment.AddConcept(conceptItem);
                    }
                }

                yield return charConceptsFragment;
            }
        }
    }
}
