namespace HK._3d.doomRenderer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // id´s of maptiles:
            // 0 = empty
            // 1 = wall
            int[,] map = new int[11, 11];

            double playerX = 6;
            double playerY = 6;

            int fov = 60; // in degrees
            double viewdistance = 10.0; // in mapunits(one index in the map array is one mapunit)

            // create test map
            for (int i = 0; i < map.GetLength(0); i++)
            {
                if (i == 0 || i == map.GetLength(0) - 1)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        map[i, j] = 1;
                    }
                }
                else
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        if (j == 0 || j == map.GetLength(1) - 1)
                        {
                            map[i, j] = 1;
                        }
                    }
                }
            }

            // visualize the testmap
            printMap(map);
        }

        static void printMap(int[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
