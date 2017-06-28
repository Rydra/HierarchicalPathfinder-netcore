using HPASharp.Infrastructure;

namespace HPASharp
{
    public struct Connection<TNode>
    {
        public Id<TNode> Target;
        public int Cost;

        public Connection(Id<TNode> target, int cost)
        {
            Target = target;
            Cost = cost;
        }
    }
}