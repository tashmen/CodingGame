namespace GameSolution
{
    public class Constants
    {
        public enum Owner
        {
            Opponent = -1,
            Neutral = 0,
            Me = 1
        }

        public class EntityTypes
        {
            public const string Factory = "FACTORY";
            public const string Troop = "TROOP";
            public const string Bomb = "BOMB";
        }

        public class MoveType
        {
            public const string Move = "MOVE";
            public const string Bomb = "BOMB";
            public const string Wait = "WAIT";
            public const string Message = "MSG";
            public const string Upgrade = "INC";
        }
    }
}
