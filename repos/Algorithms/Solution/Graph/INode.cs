using System.Collections.Generic;

namespace Algorithms.Graph
{
    public interface INode
    {
        int Id { get; }
        bool IsExplored { get; set; }
        List<ILink> GetLinks();
    }

    public class Node : INode
    {
        public int Id { get; private set; }
        public bool IsExplored { get; set; }

        private List<ILink> Links;

        public Node(int id)
        {
            Id = id;
            IsExplored = false;
            Links = new List<ILink>();
        }

        public void AddLink(ILink link)
        {
            Links.Add(link);
        }

        public List<ILink> GetLinks()
        {
            return Links;
        }

        public bool Equals(INode node)
        {
            return node.Id == Id;
        }
    }


    public interface ILink
    {
        int StartNodeId { get; }
        int EndNodeId { get; }
        double Distance { get; }
        double GetDistance(List<ILink> currentPath);
    }

    public class Link : ILink
    {
        public int StartNodeId { get; private set; }
        public int EndNodeId { get; private set; }
        public double Distance { get; private set; }

        public Link(int startNodeId, int endNodeId, double distance)
        {
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Distance = distance;
        }

        public Link(INode startNode, INode endNode, double distance)
        {
            StartNodeId = startNode.Id;
            EndNodeId = endNode.Id;
            Distance = distance;
        }

        public double GetDistance(List<ILink> currentPath)
        {
            double distance = 0;
            foreach(ILink link in currentPath)
            {
                distance += link.Distance;
            }
            return distance;
        }
    }

}