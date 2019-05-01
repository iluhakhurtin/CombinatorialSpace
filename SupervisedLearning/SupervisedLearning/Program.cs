using System;
using System.Collections;
using System.IO;
using System.Linq;
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
                try
                {
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
                        int combinatorialSpaceLength = 60000;
                        int numberOfTrackingBits = 32;
                        int clusterCreationThreshold = 6;
                        int clusterActivationThreshold = 4;
                        int trackingBinaryVectorLength = 256;
                        int outputBinaryVectorLength = 256;

                        PointActivatedEventHandler pointActivatedEventHandler = (sender, e) =>
                        {
                            
                        };

                        ICombinatorialSpaceBuilder combinatorialSpaceBuilder = new CombinatorialSpaceBuilder();

                        var combinatorialSpace = combinatorialSpaceBuilder.Build(
                            combinatorialSpaceLength,
                            numberOfTrackingBits,
                            clusterCreationThreshold,
                            clusterActivationThreshold,
                            trackingBinaryVectorLength,
                            outputBinaryVectorLength,
                            pointActivatedEventHandler);
                        
                        using (FileStream fs0 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        using (FileStream fs1 = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            //Train
                            var conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 0);
                            var conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 1);

                            Console.WriteLine("To stop trainin press S.");
                            int trainStep = 0;
                            Console.WriteLine("Train Step: ");

                            bool stopTraining = false;

                            foreach (var conceptFragment0 in conceptsFragments0)
                            {
                                if (stopTraining)
                                {
                                    break;
                                }

                                foreach (var conceptFragment1 in conceptsFragments1)
                                {
                                    if (stopTraining)
                                    {
                                        break;
                                    }

                                    Console.Write("\r{0} ", trainStep++);

                                    //ConsoleKeyInfo key = Console.ReadKey();
                                    //if (key.KeyChar == 's')
                                    //{
                                    //    stopTraining = true;
                                    //}

                                    //Training itself
                                    combinatorialSpace.AsParallel().ForAll(point => point.Train(conceptFragment0.Vector, conceptFragment1.Vector));
                                }
                            }

                            //fs0.Seek(0, SeekOrigin.Begin);
                            //fs1.Seek(0, SeekOrigin.Begin);

                            ////Check
                            //conceptsFragments0 = conceptsFragmentsStreamReader.GetConceptsFragments(fs0, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 0);
                            //conceptsFragments1 = conceptsFragmentsStreamReader.GetConceptsFragments(fs1, indexesCount, conceptVectorLength, conceptMaskLength, conceptsFragmentLength, 1);

                            //foreach (var conceptFragment0 in conceptsFragments0)
                            //{
                            //    foreach (var conceptFragment1 in conceptsFragments1)
                            //    {
                            //        combinatorialSpace.AsParallel().ForAll(point => point.Check(conceptFragment0.Vector));
                            //    }
                            //}
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

                Console.WriteLine();
            }
        }
    }
}
