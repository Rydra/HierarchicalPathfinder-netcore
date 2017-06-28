using HPASharp.Infrastructure;

namespace HPASharp.Search
{
    public interface ISearchService<TNode>
    {
        Path<TNode> FindPath(IGraph<TNode> graph, Id<TNode> startNodeId, Id<TNode> targetNodeId);
    }
}