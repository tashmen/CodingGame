namespace GameSolution
{
    public static class Constants
    {
        public const int maxTurns = 24;
        public const int maxTreeSize = 3;
        public const int sunReset = 6;
        public const int treeCompleteCost = 4;

        public enum Richness
        {
            Unusable = 0,
            Low = 1,
            Medium = 2,
            High = 3
        }

        public enum TreeSize
        {
            Seed = 0,
            Small = 1,
            Medium = 2,
            Large = 3
        }

        public static class Actions
        {
            public const string WAIT = "WAIT";
            public const string SEED = "SEED";
            public const string GROW = "GROW";
            public const string COMPLETE = "COMPLETE";
        }
    }
}
