using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPASharp;
using HPASharp.Factories;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using HPASharp.Passabilities;
using Xunit;

namespace HPA_netcore.Tests
{
    public class ConcreteGraphShould
    {
        [Fact]
        public void InstantiateConcreteGraph()
        {
            var graph = new ConcreteGraph(TileType.Octile);
            Assert.NotNull(graph);
        }

        [Theory]
        [InlineData(4, 6)]
        [InlineData(3, 4)]
        [InlineData(0, 3)]
        public void GetConnectionsFromNodeInGraph(int nodeid, int expectedConnections)
        {
            var map = @"
                000
                001
                100
                ";
            ConcreteGraph concreteGraph = GraphFactory.CreateGraph(3, 3, new ExamplePassability(map, 3, 3));

            ConcreteNode node = concreteGraph.GetNode(Id<ConcreteNode>.From(nodeid));
            IEnumerable<Connection<ConcreteNode>> connections = concreteGraph.GetConnections(node.NodeId);

            Assert.Equal(expectedConnections, connections.Count());
        }


    }
}
