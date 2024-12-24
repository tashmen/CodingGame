using Algorithms.Graph;
using System.Collections.Generic;
using Xunit;

namespace UnitTest
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

        [Fact]
        public void Test_Paths_FromManyLinks()
        {
            Graph graph = new Graph();

            List<Node> nodeList = new List<Node>();

            for (int i = 0; i < 9; i++)
            {
                nodeList.Add(new Node(i));
                graph.AddNode(nodeList[i]);
            }
            for (int i = 0; i < 9; i++)
            {
                switch (i)
                {
                    case 0:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[1], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[3], 1));
                        break;
                    case 1:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[0], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[2], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[4], 1));
                        break;
                    case 2:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[1], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[5], 1));
                        break;
                    case 3:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[0], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[4], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[6], 1));
                        break;
                    case 4:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[1], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[3], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[5], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[7], 1));
                        break;
                    case 5:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[2], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[4], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[8], 1));
                        break;
                    case 6:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[3], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[7], 1));
                        break;
                    case 7:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[4], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[6], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[8], 1));
                        break;
                    case 8:
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[5], 1));
                        nodeList[i].AddLink(new Link(nodeList[i], nodeList[7], 1));
                        break;
                }
            }

            graph.CalculateShortestPaths();

            Assert.Equal(4, graph.GetShortestPathDistance(nodeList[0], nodeList[8]));
        }
    }
}
