using Algorithms.Utility;

namespace GameSolution.Entities
{
    public class Int128
    {
        private long _low;
        private long _high;
        public Int128()
        {

        }

        public void SetBit(int bitLocation)
        {
            if(bitLocation < 64)
            {
                _low = BitFunctions.SetBit(_low, bitLocation);
            }
            else
            {
                var highLocation = bitLocation - 64;
                _high = BitFunctions.SetBit(_high, highLocation);
            }
        }
    }
}
