using System;
using System.Collections.Generic;
using HPASharp.Graph;
using HPASharp.Infrastructure;

namespace HPASharp.Factories
{
    public class HierarchicalMapFactory
    {
        private const int MAX_ENTRANCE_WIDTH = 6;

	    private HierarchicalMap _hierarchicalMap;
	    private ConcreteMap _concreteMap;
	    private EntranceStyle _entranceStyle;
	    private int _clusterSize;
	    private int _maxLevel;
        
		public HierarchicalMap CreateHierarchicalMap(ConcreteMap concreteMap, int clusterSize, int maxLevel, EntranceStyle style)
        {
            _clusterSize = clusterSize;
            _entranceStyle = style;
            _maxLevel = maxLevel;
            _concreteMap = concreteMap;
            _hierarchicalMap = new HierarchicalMap(concreteMap, clusterSize, maxLevel);

            List<Entrance> entrances;
            List<Cluster> clusters; 
            CreateEntrancesAndClusters(out entrances, out clusters);
            _hierarchicalMap.Clusters = clusters;
			
            CreateAbstractNodes(entrances);
            CreateEdges(entrances, clusters);

	        return _hierarchicalMap;
        }
        
		private void CreateEdges(List<Entrance> entrances, List<Cluster> clusters)
		{
			foreach (var entrance in entrances)
			{
				CreateInterClusterEdges(entrance, _hierarchicalMap.Type);
			}

            foreach (var cluster in clusters)
			{
				cluster.CreateIntraClusterEdges();
				CreateIntraClusterEdges(cluster);
			}

			_hierarchicalMap.CreateHierarchicalEdges();
		}

		private void CreateInterClusterEdges(Entrance entrance, AbstractType type)
		{
		    int GetCost(Orientation orientation)
		    {
		        int cost = Constants.COST_ONE;
		        switch (type)
		        {
		            case AbstractType.ABSTRACT_TILE:
		            case AbstractType.ABSTRACT_OCTILE_UNICOST:
		                // Inter-edges: cost 1
		                cost = Constants.COST_ONE;
		                break;
		            case AbstractType.ABSTRACT_OCTILE:
		            {
		                int unitCost;
		                switch (orientation)
		                {
		                    case Orientation.Horizontal:
		                    case Orientation.Vertical:
		                        unitCost = Constants.COST_ONE;
		                        break;
		                    case Orientation.Hdiag2:
		                    case Orientation.Hdiag1:
		                    case Orientation.Vdiag1:
		                    case Orientation.Vdiag2:
		                        unitCost = (Constants.COST_ONE * 34) / 24;
		                        break;
		                    default:
		                        unitCost = -1;
		                        break;
		                }

		                cost = unitCost;
		            }
		                break;
		        }

		        return cost;
		    }

			int level = entrance.GetEntranceLevel(_clusterSize, _maxLevel);

			Id<AbstractNode> srcAbstractNodeId = _hierarchicalMap.ConcreteNodeIdToAbstractNodeIdMap[entrance.SrcNode.NodeId];
			Id<AbstractNode> destAbstractNodeId = _hierarchicalMap.ConcreteNodeIdToAbstractNodeIdMap[entrance.DestNode.NodeId];

		    int costToReach = GetCost(entrance.Orientation);
            
		    for (int i = 1; i <= level; i++)
		    {
                _hierarchicalMap.SetCurrentLevel(i);
		        _hierarchicalMap.AbstractGraph.AddEdge(srcAbstractNodeId, destAbstractNodeId, costToReach, new AbstractEdgeInfo());
		        _hierarchicalMap.AbstractGraph.AddEdge(destAbstractNodeId, srcAbstractNodeId, costToReach, new AbstractEdgeInfo());
		    }
		}
        
		private void CreateIntraClusterEdges(Cluster cluster)
		{
		    _hierarchicalMap.SetCurrentLevel(1);

            foreach (var point1 in cluster.EntrancePoints)
			foreach (var point2 in cluster.EntrancePoints)
			{
				if (point1 != point2 && cluster.AreConnected(point1.AbstractNodeId, point2.AbstractNodeId))
				{
					var abstractEdgeInfo = new AbstractEdgeInfo();
					_hierarchicalMap.AbstractGraph.AddEdge(
						point1.AbstractNodeId,
						point2.AbstractNodeId,
						cluster.GetDistance(point1.AbstractNodeId, point2.AbstractNodeId),
						abstractEdgeInfo);
				}
			}
		}

		#region Entrances and clusters
		private void CreateEntrancesAndClusters(out List<Entrance> entrances, out List<Cluster> clusters)
        {
            var clusterId = 0;
            var entranceId = 0;
            
            entrances = new List<Entrance>();
			clusters = new List<Cluster>();
            
			for (int top = 0, clusterY = 0; top < _concreteMap.Height; top += _clusterSize, clusterY++)
            for (int left = 0, clusterX = 0; left < _concreteMap.Width; left += _clusterSize, clusterX++)
            {
                var width = Math.Min(_clusterSize, _concreteMap.Width - left);
                var height = Math.Min(_clusterSize, _concreteMap.Height - top);
                var cluster = new Cluster(_concreteMap, Id<Cluster>.From(clusterId), clusterX, clusterY, new Position(left, top), new Size(width, height));
				clusters.Add(cluster);

                clusterId++;

                var clusterAbove = top > 0 ? GetCluster(clusters, clusterX, clusterY - 1) : null;
                var clusterOnLeft = left > 0 ? GetCluster(clusters, clusterX - 1, clusterY) : null;

                entrances.AddRange(CreateInterClusterEntrances(cluster, clusterAbove, clusterOnLeft, ref entranceId));
            }
        }

        private List<Entrance> CreateInterClusterEntrances(Cluster cluster, Cluster clusterAbove, Cluster clusterOnLeft, ref int entranceId)
        {
            var entrances = new List<Entrance>();
            int top = cluster.Origin.Y;
            int left = cluster.Origin.X;
            
            if (clusterAbove != null)
            {
                var entrancesOnTop = CreateEntrancesOnTop(
                    left,
                    left + cluster.Size.Width - 1,
                    top - 1,
                    clusterAbove,
                    cluster,
                    ref entranceId);

                entrances.AddRange(entrancesOnTop);
            }

            if (clusterOnLeft != null)
            {
                var entrancesOnLeft = CreateEntrancesOnLeft(
                    top,
                    top + cluster.Size.Height - 1,
                    left - 1,
                    clusterOnLeft,
                    cluster,
                    ref entranceId);

                entrances.AddRange(entrancesOnLeft);
            }

            return entrances;
        }

        private List<Entrance> CreateEntrancesOnLeft(
            int rowStart,
            int rowEnd,
            int column,
			Cluster clusterOnLeft,
			Cluster cluster,
            ref int currentEntranceId)
        {
            Tuple<ConcreteNode, ConcreteNode> GetNodesForRow(int row) => Tuple.Create(GetNode(column, row), GetNode(column + 1, row));

            return CreateEntrancesAlongEdge(rowStart, rowEnd, clusterOnLeft, cluster, ref currentEntranceId, GetNodesForRow, Orientation.Horizontal);
        }

        private List<Entrance> CreateEntrancesOnTop(
            int colStart,
            int colEnd,
            int row,
            Cluster clusterOnTop,
			Cluster cluster,
            ref int currentEntranceId)
        {
            Tuple<ConcreteNode, ConcreteNode> GetNodesForColumn(int column) => Tuple.Create(GetNode(column, row), GetNode(column, row + 1));

            return CreateEntrancesAlongEdge(colStart, colEnd, clusterOnTop, cluster, ref currentEntranceId, GetNodesForColumn, Orientation.Vertical);
        }

        private List<Entrance> CreateEntrancesAlongEdge(
            int startPoint,
            int endPoint,
            Cluster precedentCluster,
			Cluster currentCluster,
            ref int currentEntranceId,
            Func<int, Tuple<ConcreteNode, ConcreteNode>> getNodesInEdge,
            Orientation orientation)
        {
            List<Entrance> entrances = new List<Entrance>();

            for (var entranceStart = startPoint; entranceStart <= endPoint; entranceStart++)
            {
                var size = GetEntranceSize(entranceStart, endPoint, getNodesInEdge);

                var entranceEnd = entranceStart + size - 1;
                if (size == 0)
                    continue;

                if (_entranceStyle == EntranceStyle.EndEntrance && size > MAX_ENTRANCE_WIDTH)
                {
                    var nodes = getNodesInEdge(entranceStart);
                    var srcNode = nodes.Item1;
                    var destNode = nodes.Item2;

                    var entrance1 = new Entrance(Id<Entrance>.From(currentEntranceId), precedentCluster, currentCluster, srcNode, destNode, orientation);

                    currentEntranceId++;
					
                    nodes = getNodesInEdge(entranceEnd);
                    srcNode = nodes.Item1;
                    destNode = nodes.Item2;

                    var entrance2 = new Entrance(Id<Entrance>.From(currentEntranceId), precedentCluster, currentCluster, srcNode, destNode, orientation);

                    currentEntranceId++;

                    entrances.Add(entrance1);
                    entrances.Add(entrance2);
                }
                else
                {
                    var nodes = getNodesInEdge((entranceEnd + entranceStart) / 2);
                    var srcNode = nodes.Item1;
                    var destNode = nodes.Item2;

                    var entrance = new Entrance(Id<Entrance>.From(currentEntranceId), precedentCluster, currentCluster, srcNode, destNode, orientation);

                    currentEntranceId++;
                    entrances.Add(entrance);
                }

                entranceStart = entranceEnd;
            }

            return entrances;
        }

        private int GetEntranceSize(int entranceStart, int end, Func<int, Tuple<ConcreteNode, ConcreteNode>> getNodesInEdge)
        {
            var size = 0;
            while (entranceStart + size <= end && !EntranceIsBlocked(entranceStart + size, getNodesInEdge))
            {
                size++;
            }

            return size;
        }

        private ConcreteNode GetNode(int left, int top)
        {
            return _concreteMap.Graph.GetNode(_concreteMap.GetNodeIdFromPos(left, top));
        }

        private bool EntranceIsBlocked(int entrancePoint, Func<int, Tuple<ConcreteNode, ConcreteNode>> getNodesInEdge)
        {
			var nodes = getNodesInEdge(entrancePoint);
			return nodes.Item1.Info.IsObstacle || nodes.Item2.Info.IsObstacle;
        }
		
		private Cluster GetCluster(List<Cluster> clusters, int left, int top)
		{
			var clustersW = _hierarchicalMap.Width / _clusterSize;
			if (_hierarchicalMap.Width % _clusterSize > 0)
				clustersW++;

			return clusters[top * clustersW + left];
		}
		#endregion
        
		#region Generate abstract nodes
		private void CreateAbstractNodes(List<Entrance> entrancesList)
		{
			foreach (var abstractNode in GenerateAbstractNodes(entrancesList))
			{
			    for (int level = 1; level <= abstractNode.Level; level++)
			    {
                    _hierarchicalMap.SetCurrentLevel(level);
			        _hierarchicalMap.ConcreteNodeIdToAbstractNodeIdMap[abstractNode.ConcreteNodeId] = abstractNode.Id;
			        _hierarchicalMap.AbstractGraph.AddNode(abstractNode.Id, abstractNode);
			    }
			}
		}

		private IEnumerable<AbstractNodeInfo> GenerateAbstractNodes(List<Entrance> entrances)
		{
			var abstractNodeId = 0;
			var abstractNodesDict = new Dictionary<Id<ConcreteNode>, AbstractNodeInfo>();
			foreach (var entrance in entrances)
			{
				var level = entrance.GetEntranceLevel(_clusterSize, _maxLevel);

				CreateOrUpdateAbstractNodeFromConcreteNode(entrance.SrcNode, entrance.Cluster1, ref abstractNodeId, level, abstractNodesDict);
				CreateOrUpdateAbstractNodeFromConcreteNode(entrance.DestNode, entrance.Cluster2, ref abstractNodeId, level, abstractNodesDict);
			}

			return abstractNodesDict.Values;
		}

		private static void CreateOrUpdateAbstractNodeFromConcreteNode(
			ConcreteNode srcNode,
			Cluster cluster,
			ref int abstractNodeId,
			int level,
			Dictionary<Id<ConcreteNode>, AbstractNodeInfo> abstractNodes)
		{
			AbstractNodeInfo abstractNodeInfo;
			if (!abstractNodes.TryGetValue(srcNode.NodeId, out abstractNodeInfo))
			{
				cluster.AddEntrance(
					Id<AbstractNode>.From(abstractNodeId),
					new Position(srcNode.Info.Position.X - cluster.Origin.X, srcNode.Info.Position.Y - cluster.Origin.Y));

				abstractNodeInfo = new AbstractNodeInfo(
					Id<AbstractNode>.From(abstractNodeId),
					level,
					cluster.Id,
					new Position(srcNode.Info.Position.X, srcNode.Info.Position.Y),
					srcNode.NodeId);
				abstractNodes[srcNode.NodeId] = abstractNodeInfo;

				abstractNodeId++;
			}
			else if (level > abstractNodeInfo.Level)
			{
				abstractNodeInfo.Level = level;
			}
		}
		#endregion

	}
}
