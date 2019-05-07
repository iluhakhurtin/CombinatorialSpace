using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CombinatorialSpace.BinaryVectors;
using Concepts.BinaryVectorsBuilders;
using Concepts.Concepts;
using Concepts.TextConcepts.English;

namespace SupervisedLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This application shows learning of a system.");
            while (true)
            {
                Console.WriteLine("Enter a path to a text file in english for training (empty line means using one of the default ones): ");
                string fileName = Console.ReadLine();
                //try
                //{
                    if (String.IsNullOrEmpty(fileName))
                        fileName = @".\\Texts\\jack_london_children_of_the_frost.txt";


                    if (File.Exists(fileName))
                    {
                        //1. Build one char consept system based on zero start index
                        //2. Build another char consept system based on third start index
                        //3. Build a combinatorial space
                        //4. Train the space using the same chars from (1) as input and (2) as output
                        //5. After the training take one concept item from (1) and try to build an output vector 
                        // using the combinatorial space.
                        //6. Compare expected vector from (2) and the built one.

                        IBinaryVectorBuilder binaryVectorBuilder = new RandomBinaryVectorBuilder();
                        IConceptSystemBuilder<byte, char> conceptSystemBuilder = new CaseInvariantEnglishCharConceptSystemBuilder(binaryVectorBuilder);
                        IConceptsFragmentsStreamReader<byte, char> conceptsFragmentsStreamReader = new CaseInvariantEnglishCharFragmentsStreamReader(conceptSystemBuilder);
                        
                        byte indexesCount = 2; //number of possible concept systems, 2 just to save memory
                        int conceptVectorLength = 256; //vector size
                        int conceptMaskLength = 8; //number of true in a vector
                        int conceptsFragmentLength = 5; //number of chars read at once

                        //combinatorial space initialization
                        int combinatorialSpaceLength = 600000;
                        int numberOfTrackingBits = 32;
                        int clusterCreationThreshold = 6;
                        int clusterActivationThreshold = 4;
                        int trackingBinaryVectorLength = 256;
                        int outputBinaryVectorLength = 256;

                        BitArray actualOutputVector = null;

                        PointActivatedEventHandler pointActivatedEventHandler = (sender, e) =>
                        {
                            actualOutputVector.Set(e.OutputBitIndex, true);
                        };

                        var combinatorialSpaceWithClusters = new HashSet<IPoint>();
                        
                        ClusterCreatedEventHandler clusterCreatedEventHandler = (sender, e) =>
                        {
                            combinatorialSpaceWithClusters.Add((IPoint)sender);
                            PrintCombinatorialSpaceSize(combinatorialSpaceWithClusters.Count);
                        };

                        ClusterDestroyedEventHandler clusterDestroyedEventHandler = (sender, e) =>
                        {
                            combinatorialSpaceWithClusters.Remove((IPoint)sender);
                            PrintCombinatorialSpaceSize(combinatorialSpaceWithClusters.Count);
                        };

                        ICombinatorialSpaceBuilder combinatorialSpaceBuilder = new CombinatorialSpaceBuilder();

                        IEnumerable<IPoint> combinatorialSpaceEnumerable = combinatorialSpaceBuilder.Build(
                            combinatorialSpaceLength,
                            numberOfTrackingBits,
                            clusterCreationThreshold,
                            clusterActivationThreshold,
                            trackingBinaryVectorLength,
                            outputBinaryVectorLength,
                            pointActivatedEventHandler,
                            clusterCreatedEventHandler,
                            clusterDestroyedEventHandler);

                        IList<IPoint> combinatorialSpace = combinatorialSpaceEnumerable.ToList();

                        ResetConsole();

                        using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        using (FileStream fs1 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            //Train
                            var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 0);
                            var conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 1);

                            Console.WriteLine("To stop trainin press ESC.\r\n");

                            IEnumerator<IConceptsFragment<byte, char>> conceptsFragments0Enumerator = conceptsFragments0.GetEnumerator();
                            IEnumerator<IConceptsFragment<byte, char>> conceptsFragments1Enumerator = conceptsFragments1.GetEnumerator();

                            int trainStep = 0;

                            while (conceptsFragments0Enumerator.MoveNext() &&
                                   conceptsFragments1Enumerator.MoveNext())
                            {
                                //Training itself
                                //combinatorialSpace.AsParallel().ForAll(point => point.Train(conceptFragment0.Vector, conceptFragment1.Vector));

                                var trainingInputVector = conceptsFragments0Enumerator.Current.Vector;
                                var trainingOutputVector = conceptsFragments1Enumerator.Current.Vector;

                                foreach (IPoint point in combinatorialSpace)
                                {
                                    point.Train(trainingInputVector, trainingOutputVector);
                                }

                                PrintTrainingStep(trainStep++);
                                PrintCombinatorialSpaceSize(combinatorialSpaceWithClusters.Count);

                                if (Console.KeyAvailable)
                                {
                                    ConsoleKeyInfo key = Console.ReadKey(true);
                                    if (key.Key == ConsoleKey.Escape)
                                    {
                                        break;
                                    }
                                }
                            }

                            //Check
                            fs0.Seek(0, SeekOrigin.Begin);
                            fs1.Seek(0, SeekOrigin.Begin);

                            conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 0);
                            conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 1);

                            conceptsFragments0Enumerator = conceptsFragments0.GetEnumerator();
                            conceptsFragments1Enumerator = conceptsFragments1.GetEnumerator();

                            while (conceptsFragments0Enumerator.MoveNext() &&
                                   conceptsFragments1Enumerator.MoveNext())
                            {
                                var checkInputVector = conceptsFragments0Enumerator.Current.Vector;
                                var expectedOutputVector = conceptsFragments1Enumerator.Current.Vector;

                                if (checkInputVector == null || expectedOutputVector == null)
                                    continue;

                                actualOutputVector = new BitArray(expectedOutputVector.Length);

                                foreach (IPoint point in combinatorialSpaceWithClusters)
                                {
                                    //see callback for point activated - the bit in the actual output vector is set there
                                    point.Check(checkInputVector);
                                }

                                PrintVectorsDifference(expectedOutputVector, actualOutputVector);
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Given file does not exist. Try another one.");
                    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex);
                //}

                Console.WriteLine("Do you want to continue? y/n");
                string answer = Console.ReadLine();
                answer = answer.ToUpper();
                if (answer != "Y" && answer != "YES")
                {
                    break;
                }

                Console.ReadLine();
            }
        }

        private static void ResetConsole()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        private static void PrintTrainingStep(int stepNumber)
        {
            //some operations with console
            Console.SetCursorPosition(0, 2);
            Console.WriteLine("Training step: " + stepNumber);
        }

        private static void PrintCombinatorialSpaceSize(int size)
        {
            //some operations with console
            Console.SetCursorPosition(0, 3);
            Console.WriteLine("Combinatorial space size: " + size);
        }

        private static void PrintVectorsDifference(BitArray expectedOutputVector, BitArray actualOutputVector)
        {
            BitArray xorResult = expectedOutputVector.Xor(actualOutputVector);
            int differencesCount = 0;
            foreach (bool bit in xorResult)
            {
                if (bit)
                    differencesCount++;
            }

            Console.WriteLine("Number of different bits: " + differencesCount);
        }
    }
}
