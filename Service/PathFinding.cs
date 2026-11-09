using System;
using System.Collections.Generic;
using System.Text;

namespace igraUP.Service
{
    public class Pathfinding
    {
        public static (int X, int Y) GetNextStep(int startX, int startY, int targetX, int targetY, int[][] map)
        {
            if (startX == targetX && startY == targetY) return (startX, startY);

            int width = map[0].Length;
            int height = map.Length;

            Queue<(int X, int Y)> queue = new Queue<(int X, int Y)>();
            bool[,] visited = new bool[height, width];
            (int X, int Y)[,] parent = new (int, int)[height, width];

            queue.Enqueue((startX, startY));
            visited[startY, startX] = true;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            bool found = false;

            while (queue.Count > 0 && !found)
            {
                var current = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = current.X + dx[i];
                    int ny = current.Y + dy[i];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                        !visited[ny, nx] && map[ny][nx] == 0)
                    {
                        visited[ny, nx] = true;
                        parent[ny, nx] = current;
                        queue.Enqueue((nx, ny));

                        if (nx == targetX && ny == targetY)
                        {
                            found = true;
                            break;
                        }
                    }
                }
            }

            if (!found) return (-1, -1);

            int currX = targetX;
            int currY = targetY;
            (int X, int Y) previous = (targetX, targetY);

            while (currX != startX || currY != startY)
            {
                var par = parent[currY, currX];
                previous = (currX, currY);
                currX = par.X;
                currY = par.Y;
            }

            return previous;
        }
    }
}