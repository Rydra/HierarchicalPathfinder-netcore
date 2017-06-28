using System;
using HPASharp;
using HPASharp.Infrastructure;

namespace HPA_netcore.Search
{
    public delegate int Heuristic(Position start, Position target);

    public class Heuristics
    {
        // Suited for Hexagonal maps
        // See P.Yap: Grid-based Path-Finding (LNAI 2338 pp.44-55)
        public static int VancouverDistance(Position start, Position target)
        {
            var startX = start.X;
            var targetX = target.X;
            var startY = start.Y;
            var targetY = target.Y;
            var diffX = Math.Abs(targetX - startX);
            var diffY = Math.Abs(targetY - startY);

            var correction = 0;
            if (diffX % 2 != 0)
            {
                if (targetY < startY)
                    correction = targetX % 2;
                else if (targetY > startY)
                    correction = startX % 2;
            }
            
            var dist = Math.Max(0, diffY - diffX / 2 - correction) + diffX;
            return dist * 1;
        }

        public static int DiagonalDistance(Position start, Position target)
        {
            var startX = start.X;
            var targetX = target.X;
            var startY = start.Y;
            var targetY = target.Y;
            var diffX = Math.Abs(targetX - startX);
            var diffY = Math.Abs(targetY - startY);

            int maxDiff;
            int minDiff;
            if (diffX > diffY)
            {
                maxDiff = diffX;
                minDiff = diffY;
            }
            else
            {
                maxDiff = diffY;
                minDiff = diffX;
            }

            return (minDiff * Constants.COST_ONE * 34) / 24 + (maxDiff - minDiff) * Constants.COST_ONE;
        }

        public static int ManhattanDistance(Position start, Position target)
        {
            var startX = start.X;
            var targetX = target.X;
            var startY = start.Y;
            var targetY = target.Y;
            var diffX = Math.Abs(targetX - startX);
            var diffY = Math.Abs(targetY - startY);

            return (diffX + diffY) * Constants.COST_ONE;
        }

        public static int OctileUnicostDistance(Position start, Position target)
        {
            var startX = start.X;
            var targetX = target.X;
            var startY = start.Y;
            var targetY = target.Y;
            var diffX = Math.Abs(targetX - startX);
            var diffY = Math.Abs(targetY - startY);

            return Math.Max(diffX, diffY) * Constants.COST_ONE;
        }
    }
}
