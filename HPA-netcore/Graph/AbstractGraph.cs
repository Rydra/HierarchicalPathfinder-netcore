using System;
using System.Collections.Generic;
using HPASharp.Infrastructure;
using HPA_netcore.Search;

namespace HPASharp.Graph
{
    public class AbstractGraph : Graph<AbstractNode, AbstractNodeInfo, AbstractEdge, AbstractEdgeInfo>
    {
        private readonly HierarchicalMap _map;

        public AbstractGraph(HierarchicalMap map) : base(AbstractNode.CreateNew, AbstractEdge.CreateNew)
        {
            _map = map;
        }

        public override int GetHeuristic(Id<AbstractNode> startNodeId, Id<AbstractNode> targetNodeId)
        {
            Position startPos = GetNodeInfo(startNodeId).Position;
            Position targetPos = GetNodeInfo(targetNodeId).Position;

            return Heuristics.ManhattanDistance(startPos, targetPos);
        }

        protected override bool IsValid(AbstractEdge edge)
        {
            var targetNode = GetNode(edge.TargetNodeId);
            return _map.PositionInCurrentCluster(targetNode.Info.Position);
        }
    }
    
    public class AbstractNode : INode<AbstractNode, AbstractNodeInfo, AbstractEdge>
    {
        public Id<AbstractNode> NodeId { get; set; }
        public AbstractNodeInfo Info { get; set; }
        public IDictionary<Id<AbstractNode>, AbstractEdge> Edges { get; set; }

        public static AbstractNode CreateNew(Id<AbstractNode> nodeId, AbstractNodeInfo info)
        {
            return new AbstractNode(nodeId, info);
        }

        public AbstractNode(Id<AbstractNode> nodeId, AbstractNodeInfo info)
        {
            NodeId = nodeId;
            Info = info;
            Edges = new Dictionary<Id<AbstractNode>, AbstractEdge>();
        }

        public void RemoveEdge(Id<AbstractNode> targetNodeId)
        {
            Edges.Remove(targetNodeId);
        }

        public void AddEdge(AbstractEdge edge)
        {
            if (!Edges.ContainsKey(edge.TargetNodeId))
            {
                Edges[edge.TargetNodeId] = edge;
            }
        }
    }

    public class AbstractEdge : IEdge<AbstractNode, AbstractEdgeInfo>
    {
        public Id<AbstractNode> TargetNodeId { get; set; }
        public AbstractEdgeInfo Info { get; set; }
        public int Cost { get; set; }

        public static AbstractEdge CreateNew(Id<AbstractNode> targetNodeId, int cost, AbstractEdgeInfo info)
        {
            return new AbstractEdge(targetNodeId, cost, info);
        }

        public AbstractEdge(Id<AbstractNode> targetNodeId, int cost, AbstractEdgeInfo info)
        {
            TargetNodeId = targetNodeId;
            Info = info;
            Cost = cost;
        }
    }

    public class AbstractEdgeInfo
    {
        public List<Id<AbstractNode>> InnerLowerLevelPath { get; set; }
    }
    
    public class AbstractNodeInfo
    {
        public Id<AbstractNode> Id { get; set; }
        public Position Position { get; set; }
        public Id<Cluster> ClusterId { get; set; }
        public Id<ConcreteNode> ConcreteNodeId { get; set; }
        public int Level { get; set; }

        public AbstractNodeInfo(Id<AbstractNode> id, int level, Id<Cluster> clId,
            Position position, Id<ConcreteNode> concreteNodeId)
        {
            Id = id;
            Level = level;
            ClusterId = clId;
            Position = position;
            ConcreteNodeId = concreteNodeId;
        }

        public void PrintInfo()
        {
            Console.Write("id: " + Id);
            Console.Write("; level: " + Level);
            Console.Write("; cluster: " + ClusterId);
            Console.Write("; row: " + Position.Y);
            Console.Write("; col: " + Position.X);
            Console.Write("; center: " + ConcreteNodeId);
            Console.WriteLine();
        }
    }

}