using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.NeuralNetwork
{
    public class NeuronLayer
    {
        //The neurons within this layer
        private Neuron[] neurons;
        //The number of neurons
        private int numNeurons;

        /**Generates a NeuronLayer from the number of neurons and number of inputs
          * @param nNeurons- The number of neurons in this layer
          * @param numInputs- The number of inputs going into this layer
          * */
        public NeuronLayer(int nNeurons, int numInputs)
        {
            numNeurons = nNeurons;
            neurons = new Neuron[numNeurons];

            for (int x = 0; x < numNeurons; x++)
            {
                neurons[x] = new Neuron(numInputs);
            }
        }

        /**Gets a neuron at index location
          * @param location- The location of the neuron
          * @return the neuron at index location
          * */
        public Neuron GetNeuron(int location)
        {
            return neurons[location];
        }

        /**Gets the number of neurons
          * @return the number of neurons
          * */
        public int GetNumNeurons()
        {
            return numNeurons;
        }

        /**Gets all of the weights for this layer
          * @return the values of the weights in this layer
          * */
        public double[] GetWeights()
        {
            double[] weights = new double[neurons[0].GetNumWeights() * numNeurons];
            int count = 0;
            for (int x = 0; x < GetNumNeurons(); x++)
            {
                for (int y = 0; y < neurons[x].GetNumWeights(); y++)
                {
                    weights[count] = neurons[x].GetWeight(y);
                    count++;
                }
            }
            return weights;
        }

        /**Gets the number of weights; calculated by the Number of Neurons times the Number of Inputs going into
          * this layer.
          * @returns The number of weights
          * */
        public int GetNumWeights()
        {
            return GetNumNeurons() * GetNeuron(0).GetNumWeights();
        }

        /**Calculates the output of this layer based on the given inputs
          * @param inputs- The values of the inputs
          * @return The responses of each neuron within this layer
          * */
        public double[] Output(double[] inputs)
        {
            double[] output = new double[GetNumNeurons()];
            double sum = 0;
            int temp = 0;
            for (int x = 0; x < GetNumNeurons(); x++)
            {
                for (int y = 0; y < GetNeuron(x).GetNumWeights() - temp; y++)
                {
                    sum = sum + inputs[y] * GetNeuron(x).GetWeight(y);
                }
                //System.out.println(sum);
                output[x] = Sigmoid(sum);
                sum = 0;
            }

            return output;
        }

        /**Calculates the value of the sigmoid function for a particular x value.  The sigmoid function
          * is an S-shaped graph with f(x)=1/(1+e^(-x)).
          * value- The x value used to calculate the y value
          * */
        public double Sigmoid(double value)
        {
            return (1.0 / (1.0 + Math.Exp(-value)));
        }
    }
}
