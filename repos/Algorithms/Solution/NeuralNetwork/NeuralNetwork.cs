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
        private double fitness;

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
            fitness = 0;
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
            neuronLayers=new NeuronLayer[numLayers];
            for (int x = 0; x < numLayers; x++)
            {
                neuronLayers[x]=new NeuronLayer(numNeurons[x], numInputs[x]);
                totalNumWeightsInNetwork += numNeurons[x] * numInputs[x];
            }
            double[] weights = new double[getNumWeights()];
            for(int i = 0; i<weights.Length; i++)
            {
                weights[i] = reader.ReadDouble();
            }
            setWeights(weights);
            fitness = reader.ReadDouble();
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
            double[] weights = getWeights();
            foreach(double weight in weights)
            {
                writer.Write(weight);
            }
            writer.Write(fitness);
        }

        /**Gets the NeuronLayer at a specified location
          * @param location- The specified layer that is to be retrieved
          * @return The NeuronLayer at location
          * */
        public NeuronLayer getNeuronLayer(int location)
        {
            return neuronLayers[location];
        }

        /**Gets the number of layers within the network
          * @return The number of layers
          * */
        public int getNumLayers()
        {
            return numLayers;
        }

        /**Gets the number of neurons within the network
          * @return an array that describes how many neurons there are in each layer
          * */
        public int[] getNumNeurons()
        {
            return numNeurons;
        }

        /**Gets the number of inputs going into each layer within the network
          * @return an array that describes the number of inputs going into each layer
          * */
        public int[] getNumInputs()
        {
            return numInputs;
        }

        /**Gets the total number of weights within the network
          * @return the total number of weights
          * */
        public int getNumWeights()
        {
            return totalNumWeightsInNetwork;
        }

        /**Gets all of the current weights being used within the network.  This function goes layer by layer
          * to pull out all of the weights and puts them into a single array.
          * @return an array that contains the value of all of the weights
          * */
        public double[] getWeights()
        {
            double[] weights = new double[totalNumWeightsInNetwork];
            double[] layerWeights;
            int count = 0;
            for (int x = 0; x < getNumLayers(); x++)
            {
                layerWeights = getNeuronLayer(x).getWeights();
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
        public void displayWeights()
        {
            double[] weights = getWeights();
            for (int x = 0; x < weights.Count(); x++)
            {
                Console.Error.Write(weights[x] + ", ");
            }
            Console.Error.WriteLine();
        }

        /**Sets all of the weights within the network going layer by layer
          * @param weights- The weights to be used for the neural network
          * */
        public void setWeights(double[] weights)
        {
            int count = 0;
            for (int x = 0; x < getNumLayers(); x++)
            {
                for (int y = 0; y < neuronLayers[x].getNumNeurons(); y++)
                {
                    for (int z = 0; z < neuronLayers[x].getNeuron(y).getNumWeights(); z++)
                    {
                        neuronLayers[x].getNeuron(y).setWeight(z, weights[count]);
                        count++;
                    }
                }
            }
        }

        /**Determines the output of the network given all of the inputs
          * @param inputs- The inputs to the neural network
          * @return The output of the neural network
          * */
        public double[] output(double[] inputs)
        {
            double[] output = inputs;
            //lets each layer handle its own output and the output of the 
            //previous becomes the input of the next layer
            for (int x = 0; x < getNumLayers(); x++)
            {
                output = getNeuronLayer(x).output(output);
            }
            return output;
        }

        //Individual Methods for the genetic algorithm
        //creates an individual from two parents 
        public Individual CreateBaby(Individual parent2, double crossOver)
        {
            double[] weights = new double[getNumWeights()];
            double[] p1weights = getWeights();
            double[] p2weights = ((NeuralNetwork)parent2).getWeights();
            for (int x = 0; x < getNumWeights(); x++)
            {
                if (getNumWeights() * crossOver < x)
                    weights[x] = p1weights[x];
                else weights[x] = p2weights[x];
            }
            NeuralNetwork net = new NeuralNetwork(getNumLayers(), getNumNeurons(), nFirstInputs);
            net.setWeights(weights);
            return net;
        }

        //mutates the individual 
        public void Mutate(double mutationRate)
        {
            double[] weights = getWeights();
            for(int i = 0; i < weights.Length; i++)
            {
                if(rand.NextDouble() < mutationRate)
                {
                    weights[i] = (rand.NextDouble() * 2 - 1);
                }
            }
            setWeights(weights);
        }

        //checks if two individuals are equal
        public bool Equals(Individual i)
        {
            double[] weights1 = getWeights();
            double[] weights2 = ((NeuralNetwork)i).getWeights();
            for (int x = 0; x < weights1.Count(); x++)
            {
                if (weights1[x] != weights2[x])
                    return false;
            }
            return true;
        }

        public double GetFitness()
        {
            return fitness;
        }

        public void SetFitness(double fit)
        {
            fitness = fit;
        }
    }
}
