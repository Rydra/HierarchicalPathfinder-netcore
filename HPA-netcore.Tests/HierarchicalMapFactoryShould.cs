using System.Linq;
using HPASharp;
using HPASharp.Factories;
using Moq;
using Xunit;

namespace HPA_netcore.Tests
{
    public class HierarchicalMapFactoryShould
    {
        [Fact]
        public void GenerateGraph()
        {
            IPassability passability = new ExamplePassability();
            var _concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var _abstractMap = abstractMapFactory.CreateHierarchicalMap(_concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var numberOfEdges = _abstractMap.GraphByLevel[2].Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }

        [Fact]
        public void GenerateGraphAndAddExtraPoints()
        {
            IPassability passability = new ExamplePassability();
            var _concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var _abstractMap = abstractMapFactory.CreateHierarchicalMap(_concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var id1 = abstractMapFactory.InsertAbstractNode(_abstractMap, new Position(18, 0));
            var id2 = abstractMapFactory.InsertAbstractNode(_abstractMap, new Position(20, 0));

            var numberOfEdges = _abstractMap.GraphByLevel[2].Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(56, numberOfEdges);

            abstractMapFactory.RemoveAbstractNode(_abstractMap, id1);
            abstractMapFactory.RemoveAbstractNode(_abstractMap, id2);

            numberOfEdges = _abstractMap.GraphByLevel[2].Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }
    }
}
