using System;
using System.Collections.Generic;
using HPASharp;
using HPASharp.Factories;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using HPASharp.Passabilities;
using HPASharp.Search;
using HPASharp.Smoother;
using System.Linq;
using Xunit;

namespace HPA_netcore.Tests
{
    public class PathfinderIntegrationTest
    {
        [Fact]
        public void CalculatePathsCorrectly()
        {
            
            const int clusterSize = 8;
            const int maxLevel = 2;
            const int height = 70;
            const int width = 70;

            Position startPosition = new Position(1, 0);
            Position endPosition = new Position(69, 69);

            var points = new Position[]
            {
                new Position(1, 0),
                new Position(2, 1),
                new Position(3, 2),
                new Position(4, 3),
                new Position(5, 4),
                new Position(6, 5),
                new Position(7, 6),
                new Position(8, 7),
                // new Position(8, 7), // Error!
                new Position(8, 8),
                new Position(9, 9),
                new Position(10, 10),
                new Position(11, 11),
                new Position(12, 12),
                new Position(13, 13),
                new Position(13, 14),
                new Position(14, 15),
                new Position(15, 16),
                new Position(16, 16),
                new Position(17, 17),
                new Position(18, 17),
                new Position(19, 18),
                new Position(19, 19),
                new Position(20, 20),
                new Position(21, 21),
                new Position(22, 21),
                new Position(23, 22),
                new Position(24, 22),
                new Position(25, 23),
                // new Position(26, 24), // Error!
                new Position(26, 24),
                new Position(27, 25),
                new Position(27, 26),
                new Position(28, 27),
                new Position(28, 28),
                new Position(28, 29),
                new Position(29, 30),
                new Position(30, 31),
                new Position(31, 32),
                // new Position(31, 32), // Error!
                new Position(32, 33),
                new Position(33, 34),
                new Position(34, 34),
                new Position(35, 35),
                new Position(36, 36),
                new Position(37, 37),
                new Position(37, 38),
                new Position(38, 39),
                new Position(38, 40),
                new Position(39, 41),
                new Position(40, 41),
                new Position(41, 42),
                new Position(42, 42),
                new Position(43, 43),
                new Position(44, 43),
                new Position(45, 44),
                new Position(46, 45),
                new Position(47, 45),
                new Position(48, 45),
                new Position(49, 46),
                new Position(50, 46),
                new Position(51, 47),
                new Position(51, 48),
                new Position(52, 49),
                new Position(53, 50),
                new Position(54, 51),
                new Position(54, 52),
                new Position(55, 53),
                new Position(56, 54),
                // new Position(56, 54), // Error
                new Position(57, 55),
                new Position(58, 56),
                new Position(59, 57),
                // new Position(59, 57), // Error
                new Position(60, 58),
                new Position(60, 59),
                new Position(61, 60),
                new Position(61, 61),
                new Position(61, 62),
                new Position(62, 63),
                new Position(62, 64),
                new Position(62, 65),
                new Position(63, 66),
                new Position(64, 67),
                // new Position(64, 67), // Error
                new Position(65, 67),
                new Position(66, 67),
                new Position(67, 67),
                new Position(68, 68),
                new Position(69, 69)
            };

            new ScenarioMaker()
                .GivenAConcreteMap(width, height)
                .GivenAnAbstractMapOverTheConcreteOne(clusterSize, maxLevel, EntranceStyle.EndEntrance)
                .WhenCalculatingThePathBetweenTwoPoints(startPosition, endPosition)
                .AssertThePathIsProperlyCalculated(points);
        }

        [Fact]
        public void CalculatePathsCorrectlyForTheSampleMap()
        {
            const int clusterSize = 10;
            const int maxLevel = 2;

            Position startPosition = new Position(18, 0);
            Position endPosition = new Position(20, 0);

            var points = new Position[]
            {
                new Position(18,0),
                new Position(17,0),
                new Position(16,0),
                new Position(15,0),
                new Position(14,0),
                new Position(13,0),
                new Position(12,0),
                new Position(11,0),
                new Position(10,0),
                new Position(9,0),
                new Position(8,0),
                new Position(7,0),
                new Position(6,0),
                new Position(5,0),
                new Position(4,0),
                new Position(3,0),
                new Position(2,0),
                new Position(1,0),
                new Position(0,1),
                new Position(0,2),
                new Position(0,3),
                new Position(0,4),
                new Position(0,5),
                new Position(0,6),
                new Position(0,7),
                new Position(0,8),
                new Position(0,9),
                new Position(0,10),
                new Position(0,11),
                new Position(0,12),
                new Position(0,13),
                new Position(0,14),
                new Position(0,15),
                new Position(0,16),
                new Position(0,17),
                new Position(0,18),
                //new Position(0,19), // This happens during a cluster change
                new Position(0,19),
                new Position(0,20),
                new Position(0,21),
                new Position(0,22),
                new Position(0,23),
                new Position(0,24),
                new Position(0,25),
                new Position(0,26),
                new Position(0,27),
                new Position(0,28),
                new Position(0,29),
                new Position(0,30),
                new Position(0,31),
                new Position(0,32),
                new Position(0,33),
                new Position(0,34),
                new Position(0,35),
                new Position(0,36),
                new Position(0,37),
                new Position(0,38),
                new Position(1,39),
                new Position(2,39),
                new Position(3,39),
                new Position(4,39),
                new Position(5,39),
                new Position(6,39),
                new Position(7,39),
                new Position(8,39),
                new Position(9,39),
                new Position(10,39),
                new Position(11,39),
                new Position(12,39),
                new Position(13,39),
                new Position(14,39),
                new Position(15,39),
                new Position(16,39),
                new Position(17,39),
                new Position(18,39),
                new Position(19,39),
                new Position(20,39),
                new Position(21,39),
                new Position(22,39),
                new Position(23,39),
                new Position(24,39),
                new Position(25,39),
                new Position(26,39),
                new Position(27,39),
                new Position(28,39),
                new Position(29,39),
                new Position(30,39),
                new Position(31,39),
                new Position(32,39),
                new Position(33,39),
                new Position(34,39),
                new Position(35,39),
                new Position(36,39),
                new Position(37,39),
                new Position(38,39),
                new Position(39,38),
                new Position(39,37),
                new Position(39,36),
                new Position(39,35),
                new Position(39,34),
                new Position(39,33),
                new Position(39,32),
                new Position(39,31),
                new Position(39,30),
                new Position(39,29),
                new Position(39,28),
                new Position(39,27),
                new Position(39,26),
                new Position(39,25),
                new Position(39,24),
                new Position(39,23),
                new Position(39,22),
                new Position(39,21),
                new Position(39,20),
                new Position(39,19),
                new Position(39,18),
                new Position(39,17),
                new Position(39,16),
                new Position(39,15),
                new Position(39,14),
                new Position(39,13),
                new Position(39,12),
                new Position(39,11),
                new Position(39,10),
                new Position(39,9),
                new Position(39,8),
                new Position(39,7),
                new Position(39,6),
                new Position(39,5),
                new Position(39,4),
                new Position(39,3),
                new Position(39,2),
                new Position(39,1),
                new Position(38,0),
                new Position(37,0),
                new Position(36,0),
                new Position(35,0),
                new Position(34,0),
                new Position(33,0),
                new Position(32,0),
                new Position(31,0),
                new Position(30,0),
                new Position(29,0),
                new Position(28,0),
                new Position(27,0),
                new Position(26,0),
                new Position(25,0),
                new Position(24,0),
                new Position(23,0),
                new Position(22,0),
                new Position(21,0),
                new Position(20,0)
            };

            new ScenarioMaker()
                .GivenTheSampleMap()
                .GivenAnAbstractMapOverTheConcreteOne(clusterSize, maxLevel, EntranceStyle.EndEntrance)
                .WhenCalculatingThePathBetweenTwoPoints(startPosition, endPosition)
                .AssertThePathIsProperlyCalculated(points);
        }

        private class ScenarioMaker {
            private ConcreteMap _concreteMap;
            private HierarchicalMap _abstractMap;
            private List<IPathNode> _pathNodes;

            public ScenarioMaker GivenAConcreteMap(int width, int height) {
                IPassability passability = new FakePassability(width, height);
                _concreteMap = ConcreteMap.Create(width, height, passability);
                return this;
            }

            public ScenarioMaker GivenTheSampleMap()
            {
                IPassability passability = new ExamplePassability();
                _concreteMap = ConcreteMap.Create(40, 40, passability);
                return this;
            }

            public ScenarioMaker GivenAnAbstractMapOverTheConcreteOne(int clusterSize, int maxLevel, EntranceStyle entranceStyle)
            {
                var abstractMapFactory = new HierarchicalMapFactory();
			    _abstractMap = abstractMapFactory.CreateHierarchicalMap(_concreteMap, clusterSize, maxLevel, entranceStyle);
                return this;
            }

            public ScenarioMaker WhenCalculatingThePathBetweenTwoPoints(Position start, Position end) {
                _pathNodes = HierarchicalSearch(_abstractMap, start, end);
                return this;
            }

            public ScenarioMaker AssertThePathIsProperlyCalculated(Position[] points)
            {
                List<Position> positions = _pathNodes.Select(node =>
                {
                    if (node is ConcretePathNode)
                    {
                        var concretePathNode = (ConcretePathNode)node;
                        return _concreteMap.Graph.GetNodeInfo(concretePathNode.Id).Position;
                    }

                    var abstractPathNode = (AbstractPathNode)node;
                    return _abstractMap.AbstractGraph.GetNodeInfo(abstractPathNode.Id).Position;
                }).Distinct().ToList();

                Assert.Equal(points, positions);

                return this;
            }

            private List<IPathNode> HierarchicalSearch(HierarchicalMap hierarchicalMap, Position startPosition, Position endPosition)
            {
                Id<AbstractNode> startAbsNode = hierarchicalMap.AddAbstractNode(startPosition);
                Id<AbstractNode> targetAbsNode = hierarchicalMap.AddAbstractNode(endPosition);

                var maxPathsToRefine = int.MaxValue;
                var hierarchicalSearch = new HierarchicalSearchService(new SmoothService(new SearchService<ConcreteNode>()), new SearchService<AbstractNode>());
                List<IPathNode> path = hierarchicalSearch.FindPath(hierarchicalMap, startAbsNode, targetAbsNode, maxPathsToRefine);

                hierarchicalMap.RemoveNode(targetAbsNode);
                hierarchicalMap.RemoveNode(startAbsNode);

                return path;
            }
        }
    }
}
