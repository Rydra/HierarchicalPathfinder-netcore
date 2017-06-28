using System;
using System.Collections.Generic;
using HPASharp.Infrastructure;
using Priority_Queue;

namespace HPASharp.Search
{
	/// <summary>
	/// An A* node embeds the status of a processed node, containing information like
	/// the cost it's taken to reach it (Cost So far, G), the expected cost to reach the goal
	/// (The heuristic, H), the parent where this node came from (which will serve later to reconstruct best paths)
	/// the current Status of the node (Open, Closed, Unexplored, see CellStatus documentation for more information) and the F-score
	/// that serves to compare which nodes are the best
	/// </summary>
    public struct SearchNode<TNode>
    {
        public SearchNode(Id<TNode> parent, int g, int h, CellStatus status)
        {
            Parent = parent;
            G = g;
            H = h;
	        F = g + h;
            Status = status;
        }

        public Id<TNode> Parent;
        public CellStatus Status;
        public int H;
        public int G;
	    public int F;
    }

	/// <summary>
	/// The cell status indicates whether a node has not yet been processed 
	/// but it lies in the open queue (Open) or the node has been processed (Closed)
	/// </summary>
    public enum CellStatus
    {
        Open,
        Closed
    }
	
	public class Path<TNode>
	{
		public int PathCost { get; }
		public List<Id<TNode>> PathNodes { get; }

		public Path(List<Id<TNode>> pathNodes, int pathCost)
		{
			PathCost = pathCost;
			PathNodes = pathNodes;
		}
	}

	public class NodeLookup<TNode>
	{
		private readonly Dictionary<Id<TNode>, SearchNode<TNode>> _searchNodes;

		public NodeLookup()
		{
			_searchNodes = new Dictionary<Id<TNode>, SearchNode<TNode>>();
		}

		public void SetNodeValue(Id<TNode> nodeId, SearchNode<TNode> value)
		{
			_searchNodes[nodeId] = value;
		}

		public bool NodeIsVisited(Id<TNode> nodeId)
		{
		    return _searchNodes.ContainsKey(nodeId);
		}

		public SearchNode<TNode> GetNodeValue(Id<TNode> nodeId)
		{
			return _searchNodes[nodeId];
		}
	}

    public class SearchService<TNode> : ISearchService<TNode>
	{
		public Path<TNode> FindPath(IGraph<TNode> graph, Id<TNode> startNodeId, Id<TNode> targetNodeId)
		{
		    bool IsGoal(Id<TNode> nodeId) => nodeId == targetNodeId;
		    int CalculateHeuristic(Id<TNode> nodeId) => graph.GetHeuristic(nodeId, targetNodeId);

		    var estimatedCost = CalculateHeuristic(startNodeId);

		    var startNode = new SearchNode<TNode>(startNodeId, 0, estimatedCost, CellStatus.Open);
		    var openQueue = new SimplePriorityQueue<Id<TNode>>();
		    openQueue.Enqueue(startNodeId, startNode.F);

		    var nodeLookup = new NodeLookup<TNode>();
		    nodeLookup.SetNodeValue(startNodeId, startNode);

		    bool CanExpand() => openQueue.Count != 0;

		    Id<TNode> Expand()
		    {
		        var nodeId = openQueue.Dequeue();
		        var node = nodeLookup.GetNodeValue(nodeId);

		        ProcessNeighbours(nodeId, node);

		        nodeLookup.SetNodeValue(nodeId, new SearchNode<TNode>(node.Parent, node.G, node.H, CellStatus.Closed));
		        return nodeId;
		    }

		    void ProcessNeighbours(Id<TNode> nodeId, SearchNode<TNode> node)
		    {
		        var connections = graph.GetConnections(nodeId);
		        foreach (var connection in connections)
		        {
		            var gCost = node.G + connection.Cost;
		            var neighbour = connection.Target;
		            if (nodeLookup.NodeIsVisited(neighbour))
		            {
		                var targetAStarNode = nodeLookup.GetNodeValue(neighbour);
		                // If we already processed the neighbour in the past or we already found in the past
		                // a better path to reach this node that the current one, just skip it, else create
		                // and replace a new PathNode
		                if (targetAStarNode.Status == CellStatus.Closed || gCost >= targetAStarNode.G)
		                    continue;

		                targetAStarNode = new SearchNode<TNode>(nodeId, gCost, targetAStarNode.H, CellStatus.Open);
		                openQueue.UpdatePriority(neighbour, targetAStarNode.F);
		                nodeLookup.SetNodeValue(neighbour, targetAStarNode);
		            }
		            else
		            {
		                var newHeuristic = CalculateHeuristic(neighbour);
		                var newAStarNode = new SearchNode<TNode>(nodeId, gCost, newHeuristic, CellStatus.Open);
		                openQueue.Enqueue(neighbour, newAStarNode.F);
		                nodeLookup.SetNodeValue(neighbour, newAStarNode);
		            }
		        }
		    }

		    Path<TNode> ReconstructPathFrom(Id<TNode> destination)
		    {
		        var pathNodes = new List<Id<TNode>>();
		        var pathCost = nodeLookup.GetNodeValue(destination).F;
		        var currentNode = destination;
		        while (nodeLookup.GetNodeValue(currentNode).Parent != currentNode)
		        {
		            pathNodes.Add(currentNode);
		            currentNode = nodeLookup.GetNodeValue(currentNode).Parent;
		        }

		        pathNodes.Add(currentNode);
		        pathNodes.Reverse();

		        return new Path<TNode>(pathNodes, pathCost);
		    }

            while (CanExpand())
            {
	            var nodeId = Expand();
	            if (IsGoal(nodeId))
				{
					return ReconstructPathFrom(nodeId);
				}
            }
			
	        return new Path<TNode>(new List<Id<TNode>>(), -1);
        }
	}
}
