using System;
using System.Collections.Generic;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using HPASharp.Search;

namespace HPASharp.Smoother
{
    public class SmoothService : ISmoothService
    {
        private readonly ISearchService<ConcreteNode> _searchService;
        private static readonly Id<ConcreteNode> INVALID_ID = Id<ConcreteNode>.From(Constants.NO_NODE);

        public SmoothService(ISearchService<ConcreteNode> searchService)
        {
            _searchService = searchService;
        }
        
        public List<IPathNode> SmoothPath(ConcreteMap concreteMap, List<IPathNode> path)
        {
            // This is a dictionary, indexed by nodeId, that tells in which order does this node occupy in the path
            var pathMap = new Dictionary<int, int>();

            Position GetPosition(Id<ConcreteNode> nodeId)
            {
                return concreteMap.Graph.GetNodeInfo(nodeId).Position;
            }

            Id<ConcreteNode> AdvanceNode(Id<ConcreteNode> nodeId, int direction)
            {
                var nodeInfo = concreteMap.Graph.GetNodeInfo(nodeId);
                var y = nodeInfo.Position.Y;
                var x = nodeInfo.Position.X;
                
                ConcreteNode GetNode(int top, int left) => concreteMap.Graph.GetNode(concreteMap.GetNodeIdFromPos(top, left));

                switch ((Direction)direction)
                {
                    case Direction.North:
                        if (y == 0)
                            return INVALID_ID;
                        return GetNode(x, y - 1).NodeId;
                    case Direction.East:
                        if (x == concreteMap.Width - 1)
                            return INVALID_ID;
                        return GetNode(x + 1, y).NodeId;
                    case Direction.South:
                        if (y == concreteMap.Height - 1)
                            return INVALID_ID;
                        return GetNode(x, y + 1).NodeId;
                    case Direction.West:
                        if (x == 0)
                            return INVALID_ID;
                        return GetNode(x - 1, y).NodeId;
                    case Direction.NorthEast:
                        if (y == 0 || x == concreteMap.Width - 1)
                            return INVALID_ID;
                        return GetNode(x + 1, y - 1).NodeId;
                    case Direction.SouthEast:
                        if (y == concreteMap.Height - 1 || x == concreteMap.Width - 1)
                            return INVALID_ID;
                        return GetNode(x + 1, y + 1).NodeId;
                    case Direction.SouthWest:
                        if (y == concreteMap.Height - 1 || x == 0)
                            return INVALID_ID;
                        return GetNode(x - 1, y + 1).NodeId;
                    case Direction.NorthWest:
                        if (y == 0 || x == 0)
                            return INVALID_ID;
                        return GetNode(x - 1, y - 1).NodeId;
                    default:
                        return INVALID_ID;
                }
            }

            // Returns the next node in the init path in a straight line that
            // lies in the same direction as the origin node
            Id<ConcreteNode> AdvanceThroughDirection(Id<ConcreteNode> originId, int direction)
            {
                var nodeId = originId;
                var lastNodeId = originId;
                while (true)
                {
                    // advance in the given direction
                    nodeId = AdvanceNode(nodeId, direction);

                    // If in the direction we advanced there was an invalid node or we cannot enter the node,
                    // just return that no node was found
                    if (nodeId == INVALID_ID || !concreteMap.CanJump(GetPosition(nodeId), GetPosition(lastNodeId)))
                        return INVALID_ID;

                    // Otherwise, if the node we advanced was contained in the original path, and
                    // it was positioned after the node we are analyzing, return it
                    if (pathMap.ContainsKey(nodeId.IdValue) && pathMap[nodeId.IdValue] > pathMap[originId.IdValue])
                    {
                        return nodeId;
                    }

                    // If we have found an obstacle, just return that no next node to advance was found
                    var newNodeInfo = concreteMap.Graph.GetNodeInfo(nodeId);
                    if (newNodeInfo.IsObstacle)
                        return INVALID_ID;

                    lastNodeId = nodeId;
                }
            }

            int DecideNextPathNodeToConsider(int index)
            {
                var newIndex = index;
                for (var dir = (int)Direction.North; dir <= (int)Direction.NorthWest; dir++)
                {
                    if (concreteMap.TileType == TileType.Tile && dir > (int)Direction.West)
                        break;

                    var seenPathNode = AdvanceThroughDirection(Id<ConcreteNode>.From(path[index].IdValue), dir);

                    if (seenPathNode == INVALID_ID)
                        // No node in advance in that direction, just continue
                        continue;
                    if (index > 0 && seenPathNode.IdValue == path[index - 1].IdValue)
                        // If the point we are advancing is the same as the previous one, we didn't
                        // improve at all. Just continue looking other directions
                        continue;
                    if (index < path.Count - 1 && seenPathNode.IdValue == path[index + 1].IdValue)
                        // If the point we are advancing is the same as a next node in the path,
                        // we didn't improve either. Continue next direction
                        continue;

                    newIndex = pathMap[seenPathNode.IdValue] - 2;

                    // count the path reduction (e.g., 2)
                    break;
                }

                return newIndex;
            }

            List<Id<ConcreteNode>> GenerateIntermediateNodes(Id<ConcreteNode> nodeid1, Id<ConcreteNode> nodeid2)
            {
                Path<ConcreteNode> pathFound = _searchService.FindPath(concreteMap.Graph, nodeid1, nodeid2);
                return pathFound.PathNodes;
            }

            for (var i = 0; i < path.Count; i++)
            {
                pathMap[path[i].IdValue] = i + 1;
            }

            var smoothedPath = new List<IPathNode>();
            var smoothedConcretePath = new List<ConcretePathNode>();
			var pathNodePosition = 0;
            for (; pathNodePosition < path.Count && path[pathNodePosition] is ConcretePathNode; pathNodePosition++)
            {
				var pathNode = (ConcretePathNode)path[pathNodePosition];
				if (smoothedConcretePath.Count == 0)
					smoothedConcretePath.Add(pathNode);

                // add this node to the smoothed path
                if (smoothedConcretePath[smoothedConcretePath.Count - 1].Id != pathNode.Id)
                {
                    // It's possible that, when smoothing, the next node that will be put in the path
                    // will not be adjacent. In those cases, since OpenRA requires a continuous path
                    // without breakings, we should calculate a new path for that section
                    var lastNodeInSmoothedPath = smoothedConcretePath[smoothedConcretePath.Count - 1];
                    var currentNodeInPath = pathNode;

                    if (!AreAdjacent(GetPosition(lastNodeInSmoothedPath.Id), GetPosition(currentNodeInPath.Id)))
                    {
                        var intermediatePath = GenerateIntermediateNodes(smoothedConcretePath[smoothedConcretePath.Count - 1].Id, pathNode.Id);
	                    for (int i = 1; i < intermediatePath.Count; i++)
	                    {
							smoothedConcretePath.Add(new ConcretePathNode(intermediatePath[i]));
						}
                    }

					smoothedConcretePath.Add(pathNode);
                }

                pathNodePosition = DecideNextPathNodeToConsider(pathNodePosition);
            }

	        foreach (var pathNode in smoothedConcretePath)
	        {
				smoothedPath.Add(pathNode);
			}

	        for (;pathNodePosition < path.Count; pathNodePosition++)
		    {
				smoothedPath.Add(path[pathNodePosition]);
			}

			return smoothedPath;
        }
        
	    private static bool AreAdjacent(Position a, Position b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) <= 2;
        }
    }
}
