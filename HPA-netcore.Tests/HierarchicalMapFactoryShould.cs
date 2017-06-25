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
            var concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var abstractMap = abstractMapFactory.CreateHierarchicalMap(concreteMap, 10, 2, EntranceStyle.EndEntrance);

            abstractMap.SetCurrentLevel(2);
            var numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }

        [Fact]
        public void GenerateGraphAndAddExtraPoints()
        {
            IPassability passability = new ExamplePassability();
            var concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var abstractMap = abstractMapFactory.CreateHierarchicalMap(concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var id1 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(18, 0));
            var id2 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(20, 0));

            abstractMap.SetCurrentLevel(2);
            var numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(56, numberOfEdges);

            abstractMapFactory.RemoveAbstractNode(abstractMap, id1);
            abstractMapFactory.RemoveAbstractNode(abstractMap, id2);

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }

        [Fact]
        public void GenerateGraphAndAddPointsOverEntrances()
        {
            IPassability passability = new ExamplePassability();
            var concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var abstractMap = abstractMapFactory.CreateHierarchicalMap(concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            var id1 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(0, 19));
            var id2 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(20, 0));

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(54, numberOfEdges);

            abstractMapFactory.RemoveAbstractNode(abstractMap, id1);
            abstractMapFactory.RemoveAbstractNode(abstractMap, id2);

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }

        [Fact]
        public void GenerateGraphAndAddPointsOverEntrances2()
        {
            IPassability passability = new ExamplePassability();
            var concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var abstractMap = abstractMapFactory.CreateHierarchicalMap(concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            var id1 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(0, 19));
            var id2 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(39, 19));

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);

            abstractMapFactory.RemoveAbstractNode(abstractMap, id1);
            abstractMapFactory.RemoveAbstractNode(abstractMap, id2);

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }

        [Fact]
        public void GenerateGraphAndAddPointsOverEntrancesJustFirstLevel()
        {
            IPassability passability = new ExamplePassability();
            var concreteMap = ConcreteMap.Create(40, 40, passability);
            var abstractMapFactory = new HierarchicalMapFactory();
            var abstractMap = abstractMapFactory.CreateHierarchicalMap(concreteMap, 10, 2, EntranceStyle.EndEntrance);

            var numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            var id1 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(1, 9));
            var id2 = abstractMapFactory.InsertAbstractNode(abstractMap, new Position(39, 19));

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(48, numberOfEdges);

            abstractMapFactory.RemoveAbstractNode(abstractMap, id1);
            abstractMapFactory.RemoveAbstractNode(abstractMap, id2);

            abstractMap.SetCurrentLevel(2);
            numberOfEdges = abstractMap.AbstractGraph.Nodes.Values.SelectMany(x => x.Edges).Count();
            Assert.Equal(46, numberOfEdges);
        }
    }
}
