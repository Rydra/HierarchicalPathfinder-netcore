using System;
using System.Collections.Generic;
using HPASharp.Infrastructure;

namespace HPASharp.Graph
{
    public class ConcreteGraph : Graph<ConcreteNode, ConcreteNodeInfo, ConcreteEdge, ConcreteEdgeInfo>
    {
        private readonly TileType _tileType;

        public ConcreteGraph(TileType tileType) : base((nodeid, info) => new ConcreteNode(nodeid, info), (nodeid, cost, info) => new ConcreteEdge(nodeid, cost))
        {
            _tileType = tileType;
        }

        public override int GetHeuristic(Id<ConcreteNode> startNodeId, Id<ConcreteNode> targetNodeId)
        {
            Position startPosition = GetNodeInfo(startNodeId).Position;
            Position targetPosition = GetNodeInfo(targetNodeId).Position;

            var startX = startPosition.X;
            var targetX = targetPosition.X;
            var startY = startPosition.Y;
            var targetY = targetPosition.Y;
            var diffX = Math.Abs(targetX - startX);
            var diffY = Math.Abs(targetY - startY);
            switch (_tileType)
            {
                case TileType.Hex:
                    // Vancouver distance
                    // See P.Yap: Grid-based Path-Finding (LNAI 2338 pp.44-55)
                {
                    var correction = 0;
                    if (diffX % 2 != 0)
                    {
                        if (targetY < startY)
                            correction = targetX % 2;
                        else if (targetY > startY)
                            correction = startX % 2;
                    }

                    // Note: formula in paper is wrong, corrected below.  
                    var dist = Math.Max(0, diffY - diffX / 2 - correction) + diffX;
                    return dist * 1;
                }
                case TileType.OctileUnicost:
                    return Math.Max(diffX, diffY) * Constants.COST_ONE;
                case TileType.Octile:
                    int maxDiff;
                    int minDiff;
                    if (diffX > diffY)
                    {
                        maxDiff = diffX;
                        minDiff = diffY;
                    }
                    else
                    {
                        maxDiff = diffY;
                        minDiff = diffX;
                    }

                    return (minDiff * Constants.COST_ONE * 34) / 24 + (maxDiff - minDiff) * Constants.COST_ONE;

                case TileType.Tile:
                    return (diffX + diffY) * Constants.COST_ONE;
                default:
                    return 0;
            }
        }

        protected override bool IsValid(ConcreteEdge edge)
        {
            ConcreteNode targetNode = GetNode(edge.TargetNodeId);
            return !targetNode.Info.IsObstacle;
        }
    }

    public class ConcreteNode : INode<ConcreteNode, ConcreteNodeInfo, ConcreteEdge>
    {
        public Id<ConcreteNode> NodeId { get; set; }
        public ConcreteNodeInfo Info { get; set; }
        public IDictionary<Id<ConcreteNode>, ConcreteEdge> Edges { get; set; }

        public ConcreteNode(Id<ConcreteNode> nodeId, ConcreteNodeInfo info)
        {
            NodeId = nodeId;
            Info = info;
            Edges = new Dictionary<Id<ConcreteNode>, ConcreteEdge>();
        }

        public void RemoveEdge(Id<ConcreteNode> targetNodeId)
        {
            Edges.Remove(targetNodeId);
        }

        public void AddEdge(ConcreteEdge edge)
        {
            Edges[edge.TargetNodeId] = edge;
        }
    }

    public class ConcreteEdge : IEdge<ConcreteNode, ConcreteEdgeInfo>
    {
        public Id<ConcreteNode> TargetNodeId { get; set; }
        public ConcreteEdgeInfo Info { get; set; }
        public int Cost { get; set; }

        public ConcreteEdge(Id<ConcreteNode> targetNodeId, int cost)
        {
            TargetNodeId = targetNodeId;
            Cost = cost;
        }
    }

    public class ConcreteEdgeInfo
    {
    }

    public class ConcreteNodeInfo
    {
        public ConcreteNodeInfo(bool isObstacle, int cost, Position position)
        {
            IsObstacle = isObstacle;
            Position = position;
            Cost = cost;
        }

        public Position Position { get; set; }
        public bool IsObstacle { get; set; }
        public int Cost { get; set; }
    }
}