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
                string trainFileName = Console.ReadLine();
                if (String.IsNullOrEmpty(trainFileName))
                    trainFileName = @".\\Texts\\jack_london_children_of_the_frost.txt";

                Console.WriteLine("Enter a path to a text file in english for checking (empty line means using one of the default ones): ");
                string checkFileName = Console.ReadLine();
                if (String.IsNullOrEmpty(checkFileName))
                    checkFileName = @".\\Texts\\isaac_asimov_foundation_1.txt";

                try
                {
                    if (File.Exists(trainFileName) && File.Exists(checkFileName))
                    {
                        //1. Build one char consept system based on zero start index
                        //2. Build another char consept system based on third start index
                        //3. Build a combinatorial space
                        //4. Train the space using the same chars from (1) as input and (2) as output
                        //5. After the training take one concept item from (1) and try to build an output vector 
                        // using the combinatorial space.
                        //6. Compare expected vector from (2) and the built one.

                        byte indexesCount = 10; //number of possible concept systems, 2 just to save memory
                        int conceptVectorLength = 256; //vector size
                        int conceptMaskLength = 8; //number of 'true' in a vector
                        int conceptsFragmentLength = 5; //number of chars read at once

                        //combinatorial space initialization
                        int combinatorialSpaceLength = 60000;
                        int numberOfTrackingBits = 32;
                        int clusterCreationThreshold = 6;
                        int clusterActivationThreshold = 4;
                        int trackingBinaryVectorLength = 256;
                        int outputBinaryVectorLength = 256;

                        IBinaryVectorBuilder binaryVectorBuilder = new RandomBinaryVectorBuilder();
                        IConceptSystemBuilder<byte, char> conceptSystemBuilder = new CaseInvariantEnglishCharConceptSystemBuilder(binaryVectorBuilder);
                        var conceptSystemEnumerable = 
                            conceptSystemBuilder.Build(indexesCount, conceptVectorLength, conceptMaskLength);
                        var conceptSystem = conceptSystemEnumerable.ToList();

                        IConceptsFragmentsStreamReader<byte, char> conceptsFragmentsStreamReader0 = new CaseInvariantEnglishCharFragmentsStreamReader(conceptSystem, indexesCount);
                        IConceptsFragmentsStreamReader<byte, char> conceptsFragmentsStreamReader1 = new CaseInvariantEnglishCharFragmentsStreamReader(conceptSystem, indexesCount);
                        
                        BitArray actualOutputVector = null;

                        //callbacks 

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

                        //training
                        using (FileStream fs0 = new FileStream(trainFileName, FileMode.Open, FileAccess.Read))
                        using (FileStream fs1 = new FileStream(trainFileName, FileMode.Open, FileAccess.Read))
                        {
                            var conceptsFragments0 =
                                conceptsFragmentsStreamReader0.GetConceptsFragments(fs0, conceptsFragmentLength, 0);
                            var conceptsFragments1 =
                                conceptsFragmentsStreamReader1.GetConceptsFragments(fs1, conceptsFragmentLength, 1);

                            Console.WriteLine("To stop trainin press ESC.\r\n");

                            IEnumerator<IConceptsFragment<byte, char>> conceptsFragments0Enumerator =
                                conceptsFragments0.GetEnumerator();
                            IEnumerator<IConceptsFragment<byte, char>> conceptsFragments1Enumerator =
                                conceptsFragments1.GetEnumerator();

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

                                //stop checking by esc pressing
                                if (Console.KeyAvailable)
                                {
                                    ConsoleKeyInfo key = Console.ReadKey(true);
                                    if (key.Key == ConsoleKey.Escape)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        //Check
                        using (FileStream fs0 = new FileStream(checkFileName, FileMode.Open, FileAccess.Read))
                        using (FileStream fs1 = new FileStream(checkFileName, FileMode.Open, FileAccess.Read))
                        {
                            var conceptsFragments0 = conceptsFragmentsStreamReader0.GetConceptsFragments(fs0, conceptsFragmentLength, 0);
                            var conceptsFragments1 = conceptsFragmentsStreamReader1.GetConceptsFragments(fs1, conceptsFragmentLength, 1);

                            var conceptsFragments0Enumerator = conceptsFragments0.GetEnumerator();
                            var conceptsFragments1Enumerator = conceptsFragments1.GetEnumerator();

                            int checkStep = 0;

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

                                checkStep++;

                                PrintCheckingStep(checkStep);

                                //Print metrics. To make it easier to read - pring only every 50th step. Otherwise
                                //the data is updated to quick and it is hard to catch it by an eye.
                                if (checkStep % 50 == 0)
                                {
                                    float precision = CalculatePrecision(actualOutputVector, expectedOutputVector);
                                    PrintPrecision(precision);
                                    float recall = CalculateRecall(actualOutputVector, expectedOutputVector);
                                    PrintRecall(recall);
                                    float f1 = CalculateF1(precision, recall);
                                    PrintF1(f1);
                                }

                                //stop checking by esc pressing
                                if (Console.KeyAvailable)
                                {
                                    ConsoleKeyInfo key = Console.ReadKey(true);
                                    if (key.Key == ConsoleKey.Escape)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Given file does not exist. Try another one.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

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
            int cursorLine = 2;
            Console.SetCursorPosition(0, cursorLine);
            Console.WriteLine("Training step: " + stepNumber);
        }

        private static void PrintCombinatorialSpaceSize(int size)
        {
            //some operations with console
            int cursorLine = 3;
            Console.SetCursorPosition(0, cursorLine);
            string message = "Combinatorial space size: " + size;
            string restOfLine = new string(' ', Console.WindowWidth - message.Length);
            Console.WriteLine(message + restOfLine);
        }

        private static void PrintCheckingStep(int stepNumber)
        {
            //some operations with console
            int cursorLine = 4;
            Console.SetCursorPosition(0, cursorLine);
            Console.WriteLine("Checking step: " + stepNumber);
        }

        private static float CalculatePrecision(BitArray actual, BitArray expected)
        {
            if(expected == null)
                throw new ArgumentNullException("expected");

            if (actual == null)
                throw new ArgumentNullException("actual");

            if(actual.Length != expected.Length)
                throw  new ArgumentException("Length of expected and actual array must be the same.");

            int falsePositivesCount = 0;
            int truePositivesCount = 0;

            for (int i = 0; i < expected.Length; i++)
            {
                if (actual[i] && expected[i])
                {
                    truePositivesCount++;
                }
                else if (actual[i] && !expected[i])
                {
                    falsePositivesCount++;
                }
            }

            float result = (float) truePositivesCount / (truePositivesCount + falsePositivesCount);

            return result;
        }

        private static void PrintPrecision(float precision)
        {
            int cursorLine = 5;
            Console.SetCursorPosition(0, cursorLine);
            Console.WriteLine("Precision: {0:P2}", precision);
        }


        private static float CalculateRecall(BitArray actual, BitArray expected)
        {
            if (expected == null)
                throw new ArgumentNullException("expected");

            if (actual == null)
                throw new ArgumentNullException("actual");

            if (actual.Length != expected.Length)
                throw new ArgumentException("Length of expected and actual array must be the same.");

            int falseNegativesAndTruePositivesCount = 0;
            int truePositivesCount = 0;

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i])
                {
                    falseNegativesAndTruePositivesCount++;
                    if (actual[i])
                    {
                        truePositivesCount++;
                    }
                }
            }

            float result = (float)truePositivesCount / falseNegativesAndTruePositivesCount;

            return result;
        }

        private static void PrintRecall(float recall)
        {
            int cursorLine = 6;
            Console.SetCursorPosition(0, cursorLine);
            Console.WriteLine("Recall: {0:P2}", recall);
        }

        private static float CalculateF1(float precision, float recall)
        {
            float result = 2 * precision * recall / (precision + recall);
            return result;
        }

        private static void PrintF1(float f1)
        {
            int cursorLine = 7;
            Console.SetCursorPosition(0, cursorLine);
            Console.WriteLine("F1: {0:P2}", f1);
        }
    }
}
