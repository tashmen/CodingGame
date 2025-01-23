using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algorithms.NeuralNetwork
{
    public class Neuron
    {
        //The weights for this neuron
        private double[] weights;
        //The number of weights 
        private int numWeights;

        Random rand;

        /**Creates a neuron with starting weights between -1 and 1
          * @param numInputs- The number of weights needed and the number of inputs
          * */
        public Neuron(int numInputs)
        {
            rand = new Random();
            numWeights = numInputs;
            weights = new double[numInputs];
            for (int x = 0; x < numInputs; x++)
            {
                weights[x] = rand.NextDouble() * 2 - 1;
            }
        }

        /**Gets a weight at a particular location
          * @param location- The location of the weight within the array
          * @return the value of the weight at index location
          */
        public double GetWeight(int location)
        {
            return weights[location];
        }

        /**Gets the number of weights
          * @return the number of weights
          * */
        public int GetNumWeights()
        {
            return numWeights;
        }

        /**Sets the weight at a particular location to the value in weight
          * @param location- The location to put the weight
          * @param weight- The value to put at index location
          * */
        public void SetWeight(int location, double weight)
        {
            weights[location] = weight;
        }
    }
}
