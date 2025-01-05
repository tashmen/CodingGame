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

        public int[] HarvestingProteins;
        public bool IsHarvestingRootProteins;
        public bool IsHarvestingTentacleProteins;
        public bool IsHarvestingSporerProteins;
        public bool IsHarvestingBasicProteins;
        public bool IsHarvestingHarvesterProteins;
        public bool[] IsHarvestingProteins;
        public bool hasHarvestable;
        public Entity[] toHarvestEntities = null;
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

            HarvestingProteins = new int[4];
            board.Harvest(isMine, HarvestingProteins);
            IsHarvestingProteins = HarvestingProteins.Select(p => p > 0).ToArray();
            IsHarvestingHarvesterProteins = IsHarvestingProteins[2] && IsHarvestingProteins[3];
            IsHarvestingBasicProteins = IsHarvestingProteins[0];
            IsHarvestingTentacleProteins = IsHarvestingProteins[1] && IsHarvestingProteins[2];
            IsHarvestingSporerProteins = IsHarvestingProteins[1] && IsHarvestingProteins[3];
            IsHarvestingRootProteins = IsHarvestingProteins[0] && IsHarvestingProteins[1] && IsHarvestingProteins[2] && IsHarvestingProteins[3];

            Entity[] harvestableEntities = board.GetHarvestableEntities();
            Entity[] harvestingEntities = board.GetHarvestedEntities(isMine);

            HashSet<EntityType> harvestableTypes = harvestableEntities.Select(e => e.Type).ToHashSet();
            HashSet<EntityType> harvestedTypes = harvestingEntities.Select(e => e.Type).ToHashSet();
            hasHarvestable = true;
            if (harvestedTypes.Count < harvestableTypes.Count)
            {
                toHarvestEntities = harvestableEntities.Where(e => !harvestedTypes.Contains(e.Type)).ToArray();
            }
            else
                hasHarvestable = false;

        }
    }
}
