namespace GameSolution.Entities
{
    public class SunPower
    {
        public int mySunPower;
        public int oppSunPower;

        public int GetDifference()
        {
            return mySunPower - oppSunPower;
        }
    }
}
