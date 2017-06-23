using HPASharp;
using Moq;
using Xunit;

namespace HPA_netcore.Tests
{
    public class ConcreteMapShould
    {
        [Fact]
        public void BeInstantiated()
        {
            var height = 4;
            var width = 4;
            var passability = new Mock<IPassability>();
            int movementCost = 100;
            passability.Setup(x => x.CanEnter(It.IsAny<Position>(), out movementCost)).Returns(true);
            var concreteMap = ConcreteMap.Create(width, height, passability.Object);

            Assert.Equal(4, concreteMap.Height);
            Assert.Equal(4, concreteMap.Width);
            Assert.Equal(16, concreteMap.NrNodes);
        }
    }
}
