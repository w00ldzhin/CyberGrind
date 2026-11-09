using System;
using System.Collections.Generic;

namespace igraUP.Models
{
    public class Map
    {
        public int[][] map;
        public double SpawnX { get; private set; }
        public double SpawnY { get; private set; }
        public double ExitX { get; private set; }
        public double ExitY { get; private set; }
        public List<(int X, int Y)> EnemySpawnPoints { get; private set; } = new List<(int, int)>();

        private Random rand = new Random();

        public Map()
        {
            Generate(120, 120);
        }
        public void Generate(int width, int height)
        {
            map = new int[height][];
            for (int y = 0; y < height; y++)
            {
                map[y] = new int[width];
                for (int x = 0; x < width; x++) map[y][x] = 1;
            }
            int cx = 5, cy = height / 2;
            map[cy][cx] = 0;
            SpawnX = cx + 0.5;
            SpawnY = cy + 0.5;

            int prevY = cy;
            int steps = 350; 

            for (int i = 0; i < steps; i++)
            {
                cx++;
                if (cx >= width - 5) break;

                if (rand.Next(0, 100) > 40)
                {
                    cy += rand.Next(-3, 4);
                }

                cy = Math.Clamp(cy, 3, height - 4);

                int startY = Math.Min(prevY, cy);
                int endY = Math.Max(prevY, cy);
                for (int y = startY; y <= endY; y++)
                {
                    map[y][cx] = 0;
                }
                prevY = cy;

                if (rand.Next(0, 100) > 85) 
                {
                    int by = cy;
                    int bDir = rand.Next(0, 2) == 0 ? -1 : 1;
                    int bLen = rand.Next(3, 6); 
                    bool valid = true;

                    for (int j = 0; j < bLen; j++)
                    {
                        by += bDir;
                        if (by < 2 || by > height - 3) { valid = false; break; }
                        map[by][cx] = 0; 
                    }
                    if (valid)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                if (cx + dx > 0 && cx + dx < width - 1 && by + dy > 0 && by + dy < height - 1)
                                    map[by + dy][cx + dx] = 0;

                        EnemySpawnPoints.Add((cx, by)); 
                    }
                }
            }
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (cx + dx > 0 && cx + dx < width - 1 && cy + dy > 0 && cy + dy < height - 1)
                        map[cy + dy][cx + dx] = 0;

            ExitX = cx + 0.5;
            ExitY = cy + 0.5;
            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    if (map[y][x] == 0)
                    {
                        double distToStart = Math.Sqrt(Math.Pow(x - SpawnX, 2) + Math.Pow(y - SpawnY, 2));
                        if (distToStart < 10.0) continue;

                        if (rand.Next(0, 100) > 95) 
                        {
                            EnemySpawnPoints.Add((x, y));
                        }
                    }
                }
            }
        }
    }
}



// переделка на основе декомпила пост войда, ну она конечно не прям такая крутая получилась,
// но с большим количеством врагов прикольно получается
// в целом убирает узкие коридоры, делает все более открытым, генерация слева направо с рандомным шансом спавна комнаты в 40%
// комнаты заранее не сгенерированы, хотя стоило бы, мб потом, начальные конечные комнаты для спавна и портала
// проще для тестов


//    public class Map
//    {
//        public int[][] map;
//        public double SpawnX { get; private set; }
//        public double SpawnY { get; private set; }
//        public double ExitX { get; private set; }
//        public double ExitY { get; private set; }
//        public List<(int X, int Y)> EnemySpawnPoints { get; private set; } = new List<(int, int)>();

//        private Random rand = new Random();

//        public Map()
//        {
//            Generate(100, 100); 
//        }

//        public void Generate(int width, int height)
//        {)
//            map = new int[height][];
//            for (int y = 0; y < height; y++)
//            {
//                map[y] = new int[width];
//                for (int x = 0; x < width; x++) map[y][x] = 1;
//            }

//            int startX = 5;
//            int startY = height / 2;
//            CarveRoom(startX, startY, 5, 5);
//            SpawnX = startX;
//            SpawnY = startY;

//            int pathPosX = startX;
//            int pathPosY = startY;
//            int goalX = width - 15; 
//            int roomChance = 40; 

//            while (pathPosX < goalX)
//            {
//                
//                CarveRoom(pathPosX, pathPosY, 3, 3);

//                if (rand.Next(0, 100) > 60) 
//                {
//                    int newY = Math.Clamp(pathPosY + rand.Next(-4, 5), 5, height - 5);

//                    int startLine = Math.Min(pathPosY, newY);
//                    int endLine = Math.Max(pathPosY, newY);
//                    for (int y = startLine; y <= endLine; y++)
//                    {
//                        map[y][pathPosX] = 0;
//                        map[y][pathPosX + 1] = 0;
//                        map[y][pathPosX - 1] = 0;
//                    }
//                    pathPosY = newY;
//                }

//              
//                if (rand.Next(0, 100) < roomChance)
//                {
//                    int roomW = rand.Next(5, 10);
//                    int roomH = rand.Next(5, 10);
//                    int roomX = pathPosX + rand.Next(2, 5);
//                    int roomY = pathPosY + rand.Next(-6, 7);

//                    if (roomX + roomW < width - 5 && roomY + roomH < height - 5 && roomY > 5)
//                    {
//                        CarveRoom(roomX, roomY, roomW, roomH);

//                        for (int x = pathPosX; x <= roomX; x++)
//                        {
//                            map[pathPosY][x] = 0;
//                        }

//                        EnemySpawnPoints.Add((roomX, roomY));
//                    }
//                }

//              
//                pathPosX += 2;
//            }
//         
//            CarveRoom(pathPosX, pathPosY, 6, 6);
//            ExitX = pathPosX;
//            ExitY = pathPosY;

//            for (int y = 3; y < height - 3; y++)
//            {
//                for (int x = 3; x < width - 3; x++)
//                {
//                    if (map[y][x] == 0)
//                    {
//                        double distToStart = Math.Sqrt(Math.Pow(x - SpawnX, 2) + Math.Pow(y - SpawnY, 2));
//                        if (distToStart < 15.0) continue; 

//                        double distToExit = Math.Sqrt(Math.Pow(x - ExitX, 2) + Math.Pow(y - ExitY, 2));
//                        if (distToExit < 5.0) continue; 

//                        if (rand.Next(0, 100) > 92) 
//                        {
//                            EnemySpawnPoints.Add((x, y));
//                        }
//                    }
//                }
//            }
//        }

//        private void CarveRoom(int cx, int cy, int w, int h)
//        {
//            for (int y = cy - h / 2; y <= cy + h / 2; y++)
//            {
//                for (int x = cx - w / 2; x <= cx + w / 2; x++)
//                {
//                    if (x > 0 && x < map[0].Length - 1 && y > 0 && y < map.Length - 1)
//                        map[y][x] = 0;
//                }
//            }
//        }
//    }
//}