using System.Collections.Generic;
using System.Linq;

namespace GameSolution.Entities
{
    public enum ResourceType
    {
        None = 0,
        Egg = 1,
        Crystal = 2
    }

    public enum BaseType
    {
        NoBase = 0,
        MyBase = 1,
        OppBase = 2
    }

    public class Cell
    {
        public int Index { get; set; }
        public ResourceType ResourceType { get; set; }
        public int ResourceAmount { get; set; }
        public int MyAnts { get; set; }
        public int OppAnts { get; set; }
        public List<int> Neighbors { get; set; }
        public BaseType BaseType { get; set; }

        public Cell(int index, ResourceType resourceType, int resourceAmount, List<int> neighbors)
        {
            Index= index;
            ResourceType= resourceType;
            Neighbors = neighbors;
            ResourceAmount= resourceAmount;
        }

        public Cell(Cell cell)
        {
            Index = cell.Index;
            ResourceType = cell.ResourceType;
            ResourceAmount= cell.ResourceAmount;
            Neighbors = cell.Neighbors.Select(cell=> cell).ToList();
        }

        public void SetAnts(int myAnts, int oppAnts) 
        {
            MyAnts= myAnts;
            OppAnts= oppAnts;
        }

        public void SetResource(int resourcesLeft)
        {
            ResourceAmount= resourcesLeft;
            if(ResourceAmount <= 0)
            {
                ResourceType = ResourceType.None;
            }
        }

        public void SetBaseType(BaseType type)
        {
            BaseType= type;
        }

        public Cell Clone()
        {
            return new Cell(this);
        }

        public bool Equals(Cell cell)
        {
            return false;
        }

        public override string ToString()
        {
            //return $"new Cell({Index}, (ResourceType){(int)ResourceType}, {ResourceAmount}, new List<int>(){{{string.Join(',', Neighbors)})}},";
            return $"id: {Index}, rt: {ResourceType}, ra: {ResourceAmount}, bt: {BaseType}, ma: {MyAnts}, oa: {OppAnts}";
        }
    }
}
