using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HPASharp;
using HPASharp.Factories;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using Xunit;

namespace HPA_netcore.Tests
{
    public class HierarchicalMapFactoryShould
    {
        [Fact]
        public void GenerateGraph()
        {
            new ScenarioMaker()
                .GivenTheSampleMap()
                .GivenAnAbstractMapOverTheConcreteOne(10, 2, EntranceStyle.EndEntrance)
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 2, nodes: 12, edges: 46)
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 1, nodes: 66, edges: 260);
        }

        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public void GenerateGraphAndProperlyInsertNewNodesAtDemand(Position p1, Position p2, ExpectationForGraph lvl1Graph, ExpectationForGraph lvl2Graph)
        {
            new ScenarioMaker()
                .GivenTheSampleMap()
                .GivenAnAbstractMapOverTheConcreteOne(10, 2, EntranceStyle.EndEntrance)
                .GivenANewNodeAddedToThePosition(p1)
                .GivenANewNodeAddedToThePosition(p2)
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 2, nodes: lvl2Graph.ExpectedNodes, edges: lvl2Graph.ExpectedEdges)
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 1, nodes: lvl1Graph.ExpectedNodes, edges: lvl1Graph.ExpectedEdges)
                .RestoreTheGraph()
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 2, nodes: 12, edges: 46)
                .AssertGraphAtLevelHasExpectedEdgesAndNodes(level: 1, nodes: 66, edges: 260);
        }


        public class ExpectationForGraph
        {
            public ExpectationForGraph(int expectedNodes, int expectedEdges)
            {
                ExpectedEdges = expectedEdges;
                ExpectedNodes = expectedNodes;
            }

            public int ExpectedEdges { get; }
            public int ExpectedNodes { get; }
        }


        private class TestDataGenerator : IEnumerable<object[]>
        {
            private static Position A_POSITION_1 = new Position(18, 0);
            private static Position A_POSITION_2 = new Position(20, 0);

            private static Position POSITION_IN_ENTRANCE_LVL2_1 = new Position(0, 19);
            private static Position POSITION_IN_ENTRANCE_LVL2_2 = new Position(39, 19);

            private static Position POSITION_IN_ENTRANCE_LVL1_1 = new Position(1, 9);
            private static Position POSITION_IN_ENTRANCE_LVL1_2 = new Position(29, 29);

            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] { A_POSITION_1, A_POSITION_2, new ExpectationForGraph(68, 264), new ExpectationForGraph(14, 56)},
                new object[] { POSITION_IN_ENTRANCE_LVL2_1, new Position(20, 0), new ExpectationForGraph(67, 262), new ExpectationForGraph(13, 54)},
                new object[] { POSITION_IN_ENTRANCE_LVL2_1, POSITION_IN_ENTRANCE_LVL2_2, new ExpectationForGraph(66, 260), new ExpectationForGraph(12, 46)},
                new object[] { POSITION_IN_ENTRANCE_LVL1_1, POSITION_IN_ENTRANCE_LVL2_2, new ExpectationForGraph(66, 260), new ExpectationForGraph(13, 48)},
                new object[] { POSITION_IN_ENTRANCE_LVL1_1, POSITION_IN_ENTRANCE_LVL1_2, new ExpectationForGraph(66, 260), new ExpectationForGraph(14, 58)},
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        private class ScenarioMaker
        {
            private ConcreteMap _concreteMap;
            private HierarchicalMap _abstractMap;
            private readonly HierarchicalMapFactory _abstractMapFactory;

            private readonly List<Id<AbstractNode>> _createdNodes;

            public ScenarioMaker()
            {
                _createdNodes = new List<Id<AbstractNode>>();
                _abstractMapFactory = new HierarchicalMapFactory();
            }

            public ScenarioMaker GivenTheSampleMap()
            {
                IPassability passability = new ExamplePassability();
                _concreteMap = ConcreteMap.Create(40, 40, passability);
                return this;
            }

            public ScenarioMaker GivenAnAbstractMapOverTheConcreteOne(int clusterSize, int maxLevel, EntranceStyle entranceStyle)
            {
                _abstractMap = _abstractMapFactory.CreateHierarchicalMap(_concreteMap, clusterSize, maxLevel, entranceStyle);
                return this;
            }

            public ScenarioMaker GivenANewNodeAddedToThePosition(Position position)
            {
                _createdNodes.Add(_abstractMapFactory.InsertAbstractNode(_abstractMap, position));

                return this;
            }

            public ScenarioMaker AssertGraphAtLevelHasExpectedEdgesAndNodes(int level, int nodes, int edges)
            {
                _abstractMap.SetCurrentLevel(level);
                var numberOfEdges = _abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
                var numberOfNodes = _abstractMap.AbstractGraph.Nodes.Count;

                Assert.Equal(edges, numberOfEdges);
                Assert.Equal(nodes, numberOfNodes);

                return this;
            }

            public ScenarioMaker RestoreTheGraph()
            {
                foreach (var createdNode in _createdNodes)
                {
                    _abstractMapFactory.RemoveAbstractNode(_abstractMap, createdNode);
                }

                return this;
            }
        }
    }
}
