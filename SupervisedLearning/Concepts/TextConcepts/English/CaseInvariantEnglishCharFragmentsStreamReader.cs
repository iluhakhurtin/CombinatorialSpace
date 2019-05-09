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
    /// <summary>
    /// Reads an english text splitting it by fragments of given size and returns 
    /// a concept fragment - the representation of the merged concepts (from the 
    /// concept system) in the fragment. 
    /// </summary>
    public class CaseInvariantEnglishCharFragmentsStreamReader : IConceptsFragmentsStreamReader<byte, char>
    {
        private byte keysCount;
        private IList<IConceptItem<byte, char>> conceptSystem;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conceptSystem"></param>
        /// <param name="keysCount">Usually equals to the number of possible contexts (keys) that are in the concept system.</param>
        public CaseInvariantEnglishCharFragmentsStreamReader(IEnumerable<IConceptItem<byte, char>> conceptSystem, byte keysCount)
        {
            if(conceptSystem == null)
                throw new ArgumentNullException("conceptSystem");

            this.conceptSystem = conceptSystem as IList<IConceptItem<byte, char>>;
            if (this.conceptSystem == null)
                this.conceptSystem = conceptSystem.ToList();

            this.keysCount = keysCount;
        }

        /// <summary>
        /// Reads concepts from the given stream.
        /// </summary>
        /// <param name="stream">Input stream, must be readable.</param>
        /// <param name="keysCount">Number of possible shifts. 
        /// Here <see cref="https://habrahabr.ru/post/326334/"/> the number of shifts is K and equals to 10.</param>
        /// <param name="initialKey">Means starting key (position). Think a key for a text is a starting position 
        /// from 0 to 9. So, initial key means that is starts from it, not from 0 (for example, from 2 instead of 0).</param>
        /// <param name="conceptsFragmentLength">The number of concepts (characters, speaking about text) in
        /// one fragment. Here <see cref="https://habrahabr.ru/post/326334/"/> it pertains to a string length.</param>
        /// <returns></returns>
        public IEnumerable<IConceptsFragment<byte, char>> GetConceptsFragments(Stream stream, int conceptsFragmentLength, byte initialKey = default(byte))
        {
            if (initialKey > keysCount)
                throw new Exception("Initial initialKey (position of a frame) cannot be greater than the number of keys.");

            if(this.conceptSystem == null)
                throw new NullReferenceException("conceptSystem");

            StreamReader streamReader = new StreamReader(stream);

            //buffer is a frame of size N in Alexey Redozubov terminology here:
            //https://habrahabr.ru/post/326334/
            char[] buffer = new char[conceptsFragmentLength];

            byte keyIdx = 0;
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
                        // this is a cyclic identifier. its maximum value is keysCount - 1 because 
                        // indexes start from  0. initialKey is a shift from the initial key index (0)
                        // For example, if initialContext is 2, then the first character is considered to be in the 
                        // 2nd context, and all characters positions for the same stream are shifted in 2 positions.
                        int refinedContextIdx = (keyIdx + initialKey) % (this.keysCount - 1);
                        //find appropriate concept system for it by the character and symbol position
                        var conceptItem = (from ci in conceptSystem.AsParallel()
                                          where ci.Key == refinedContextIdx && ci.Value == lowerCaseChar
                                          select ci).FirstOrDefault();

                        if(conceptItem != null)
                            charConceptsFragment.AddConcept(conceptItem);
                    }
                }

                yield return charConceptsFragment;
            }
        }
    }
}
