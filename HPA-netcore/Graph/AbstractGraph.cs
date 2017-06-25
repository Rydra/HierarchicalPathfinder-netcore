using System;
using HPASharp.Infrastructure;

namespace HPASharp.Graph
{
    public class AbstractGraph : Graph<AbstractNode, AbstractNodeInfo, AbstractEdge, AbstractEdgeInfo>
    {
        private readonly HierarchicalMap _map;

        public AbstractGraph(HierarchicalMap map) : base((nodeid, info) => new AbstractNode(nodeid, info), (nodeid, cost, info) => new AbstractEdge(nodeid, cost, info))
        {
            _map = map;
        }

        public override int GetHeuristic(Id<AbstractNode> startNodeId, Id<AbstractNode> targetNodeId)
        {
            var startPos = GetNodeInfo(startNodeId).Position;
            var targetPos = GetNodeInfo(targetNodeId).Position;
            var diffY = Math.Abs((int) (startPos.Y - targetPos.Y));
            var diffX = Math.Abs((int) (startPos.X - targetPos.X));

            // Manhattan distance, after testing a bit for hierarchical searches we do not need
            // the level of precision of Diagonal distance or euclidean distance
            return (diffY + diffX) * Constants.COST_ONE;
        }

        protected override bool IsValid(AbstractEdge edge)
        {
            var targetNode = GetNode(edge.TargetNodeId);
            return _map.PositionInCurrentCluster(targetNode.Info.Position);
        }
    }
}