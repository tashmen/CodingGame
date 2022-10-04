namespace GameSolution.Game
{
    public class InternalRandom
    {
        static int mask = 2_147_483_647;

        public static int rand(ref int seed, int bound)
        {
            seed = (seed * 1_103_515_245 + 12_345) & mask;
            return seed % bound;
        }
    }
}
