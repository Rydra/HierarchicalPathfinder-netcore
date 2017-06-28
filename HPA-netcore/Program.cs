using System;
using System.Collections.Generic;
using System.Linq;
using HPASharp.Factories;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using HPASharp.Passabilities;
using HPASharp.Search;
using HPASharp.Smoother;

namespace HPASharp
{
    using System.Diagnostics;

    public class Program
    {
        public static void Main2(string[] args)
        {
            const int clusterSize = 8;
            const int maxLevel = 2;
            const int height = 70;
            const int width = 70;

            Position startPosition = new Position(1, 0);
            Position endPosition = new Position(69, 69);

            Execute(width, height, maxLevel, clusterSize, startPosition, endPosition);
        }
        
        public static void Main(string[] args)
        {
            const int clusterSize = 10;
            const int maxLevel = 2;
            const int height = 40;
            const int width = 40;

            Position startPosition = new Position(18, 0);
            Position endPosition = new Position(20, 0);

            Execute(width, height, maxLevel, clusterSize, startPosition, endPosition, true);
        }

        public static void Main10(string[] args)
        {
            const int clusterSize = 8;
            const int maxLevel = 2;
            const int height = 128;
            const int width = 128;

            Position startPosition = new Position(17, 38);
            Position endPosition = new Position(16, 18);

            Execute(width, height, maxLevel, clusterSize, startPosition, endPosition, true);
        }

        public static void Main7(string[] args)
        {
            const int clusterSize = 8;
            const int maxLevel = 2;
            const int height = 128;
            const int width = 128;
            
            IPassability passability = new FakePassability(width, height);

            var concreteMap = ConcreteMap.Create(width, height, passability);

            var abstractMapFactory = new HierarchicalMapFactory();
            var absTiling = abstractMapFactory.CreateHierarchicalMap(concreteMap, clusterSize, maxLevel, EntranceStyle.EndEntrance);

            Func<Position, Position, List<IPathNode>> doHierarchicalSearch = (startPosition, endPosition)
                => HierarchicalSearch(absTiling, startPosition, endPosition);

            Func<Position, Position, List<IPathNode>> doRegularSearch = (startPosition, endPosition)
                => RegularSearch(concreteMap, startPosition, endPosition);

            Func<List<IPathNode>, List<Position>> toPositionPath = (path) =>
                path.Select(p =>
                {
                    if (p is ConcretePathNode)
                    {
                        var concretePathNode = (ConcretePathNode)p;
                        return concreteMap.Graph.GetNodeInfo(concretePathNode.Id).Position;
                    }

                    var abstractPathNode = (AbstractPathNode)p;
                    return absTiling.AbstractGraph.GetNodeInfo(abstractPathNode.Id).Position;
                }).ToList();
            
            var points = Enumerable.Range(0, 2000).Select(_ =>
            {
                var pos1 = ((FakePassability)passability).GetRandomFreePosition();
                var pos2 = ((FakePassability)passability).GetRandomFreePosition();
                while (Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y) < 10)
                {
                    pos2 = ((FakePassability)passability).GetRandomFreePosition();
                }

                return Tuple.Create(pos1, pos2);
            }).ToArray();

            var searchStrategies = new[] { doRegularSearch, doHierarchicalSearch };

            foreach (var searchStrategy in searchStrategies)
            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < points.Length; i++)
                {
                    Position startPosition2 = points[i].Item1;
                    Position endPosition2 = points[i].Item2;
                    var regularSearchPath = searchStrategy(startPosition2, endPosition2);
                    var posPath1 = toPositionPath(regularSearchPath);
                }

                var regularSearchTime = watch.ElapsedMilliseconds;
                Console.WriteLine(regularSearchTime);
            }
        }


        public static void Execute(int width, int height, int maxLevel, int clusterSize, Position startPosition, Position endPosition, bool sample = false)
        {
            // Prepare the abstract graph beforehand
            IPassability passability = sample ? new ExamplePassability() : (IPassability)new FakePassability(width, height);
            var concreteMap = ConcreteMap.Create(width, height, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var absTiling = abstractMapFactory.CreateHierarchicalMap(concreteMap, clusterSize, maxLevel, EntranceStyle.EndEntrance);

            var watch = Stopwatch.StartNew();

            watch = Stopwatch.StartNew();
            var hierarchicalSearchPath = HierarchicalSearch(absTiling, startPosition, endPosition);
            long hierarchicalSearchTime = watch.ElapsedMilliseconds;

            List<Position> pospath = hierarchicalSearchPath.Select(p =>
            {
                if (p is ConcretePathNode)
                {
                    var concretePathNode = (ConcretePathNode)p;
                    return concreteMap.Graph.GetNodeInfo(concretePathNode.Id).Position;
                }

                var abstractPathNode = (AbstractPathNode)p;
                return absTiling.AbstractGraph.GetNodeInfo(abstractPathNode.Id).Position;
            }).ToList();

#if DEBUG
            Console.WriteLine("Hierachical search: " + hierarchicalSearchTime + " ms");
            Console.WriteLine($"{hierarchicalSearchPath.Count} path nodes");
            PrintFormatted(concreteMap, absTiling, clusterSize, pospath);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
#endif
        }

        private static List<IPathNode> HierarchicalSearch(HierarchicalMap hierarchicalMap, Position startPosition, Position endPosition)
        {
            var startAbsNode = hierarchicalMap.AddAbstractNode(startPosition);
            var targetAbsNode = hierarchicalMap.AddAbstractNode(endPosition);
            var maxPathsToRefine = int.MaxValue;
            var hierarchicalSearch = new HierarchicalSearchService(new SmoothService(new SearchService<ConcreteNode>()), new SearchService<AbstractNode>());
            var path = hierarchicalSearch.FindPath(hierarchicalMap, startAbsNode, targetAbsNode, maxPathsToRefine);

            hierarchicalMap.RemoveNode(targetAbsNode);
            hierarchicalMap.RemoveNode(startAbsNode);

            return path;
        }

        private static List<IPathNode> RegularSearch(ConcreteMap concreteMap, Position startPosition, Position endPosition)
        {
            var tilingGraph = concreteMap.Graph;
            ConcreteNode GetNode(int top, int left) => tilingGraph.GetNode(concreteMap.GetNodeIdFromPos(top, left));

            // Regular pathfinding
            var searcher = new SearchService<ConcreteNode>();
            var path = searcher.FindPath(concreteMap.Graph, GetNode(startPosition.X, startPosition.Y).NodeId, GetNode(endPosition.X, endPosition.Y).NodeId);
            var path2 = path.PathNodes;
            return new List<IPathNode>(path2.Select(p => (IPathNode)new ConcretePathNode(p)));
        }

        private static List<char> GetCharVector(ConcreteMap concreteMap)
        {
            var result = new List<char>();
            var numberNodes = concreteMap.NrNodes;
            for (var i = 0; i < numberNodes; i++)
            {
                result.Add(concreteMap.Graph.GetNodeInfo(Id<ConcreteNode>.From(i)).IsObstacle ? '@' : '.');
            }

            return result;
        }

        public static void PrintFormatted(ConcreteMap concreteMap, HierarchicalMap hierarchicalGraph, int clusterSize, List<Position> path)
        {
            PrintFormatted(GetCharVector(concreteMap), concreteMap, hierarchicalGraph, clusterSize, path);
        }

        private static void PrintFormatted(List<char> chars, ConcreteMap concreteMap, HierarchicalMap hierarchicalMap, int clusterSize, List<Position> path)
        {
            hierarchicalMap.SetCurrentLevel(1);
            for (var y = 0; y < concreteMap.Height; y++)
            {
                if (y % clusterSize == 0) Console.WriteLine("---------------------------------------------------------");
                for (var x = 0; x < concreteMap.Width; x++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    if (x % clusterSize == 0) Console.Write('|');

                    var nodeId = concreteMap.GetNodeIdFromPos(x, y);
                    
                    var hasAbsNode = hierarchicalMap.AbstractGraph.Nodes.Values.SingleOrDefault(n => n.Info.ConcreteNodeId == nodeId);

                    if (hasAbsNode != null)
                        switch (hasAbsNode.Info.Level)
                        {
                            case 1:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case 2:
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                break;
                        }

                    Console.Write(path.Any(node => node.X == x && node.Y == y) ? 'X' : chars[nodeId.IdValue]);
                }

                Console.WriteLine();
            }
        }
    }
}
