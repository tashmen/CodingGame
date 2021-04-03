using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSolution
{
    public class Move
    {
        private readonly int source;
        private readonly int destination;
        private readonly int count;
        private readonly string moveType;
        private readonly string message;
        public Move()
        {
            moveType = MoveType.Wait;
        }

        public Move(int factoryId)
        {
            source = factoryId;
            moveType = MoveType.Upgrade;
        }

        public Move(int sourceFactory, int destinationFactory, int cyborgCount)
        {
            source = sourceFactory;
            destination = destinationFactory;
            count = cyborgCount;
            moveType = MoveType.Move;
        }
        public Move(int sourceFactory, int destinationFactory)
        {
            moveType = MoveType.Bomb;
            source = sourceFactory;
            destination = destinationFactory;
        }
        public Move(string message)
        {
            moveType = MoveType.Message;
            this.message = message;
        }


        public void PlayMove()
        {
            string move = moveType switch
            {
                MoveType.Move => MoveType.Move + " " + source + " " + destination + " " + count,
                MoveType.Wait => MoveType.Wait,
                MoveType.Bomb => MoveType.Bomb + " " + source + " " + destination,
                MoveType.Message => MoveType.Message + " " + message,
                MoveType.Upgrade => MoveType.Upgrade + " " + source,
                _ => MoveType.Wait
            };
            Console.Write(move);
        }
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
