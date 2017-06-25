using System.Collections.Generic;
using HPASharp;
using HPASharp.Graph;
using HPASharp.Infrastructure;

namespace HPA_netcore.Graph
{
    public class GraphLayers
    {
        private Dictionary<int, AbstractGraph> _graphLevels;
        private int _currentLevel;
        private int _maxLevels;

        public AbstractGraph AbstractGraph => _graphLevels[_currentLevel];

        public GraphLayers(int levels, HierarchicalMap map)
        {
            _graphLevels = new Dictionary<int, AbstractGraph>();
            _maxLevels = levels;
            for (int level = 1; level <= levels; level++)
            {
                _graphLevels[level] = new AbstractGraph(map);
            }

            _currentLevel = 1;
        }

        public void AddNodeToAllLayers(Id<AbstractNode> abstractNodeId, AbstractNodeInfo info)
        {
            for (int level = 1; level <= _maxLevels; level++)
            {
                _graphLevels[level].AddNode(abstractNodeId, info);
            }
        }

        public void SetLevel(int level)
        {
            _currentLevel = level;
        }

        public AbstractGraph GetSearchLayer()
        {
            return _graphLevels[_currentLevel - 1];
        }
    }
}
