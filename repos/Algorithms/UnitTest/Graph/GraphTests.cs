using Algorithms.Graph;
using Xunit;

namespace UnitTest.Utilities
{
    public class GraphTests
    {
        Graph _graph;

        Node node1 = new Node(1);
        Node node2 = new Node(2);
        Node node3 = new Node(3);
        Node node4 = new Node(4);

        public GraphTests()
        {
            _graph = new Graph();

            node1.AddLink(new Link(node1, node2, 1));
            node1.AddLink(new Link(node1, node3, 2));
            node1.AddLink(new Link(node1, node4, 4));

            node2.AddLink(new Link(node2, node3, 1));
            node2.AddLink(new Link(node2, node4, 2));

            _graph.AddNode(node1);
            _graph.AddNode(node2);
            _graph.AddNode(node3);
            _graph.AddNode(node4);
        }

        [Fact]
        public void Test_ShortestPath()
        {
            _graph.CalculateShortestPaths();

            Assert.Equal(node3, _graph.GetNextNodeInShortestPath(node1, node3));
            Assert.Equal(node2, _graph.GetNextNodeInShortestPath(node1, node4));

            Assert.Equal(2, _graph.GetShortestPathDistance(node1, node3));
            Assert.Equal(3, _graph.GetShortestPathDistance(node1, node4));
        }

        [Fact]
        public void Test_Distance()
        {
            Assert.Equal(2, _graph.GetDistance(node1, node3));
            Assert.Equal(4, _graph.GetDistance(node1, node4));
        }
    }
}
