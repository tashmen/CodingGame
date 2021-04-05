using GameSolution.Entities;

namespace GameSolution.Moves
{
    public class MoveOption
    {
        public FactoryEntity SourceFactory { get; set; }
        public FactoryEntity TargetFactory { get; set; }
        public int SendCount { get; set; }
        public int BestTargetId { get; set; }
        public MoveOption(FactoryEntity sourceFactory, FactoryEntity targetFactory, int sendCount, int bestTargetId)
        {
            SourceFactory = sourceFactory;
            TargetFactory = targetFactory;
            SendCount = sendCount;
            BestTargetId = bestTargetId;
        }

        public Move GenerateMove()
        {
            return new Move(SourceFactory.Id, BestTargetId, SendCount);
        }
    }
}
