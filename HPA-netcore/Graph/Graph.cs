using System.Collections.Generic;
using HPASharp.Infrastructure;

namespace HPASharp.Graph
{
	public interface INode<TId, TInfo, TEdge>
	{
		Id<TId> NodeId { get; set; }
		TInfo Info { get; set; }
		IDictionary<Id<TId>, TEdge> Edges { get; set; }
        void RemoveEdge(Id<TId> targetNodeId);
		void AddEdge(TEdge targetNodeId);
	}

	public interface IEdge<TNode, TEdgeInfo>
	{
		Id<TNode> TargetNodeId { get; set; }
		TEdgeInfo Info { get; set; }
        int Cost { get; set; }
	}
    
	/// <summary>
	/// A graph is a set of nodes connected with edges. Each node and edge can hold
	/// a certain amount of information, which is expressed in the templated parameters
	/// NODEINFO and EDGEINFO
	/// </summary>
	public abstract class Graph<TNode, TNodeInfo, TEdge, TEdgeInfo> : IGraph<TNode>
		where TNode : INode<TNode, TNodeInfo, TEdge>
		where TEdge : IEdge<TNode, TEdgeInfo>
	{
		// We store the nodes in a list because the main operations we use
		// in this list are additions, random accesses and very few removals (only when
		// adding or removing nodes to perform specific searches).
		// This list is implicitly indexed by the nodeId, which makes removing a random
		// Node in the list quite of a mess. We could use a dictionary to ease removals,
		// but lists and arrays are faster for random accesses, and we need performance.
        public Dictionary<Id<TNode>, TNode> Nodes { get; set; }
	    public int NrNodes => Nodes.Count;

        public delegate TEdge EdgeCreator(Id<TNode> targetNodeId, int cost, TEdgeInfo info);
	    public delegate TNode NodeCreator(Id<TNode> nodeId, TNodeInfo info);

        private readonly NodeCreator _nodeCreator;
		private readonly EdgeCreator _edgeCreator;

		protected Graph(NodeCreator nodeCreator, EdgeCreator edgeCreator)
        {
            Nodes = new Dictionary<Id<TNode>, TNode>();
	        _nodeCreator = nodeCreator;
	        _edgeCreator = edgeCreator;
        } 

		/// <summary>
		/// Adds or updates a node with the provided info. A node is updated
		/// only if the nodeId provided previously existed.
		/// </summary>
        public void AddNode(Id<TNode> nodeId, TNodeInfo info)
        {
            if (!Nodes.ContainsKey(nodeId))
            {
                Nodes[nodeId] = _nodeCreator(nodeId, info);
            }
        }

	    public abstract int GetHeuristic(Id<TNode> startNodeId, Id<TNode> targetNodeId);

	    public IEnumerable<Connection<TNode>> GetConnections(Id<TNode> nodeId)
	    {
	        var result = new List<Connection<TNode>>();
	        TNode node = GetNode(nodeId);

	        foreach (var edge in node.Edges.Values)
	        {
	            if (IsValid(edge))
	            {
	                result.Add(new Connection<TNode>(edge.TargetNodeId, edge.Cost));
                }
	                
	        }

	        return result;
        }

	    protected abstract bool IsValid(TEdge edge);

        #region AbstractGraph updating

        public void AddEdge(Id<TNode> sourceNodeId, Id<TNode> targetNodeId, int cost, TEdgeInfo info)
        {
            Nodes[sourceNodeId].AddEdge(_edgeCreator(targetNodeId, cost, info));
        }
        
        public void RemoveEdgesFromAndToNode(Id<TNode> nodeId)
        {
            foreach (var targetNodeId in Nodes[nodeId].Edges.Keys)
            {
                Nodes[targetNodeId].RemoveEdge(nodeId);
            }

            Nodes[nodeId].Edges.Clear();
        }

        public void Remove(Id<TNode> nodeId)
        {
            Nodes.Remove(nodeId);
        }

        #endregion

        public TNode GetNode(Id<TNode> nodeId)
        {
            return Nodes[nodeId];
        }

	    public bool NodeExists(Id<TNode> nodeId)
	    {
	        return Nodes.ContainsKey(nodeId);
	    }

        public TNodeInfo GetNodeInfo(Id<TNode> nodeId)
        {
            return Nodes[nodeId].Info;
        }
        
        public IDictionary<Id<TNode>, TEdge> GetEdges(Id<TNode> nodeId)
        {
            return Nodes[nodeId].Edges;
        }
    }
}