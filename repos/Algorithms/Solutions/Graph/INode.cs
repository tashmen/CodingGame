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
        INode StartNode { get; }
        INode EndNode { get; }
        long Distance { get; }
        long GetDistance(List<ILink> currentPath);
    }

    public class Link : ILink
    {
        public INode StartNode { get; private set; }
        public INode EndNode { get; private set; }
        public long Distance { get; private set; }

        public Link(INode startNode, INode endNode, long distance)
        {
            StartNode = startNode;
            EndNode = endNode;
            Distance = distance;
        }

        public long GetDistance(List<ILink> currentPath)
        {
            long distance = 0;
            foreach(ILink link in currentPath)
            {
                distance += link.Distance;
            }
            return distance;
        }
    }

}