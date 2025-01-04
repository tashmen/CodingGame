using System.Linq;

namespace GameSolution.Entities
{
    public class ProteinInfo
    {
        public int[] Proteins;
        public bool[] HasProteins;
        public bool HasHarvestProteins;
        public bool HasBasicProteins;
        public bool HasTentacleProteins;
        public bool HasSporerProteins;
        public bool HasRootProteins;

        public bool[] HasManyProteins;
        public bool HasManyRootProteins;
        public bool HasManyTentacleProteins;
        public bool HasManySporerProteins;
        public bool HasAtLeastTwoMany;

        public bool IsHarvestingRootProteins;
        public bool IsHarvestingTentacleProteins;
        public bool IsHarvestingSporerProteins;
        public bool IsHarvestingBasicProteins;
        public bool IsHarvestingHarvesterProteins;
        public bool[] IsHarvestingProteins;
        public ProteinInfo(int[] proteins, Board board, bool isMine)
        {
            Proteins = proteins;
            HasProteins = proteins.Select(p => p > 0).ToArray();
            HasHarvestProteins = HasProteins[2] && HasProteins[3];
            HasBasicProteins = HasProteins[0];
            HasTentacleProteins = HasProteins[1] && HasProteins[2];
            HasSporerProteins = HasProteins[1] && HasProteins[3];

            HasRootProteins = HasProteins.All(m => m);
            HasManyProteins = proteins.Select(p => p > 10).ToArray();
            HasManyRootProteins = HasManyProteins.All(m => m);
            HasManyTentacleProteins = HasManyProteins[1] && HasManyProteins[2];
            HasManySporerProteins = HasManyProteins[1] && HasManyProteins[3];
            HasAtLeastTwoMany = HasManyProteins.Count(value => value) > 1;

            int[] harvestingProteins = new int[4];
            board.Harvest(isMine, harvestingProteins);
            IsHarvestingProteins = harvestingProteins.Select(p => p > 0).ToArray();
            IsHarvestingHarvesterProteins = IsHarvestingProteins[2] && IsHarvestingProteins[3];
            IsHarvestingBasicProteins = IsHarvestingProteins[0];
            IsHarvestingTentacleProteins = IsHarvestingProteins[1] && IsHarvestingProteins[2];
            IsHarvestingSporerProteins = IsHarvestingProteins[1] && IsHarvestingProteins[3];
            IsHarvestingRootProteins = IsHarvestingProteins[0] && IsHarvestingProteins[1] && IsHarvestingProteins[2] && IsHarvestingProteins[3];

        }
    }
}
