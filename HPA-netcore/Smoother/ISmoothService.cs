using System.Collections.Generic;

namespace HPASharp.Smoother
{
    public interface ISmoothService
    {
        List<IPathNode> SmoothPath(ConcreteMap concreteMap, List<IPathNode> path);
    }
}