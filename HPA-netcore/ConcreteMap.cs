using System;
using System.Collections.Generic;
using HPASharp.Factories;
using HPASharp.Graph;
using HPASharp.Infrastructure;

namespace HPASharp
{

    public enum TileType
    {
        Hex,
        /** Octiles with cost 1 to adjacent and sqrt(2) to diagonal. */
        Octile,
        /** Octiles with uniform cost 1 to adjacent and diagonal. */
        OctileUnicost,
        Tile
    }

    public class ConcreteMap
    {
		public IPassability Passability { get; set; }

	    public TileType TileType { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public int MaxEdges { get; set; }

        public ConcreteGraph Graph { get; set; }

        public ConcreteMap(TileType tileType, int width, int height, IPassability passability)
        {
            Passability = passability;
			TileType = tileType;
			MaxEdges = Helpers.GetMaxEdges(tileType);
			Height = height;
			Width = width;
			Graph = GraphFactory.CreateGraph(width, height, Passability);
		}

        // Create a new concreteMap as a copy of another concreteMap (just copying obstacles)
        public ConcreteMap Slice(int horizOrigin, int vertOrigin, int width, int height, IPassability passability)
        {
            var slicedConcreteMap = new ConcreteMap(TileType, width, height, passability);

	        foreach (var slicedMapNode in slicedConcreteMap.Graph.Nodes.Values)
	        {
		        var globalConcreteNode =
			        Graph.GetNode(GetNodeIdFromPos(horizOrigin + slicedMapNode.Info.Position.X,
				        vertOrigin + slicedMapNode.Info.Position.Y));
				slicedMapNode.Info.IsObstacle = globalConcreteNode.Info.IsObstacle;
				slicedMapNode.Info.Cost = globalConcreteNode.Info.Cost;
			}

            return slicedConcreteMap;
		}

	    public int NrNodes => Width * Height;

        public Id<ConcreteNode> GetNodeIdFromPos(int x, int y)
	    {
		    return Id<ConcreteNode>.From(y * Width + x);
	    }

        /// <summary>
        /// Tells whether we can move from p1 to p2 in line. Bear in mind
        /// this function does not consider intermediate points (it is
        /// assumed you can jump between intermediate points)
        /// </summary>
        public bool CanJump(Position p1, Position p2)
        {
            //if (TileType != TileType.Octile && this.TileType != TileType.OctileUnicost)
            //    return true;
            if (Helpers.AreAligned(p1, p2))
                return true;
            
            ConcreteNodeInfo nodeInfo12 = Graph.GetNode(GetNodeIdFromPos(p2.X, p1.Y)).Info;
            ConcreteNodeInfo nodeInfo21 = Graph.GetNode(GetNodeIdFromPos(p1.X, p2.Y)).Info;
            return !(nodeInfo12.IsObstacle && nodeInfo21.IsObstacle);
        }
		
        #region Printing

        private List<char> GetCharVector()
        {
            var result = new List<char>();
            var numberNodes = NrNodes;
            for (var i = 0; i < numberNodes; ++i)
                result.Add(Graph.GetNodeInfo(Id<ConcreteNode>.From(i)).IsObstacle ? '@' : '.');

            return result;
        }

        public void PrintFormatted()
        {
            PrintFormatted(GetCharVector());
        }

        private void PrintFormatted(List<char> chars)
        {
            for (var y = 0; y < Height; ++y)
            {
                for (var x = 0; x < Width; ++x)
                {
                    var nodeId = this.GetNodeIdFromPos(x, y);
                    Console.Write(chars[nodeId.IdValue]);
                }

                Console.WriteLine();
            }
        }

        public void PrintFormatted(List<int> path)
        {
            var chars = GetCharVector();
            if (path.Count > 0)
            {
                foreach (var i in path)
                {
                    chars[i] = 'x';
                }

                chars[path[0]] = 'T';
                chars[path[path.Count - 1]] = 'S';
            }

            PrintFormatted(chars);
        }

        #endregion

        public static ConcreteMap Create(int width, int height, IPassability passability, TileType tilingType = TileType.Octile)
        {
            return new ConcreteMap(tilingType, width, height, passability);
        }
    }
}
