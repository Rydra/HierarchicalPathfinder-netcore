using System;
using System.Collections.Generic;
using System.Linq;
using HPASharp.Graph;
using HPASharp.Infrastructure;
using HPASharp.Search;
using HPA_netcore.Graph;

namespace HPASharp
{
    /// <summary>
    /// Abstract maps represent, as the name implies, an abstraction
    /// built over the concrete map.
    /// </summary>
    public class HierarchicalMap
    {
        public int Height { get; set; }
        public int Width { get; set; }
        
        public GraphLayers GraphLayers { get; set; }
        public AbstractGraph AbstractGraph => GraphLayers.AbstractGraph;

        public int ClusterSize { get; set; }
        public int MaxLevel { get; set; }
        public List<Cluster> Clusters { get; set; }

	    public int NrNodes => AbstractGraph.Nodes.Count;

        // This list, indexed by a node id from the low level, 
        // indicates to which abstract node id it maps. It is a sparse
        // array for quick access. For saving memory space, this could be implemented as a dictionary
        // NOTE: It is currently just used for insert and remove STAL
        public Dictionary<Id<ConcreteNode>, Id<AbstractNode>> ConcreteNodeIdToAbstractNodeIdMap { get; set; }
        public AbstractType Type { get; set; }
        public ConcreteMap ConcreteMap { get; set; }
		
		private int currentClusterY0;

		private int currentClusterY1;

		private int currentClusterX0;

		private int currentClusterX1;

		public void SetType(TileType tileType)
        {
            switch(tileType)
            {
                case TileType.Tile:
                    Type = AbstractType.ABSTRACT_TILE;
                    break;
                case TileType.Octile:
                    Type = AbstractType.ABSTRACT_OCTILE;
                    break;
                case TileType.OctileUnicost:
                    Type = AbstractType.ABSTRACT_OCTILE_UNICOST;
                    break;
            }
        }

        public HierarchicalMap(ConcreteMap concreteMap, int clusterSize, int maxLevel)
        {
            ClusterSize = clusterSize;
            MaxLevel = maxLevel;
            
            SetType(concreteMap.TileType);
            Height = concreteMap.Height;
            Width = concreteMap.Width;
            ConcreteMap = concreteMap;
            ConcreteNodeIdToAbstractNodeIdMap = new Dictionary<Id<ConcreteNode>, Id<AbstractNode>>();

            Clusters = new List<Cluster>();
            GraphLayers = new GraphLayers(maxLevel, this);
        }
		
		public Cluster FindClusterForPosition(Position pos)
		{
		    Cluster foundCluster = null;
            foreach (var cluster in Clusters)
            {
                if (cluster.Origin.Y <= pos.Y &&
                    pos.Y < cluster.Origin.Y + cluster.Size.Height &&
                    cluster.Origin.X <= pos.X &&
                    pos.X < cluster.Origin.X + cluster.Size.Width)
                {
                    foundCluster = cluster;
                    break;
                }
            }

		    return foundCluster;
        }

        public void AddEdge(Id<AbstractNode> sourceNodeId, Id<AbstractNode> destNodeId, int cost, List<Id<AbstractNode>> pathPathNodes = null)
        {
	        var edgeInfo = new AbstractEdgeInfo();
	        edgeInfo.InnerLowerLevelPath = pathPathNodes;

			AbstractGraph.AddEdge(sourceNodeId, destNodeId, cost, edgeInfo);
        }

        public List<AbstractEdge> GetNodeEdges(Id<ConcreteNode> nodeId)
        {
            var node = AbstractGraph.GetNode(ConcreteNodeIdToAbstractNodeIdMap[nodeId]);
            return node.Edges.Values.ToList();
        }

        public Cluster GetCluster(Id<Cluster> id)
        {
            return Clusters[id.IdValue];
        }
		
	    private void RemoveAbstractNode(Id<AbstractNode> abstractNodeId)
	    {
	        AbstractNodeInfo abstractNodeInfo = AbstractGraph.GetNodeInfo(abstractNodeId);
            Cluster cluster = Clusters[abstractNodeInfo.ClusterId.IdValue];
	        cluster.RemoveLastEntranceRecord();
	        ConcreteNodeIdToAbstractNodeIdMap.Remove(abstractNodeInfo.ConcreteNodeId);
            
	        if (AbstractGraph.NodeExists(abstractNodeId))
	        {
	            AbstractGraph.RemoveEdgesFromAndToNode(abstractNodeId);
	            AbstractGraph.Remove(abstractNodeId);
	        }
	    }

        public Id<AbstractNode> AddAbstractNode(Position pos)
        {
            var nodeId = Id<ConcreteNode>.From(pos.Y * Width + pos.X);

            SetCurrentLevel(1);
            Id<AbstractNode> abstractNodeId = InsertNodeIntoHierarchicalMap(nodeId, pos);
            AddHierarchicalEdgesForAbstractNode(abstractNodeId);
            return abstractNodeId;
        }

        public void RemoveNode(Id<AbstractNode> nodeId)
        {
            if (nodeBackups.ContainsKey(nodeId))
            {
                RestoreNodeBackup(nodeId);
            }
            else
            {
                for (int i = 1; i <= MaxLevel; i++)
                {
                    SetCurrentLevel(i);
                    RemoveAbstractNode(nodeId);
                }
            }
        }
        
        private void RestoreNodeBackup(Id<AbstractNode> nodeId)
        {
            var nodeBackupList = nodeBackups[nodeId];
            int maxLevel = nodeBackupList.Max(x => x.Level);

            foreach (var nodeBackup in nodeBackupList)
            {
                var level = nodeBackup.Level;
                SetCurrentLevel(level);

                var nodeInfo = AbstractGraph.GetNodeInfo(nodeId);
                AbstractGraph.RemoveEdgesFromAndToNode(nodeId);
                AbstractGraph.AddNode(nodeId, nodeInfo);
                foreach (var edge in nodeBackup.Edges)
                {
                    var targetNodeId = edge.TargetNodeId;

                    AddEdge(nodeId, targetNodeId, edge.Cost, edge.Info.InnerLowerLevelPath != null
                        ? new List<Id<AbstractNode>>(edge.Info.InnerLowerLevelPath)
                        : null);

                    edge.Info.InnerLowerLevelPath?.Reverse();

                    AddEdge(targetNodeId, nodeId, edge.Cost, edge.Info.InnerLowerLevelPath);
                }
            }

            for (int i = maxLevel + 1; i <= MaxLevel; i++)
            {
                SetCurrentLevel(i);
                RemoveAbstractNode(nodeId);
            }

            nodeBackups.Remove(nodeId);
        }


        private class NodeBackup
        {
            public int Level { get; }
            public List<AbstractEdge> Edges { get; }

            public NodeBackup(int level, List<AbstractEdge> edges)
            {
                Level = level;
                Edges = edges;
            }
        }

        readonly Dictionary<Id<AbstractNode>, List<NodeBackup>> nodeBackups = new Dictionary<Id<AbstractNode>, List<NodeBackup>>();

        // insert a new node, such as start or target, to the abstract graph and
        // returns the id of the newly created node in the abstract graph
        // x and y are the positions where I want to put the node
        private Id<AbstractNode> InsertNodeIntoHierarchicalMap(Id<ConcreteNode> concreteNodeId, Position pos)
        {
            void SaveBackup(Id<AbstractNode> existingAbstractNodeId, AbstractNodeInfo nodeInfo)
            {
                var nodeBackupList = new List<NodeBackup>();
                for (int level = 1; level <= nodeInfo.Level; level++)
                {
                    SetCurrentLevel(level);
                    nodeBackupList.Add(new NodeBackup(
                        level,
                        GetNodeEdges(concreteNodeId)));
                }

                nodeBackups[existingAbstractNodeId] = nodeBackupList;
            }

            // If the node already existed (for instance, it was the an entrance point already
            // existing in the graph, we need to keep track of the previous status in order
            // to be able to restore it once we delete this STAL
            if (ConcreteNodeIdToAbstractNodeIdMap.ContainsKey(concreteNodeId))
            {
                Id<AbstractNode> existingAbstractNodeId = ConcreteNodeIdToAbstractNodeIdMap[concreteNodeId];
                AbstractNodeInfo nodeInfo = AbstractGraph.GetNodeInfo(existingAbstractNodeId);

                SaveBackup(existingAbstractNodeId, nodeInfo);

                GraphLayers.AddNodeToAllLayers(existingAbstractNodeId, nodeInfo);

                return ConcreteNodeIdToAbstractNodeIdMap[concreteNodeId];
            }

            Cluster cluster = FindClusterForPosition(pos);

            // create global entrance
            var abstractNodeId = Id<AbstractNode>.From(NrNodes);

            EntrancePoint entrance = cluster.AddEntrance(abstractNodeId, new Position(pos.X - cluster.Origin.X, pos.Y - cluster.Origin.Y));
            cluster.UpdatePathsForLocalEntrance(entrance);

            ConcreteNodeIdToAbstractNodeIdMap[concreteNodeId] = abstractNodeId;

            var info = new AbstractNodeInfo(
                abstractNodeId,
                MaxLevel,
                cluster.Id,
                pos,
                concreteNodeId);

            GraphLayers.AddNodeToAllLayers(abstractNodeId, info);

            foreach (var entrancePoint in cluster.EntrancePoints)
            {
                if (cluster.AreConnected(abstractNodeId, entrancePoint.AbstractNodeId))
                {
                    AddEdge(
                        entrancePoint.AbstractNodeId,
                        abstractNodeId,
                        cluster.GetDistance(entrancePoint.AbstractNodeId, abstractNodeId));
                    AddEdge(
                        abstractNodeId,
                        entrancePoint.AbstractNodeId,
                        cluster.GetDistance(abstractNodeId, entrancePoint.AbstractNodeId));
                }

            }

            return abstractNodeId;
        }

        public bool PositionInCurrentCluster(Position position)
		{
			var y = position.Y;
			var x = position.X;
			return y >= currentClusterY0 && y <= currentClusterY1 && x >= currentClusterX0 && x <= currentClusterX1;
		}

		// Define the offset between two clusters in this level (each level doubles the cluster size)
		private int GetOffset(int level)
		{
			return ClusterSize * (1 << (level - 1));
		}

	    public void SetAllMapAsCurrentCluster()
	    {
			currentClusterY0 = 0;
			currentClusterY1 = Height - 1;
			currentClusterX0 = 0;
			currentClusterX1 = Width - 1;
		}
		
		public void SetCurrentClusterByPositionAndLevel(Position pos, int level)
		{
			var offset = GetOffset(level);
			var nodeY = pos.Y;
			var nodeX = pos.X;
			currentClusterY0 = nodeY - (nodeY % offset);
			currentClusterY1 = Math.Min(this.Height - 1, this.currentClusterY0 + offset - 1);
			currentClusterX0 = nodeX - (nodeX % offset);
			currentClusterX1 = Math.Min(this.Width - 1, this.currentClusterX0 + offset - 1);
		}
        
		public bool BelongToSameCluster(Id<AbstractNode> node1Id, Id<AbstractNode> node2Id, int level)
		{
			var node1Pos = AbstractGraph.GetNodeInfo(node1Id).Position;
			var node2Pos = AbstractGraph.GetNodeInfo(node2Id).Position;
			var offset = GetOffset(level);
			var currentRow1 = node1Pos.Y - (node1Pos.Y % offset);
			var currentRow2 = node2Pos.Y - (node2Pos.Y % offset);
			var currentCol1 = node1Pos.X - (node1Pos.X % offset);
			var currentCol2 = node2Pos.X - (node2Pos.X % offset);

			if (currentRow1 != currentRow2)
				return false;

			if (currentCol1 != currentCol2)
				return false;

			return true;
		}
        
        public void CreateHierarchicalEdges()
        {
            // Starting from level 2 denotes a serious mess on design, because lvl 1 is
            // used by the clusters.
            for (var level = 2; level <= MaxLevel; level++)
            {
                SetCurrentLevel(level);

                int n = 1 << (level - 1);
                // Group clusters by their level. Each subsequent level doubles the amount of clusters in each group
                var clusterGroups = Clusters.GroupBy(cl => $"{cl.ClusterX / n}_{cl.ClusterY / n}");

                foreach (var clusterGroup in clusterGroups)
                {
                    var entrancesInClusterGroup = clusterGroup
                        .SelectMany(cl => cl.EntrancePoints)
                        .Where(entrance => AbstractGraph.NodeExists(entrance.AbstractNodeId))
                        .ToList();

                    var firstEntrance = entrancesInClusterGroup.FirstOrDefault();

                    if (firstEntrance == null)
                        continue;

                    var entrancePosition = AbstractGraph.GetNode(firstEntrance.AbstractNodeId).Info.Position;

                    SetCurrentClusterByPositionAndLevel(
                        entrancePosition,
                        level);

                    foreach (var entrance1 in entrancesInClusterGroup)
                        foreach (var entrance2 in entrancesInClusterGroup)
                        {
                            if (entrance1 == entrance2)
                                continue;

                            AddEdgesBetweenAbstractNodes(entrance1.AbstractNodeId, entrance2.AbstractNodeId, level);
                        }
                }
            }
        }

        public void AddEdgesBetweenAbstractNodes(Id<AbstractNode> srcAbstractNodeId, Id<AbstractNode> destAbstractNodeId, int level)
        {
            var search = new SearchService<AbstractNode>();
            var path = search.FindPath(GraphLayers.GetSearchLayer(), srcAbstractNodeId, destAbstractNodeId);
            if (path.PathCost >= 0)
            {
                AddEdge(srcAbstractNodeId, destAbstractNodeId, path.PathCost, new List<Id<AbstractNode>>(path.PathNodes));
	            path.PathNodes.Reverse();
                AddEdge(destAbstractNodeId, srcAbstractNodeId, path.PathCost, path.PathNodes);
            }
        }

        public void AddEdgesToOtherEntrancesInCluster(AbstractNodeInfo abstractNodeInfo, int level)
        {
            SetCurrentLevel(level);
            SetCurrentClusterByPositionAndLevel(abstractNodeInfo.Position, level);
            
            foreach (var cluster in Clusters)
            {
                if (cluster.Origin.X >= currentClusterX0 && cluster.Origin.X <= currentClusterX1 &&
                    cluster.Origin.Y >= currentClusterY0 && cluster.Origin.Y <= currentClusterY1)
                {
                    foreach (var entrance in cluster.EntrancePoints.Where(x => AbstractGraph.NodeExists(x.AbstractNodeId)))
                    {
                        if (abstractNodeInfo.Id == entrance.AbstractNodeId)
                            continue;
                        
                        AddEdgesBetweenAbstractNodes(abstractNodeInfo.Id, entrance.AbstractNodeId, level);
                    }
                }
            }
        }
		
		public void AddHierarchicalEdgesForAbstractNode(Id<AbstractNode> abstractNodeId)
		{
		    GraphLayers.SetLevel(1);
			AbstractNodeInfo abstractNodeInfo = AbstractGraph.GetNodeInfo(abstractNodeId);
			abstractNodeInfo.Level = MaxLevel;
			for (var level = 2; level <= MaxLevel; level++)
			{
				AddEdgesToOtherEntrancesInCluster(abstractNodeInfo, level);
			}
		}

        public void SetCurrentLevel(int level)
        {
            GraphLayers.SetLevel(level);
        }
    }
}
