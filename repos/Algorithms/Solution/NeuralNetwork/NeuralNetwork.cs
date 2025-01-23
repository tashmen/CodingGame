using Algorithms.Genetic;
using System;
using System.IO;
using System.Linq;

namespace Algorithms.NeuralNetwork
{
    /**The NeuralNetwork Class is used as the 'brain'*/
    public class NeuralNetwork : Individual
    {
        //The array of NeuronLayers keeps track of the neurons within the network
        private NeuronLayer[] neuronLayers;
        //The number of layers of neurons in the network
        private int numLayers;
        //The number of neurons within each layer of the network
        private int[] numNeurons;
        //The number of inputs that go into the first layer of the network
        private int[] numInputs;
        private int nFirstInputs;
        //The total number of weights within the network
        private int totalNumWeightsInNetwork;
        //The fitness of the neural network
        public double Fitness { get; set; }

        Random rand;

        /**Generates all of the layers of the network 
          * @param nLayers- The number of layers of neurons within the network
          * @param nNeurons- The number of neurons within each layer of the network
          * @param nInputs- The number of inputs going into the first layer of the network
          * */
        public NeuralNetwork(int nLayers, int[] nNeurons, int nInputs)
        {
            rand = new Random();
            numLayers = nLayers;
            numNeurons = nNeurons;
            nFirstInputs = nInputs;

            numInputs = new int[nLayers];
            numInputs[0] = nInputs;
            for (int x = 1; x < nLayers; x++)
            {
                numInputs[x] = numNeurons[x - 1];
            }

            neuronLayers = new NeuronLayer[nLayers];

            for (int x = 0; x < nLayers; x++)
            {
                neuronLayers[x] = new NeuronLayer(numNeurons[x], numInputs[x]);
                totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
            }
            Fitness = 0;
        }

        public NeuralNetwork(NeuralNetwork net)
        {
            rand = new Random();
            numLayers = net.numLayers;
            numNeurons = (int[])net.numNeurons.Clone();
            nFirstInputs = net.nFirstInputs;
            numInputs = (int[])net.numInputs.Clone();


            neuronLayers = new NeuronLayer[numLayers];
            for (int x = 0; x < numLayers; x++)
            {
                neuronLayers[x] = new NeuronLayer(numNeurons[x], numInputs[x]);
                totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
            }
            SetWeights(net.GetWeights());
            Fitness = 0;
        }

        public NeuralNetwork(BinaryReader reader)
        {
            numLayers = reader.ReadInt32();
            numNeurons = new int[numLayers];
            for (int x = 0; x < numLayers; x++)
            {
                numNeurons[x] = reader.ReadInt32();
            }
            numInputs = new int[numLayers];
            for (int x = 0; x < numLayers; x++)
            {
                numInputs[x] = reader.ReadInt32();
            }
            nFirstInputs = numInputs[0];
            neuronLayers = new NeuronLayer[numLayers];
            for (int x = 0; x < numLayers; x++)
            {
                neuronLayers[x] = new NeuronLayer(numNeurons[x], numInputs[x]);
                totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
            }
            double[] weights = new double[GetNumWeights()];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = reader.ReadDouble();
            }
            SetWeights(weights);
            Fitness = reader.ReadDouble();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(numLayers);
            for (int x = 0; x < numNeurons.Count(); x++)
            {
                writer.Write(numNeurons[x]);
            }
            for (int x = 0; x < numInputs.Count(); x++)
            {
                writer.Write(numInputs[x]);
            }
            double[] weights = GetWeights();
            foreach (double weight in weights)
            {
                writer.Write(weight);
            }
            writer.Write(Fitness);
        }

        /**Gets the NeuronLayer at a specified location
          * @param location- The specified layer that is to be retrieved
          * @return The NeuronLayer at location
          * */
        public NeuronLayer GetNeuronLayer(int location)
        {
            return neuronLayers[location];
        }

        /**Gets the number of layers within the network
          * @return The number of layers
          * */
        public int GetNumLayers()
        {
            return numLayers;
        }

        /**Gets the number of neurons within the network
          * @return an array that describes how many neurons there are in each layer
          * */
        public int[] GetNumNeurons()
        {
            return numNeurons;
        }

        /**Gets the number of inputs going into each layer within the network
          * @return an array that describes the number of inputs going into each layer
          * */
        public int[] GetNumInputs()
        {
            return numInputs;
        }

        /**Gets the total number of weights within the network
          * @return the total number of weights
          * */
        public int GetNumWeights()
        {
            return totalNumWeightsInNetwork;
        }

        /**Gets all of the current weights being used within the network.  This function goes layer by layer
          * to pull out all of the weights and puts them into a single array.
          * @return an array that contains the value of all of the weights
          * */
        public double[] GetWeights()
        {
            double[] weights = new double[totalNumWeightsInNetwork];
            double[] layerWeights;
            int count = 0;
            for (int x = 0; x < GetNumLayers(); x++)
            {
                layerWeights = GetNeuronLayer(x).GetWeights();
                for (int y = 0; y < layerWeights.Count(); y++)
                {
                    weights[count] = layerWeights[y];
                    count++;
                }
            }
            return weights;
        }

        /**Displays all of the current weights in the network
          * */
        public void DisplayWeights()
        {
            double[] weights = GetWeights();
            for (int x = 0; x < weights.Count(); x++)
            {
                Console.Error.Write(weights[x] + ", ");
            }
            Console.Error.WriteLine();
        }

        /**Sets all of the weights within the network going layer by layer
          * @param weights- The weights to be used for the neural network
          * */
        public void SetWeights(double[] weights)
        {
            int count = 0;
            for (int x = 0; x < GetNumLayers(); x++)
            {
                for (int y = 0; y < neuronLayers[x].GetNumNeurons(); y++)
                {
                    for (int z = 0; z < neuronLayers[x].GetNeuron(y).GetNumWeights(); z++)
                    {
                        neuronLayers[x].GetNeuron(y).SetWeight(z, weights[count]);
                        count++;
                    }
                }
            }
        }

        /**Determines the output of the network given all of the inputs
          * @param inputs- The inputs to the neural network
          * @return The output of the neural network
          * */
        public double[] Output(double[] inputs)
        {
            double[] output = inputs;
            //lets each layer handle its own output and the output of the 
            //previous becomes the input of the next layer
            for (int x = 0; x < GetNumLayers(); x++)
            {
                output = GetNeuronLayer(x).Output(output);
            }
            return output;
        }

        //Individual Methods for the genetic algorithm
        //creates an individual from two parents 
        public Individual CreateBaby(Individual parent1, Individual parent2, double crossOver)
        {
            NeuralNetwork p1 = (NeuralNetwork)parent1;
            NeuralNetwork p2 = (NeuralNetwork)parent2;
            double[] weights = new double[p1.GetNumWeights()];
            double[] p1weights = p1.GetWeights();
            double[] p2weights = p2.GetWeights();
            for (int x = 0; x < p1.GetNumWeights(); x++)
            {
                if (p1.GetNumWeights() * crossOver < x)
                    weights[x] = p1weights[x];
                else weights[x] = p2weights[x];
            }

            SetWeights(weights);
            return this;
        }

        //mutates the individual 
        public void Mutate(double mutationRate)
        {
            double[] weights = GetWeights();
            for (int i = 0; i < weights.Length; i++)
            {
                if (rand.NextDouble() < mutationRate)
                {
                    weights[i] = (rand.NextDouble() * 2 - 1);
                }
            }
            SetWeights(weights);
        }

        //checks if two individuals are equal
        public bool Equals(Individual i)
        {
            double[] weights1 = GetWeights();
            double[] weights2 = ((NeuralNetwork)i).GetWeights();
            for (int x = 0; x < weights1.Count(); x++)
            {
                if (weights1[x] != weights2[x])
                    return false;
            }
            return true;
        }

        public double CalculateFitness()
        {
            throw new NotImplementedException();
        }

        public object GetNextMove()
        {
            throw new NotImplementedException();
        }

        public Individual Clone()
        {
            return new NeuralNetwork(this);
        }
    }
}
