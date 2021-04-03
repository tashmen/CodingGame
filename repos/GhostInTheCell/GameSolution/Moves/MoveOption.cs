using GameSolution.Entities;

namespace GameSolution.Moves
{
    public class MoveOption
    {
        public FactoryEntity SourceFactory { get; set; }
        public FactoryEntity TargetFactory { get; set; }
        public int SendCount { get; set; }
        public int BestTargetId { get; set; }
        public int DefendCount { get; set; }
        public MoveOption(FactoryEntity sourceFactory, FactoryEntity targetFactory, int sendCount, int bestTargetId, int defendCount)
        {
            SourceFactory = sourceFactory;
            TargetFactory = targetFactory;
            SendCount = sendCount;
            BestTargetId = bestTargetId;
            DefendCount = defendCount;
        }

        public Move GenerateMove()
        {
            return new Move(SourceFactory.Id, BestTargetId, SendCount);
        }
    }
}
