using System.Collections.Generic;
using HPASharp.Infrastructure;
using HPA_netcore.Search;

namespace HPASharp.Graph
{
    public class ConcreteGraph : Graph<ConcreteNode, ConcreteNodeInfo, ConcreteEdge, ConcreteEdgeInfo>
    {
        private readonly TileType _tileType;

        private readonly IDictionary<TileType, Heuristic> _heuristics = new Dictionary<TileType, Heuristic>
        {
            {TileType.Hex, Heuristics.VancouverDistance},
            {TileType.OctileUnicost, Heuristics.OctileUnicostDistance},
            {TileType.Octile, Heuristics.DiagonalDistance},
            {TileType.Tile, Heuristics.ManhattanDistance}
        };

        public ConcreteGraph(TileType tileType) : base(ConcreteNode.CreateNew, (nodeid, cost, info) => ConcreteEdge.CreateNew(nodeid, cost))
        {
            _tileType = tileType;
        }

        public override int GetHeuristic(Id<ConcreteNode> startNodeId, Id<ConcreteNode> targetNodeId)
        {
            Position startPosition = GetNodeInfo(startNodeId).Position;
            Position targetPosition = GetNodeInfo(targetNodeId).Position;

            return _heuristics[_tileType](startPosition, targetPosition);
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

        public static ConcreteNode CreateNew(Id<ConcreteNode> nodeId, ConcreteNodeInfo info)
        {
            return new ConcreteNode(nodeId, info);
        }

        private ConcreteNode(Id<ConcreteNode> nodeId, ConcreteNodeInfo info)
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

        public static ConcreteEdge CreateNew(Id<ConcreteNode> targetNodeId, int cost)
        {
            return new ConcreteEdge(targetNodeId, cost);
        }

        private ConcreteEdge(Id<ConcreteNode> targetNodeId, int cost)
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