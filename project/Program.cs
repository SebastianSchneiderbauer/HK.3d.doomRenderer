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

            (double, double) playerPosition = (6, 6);

            int fov = 60; // in degrees
            double viewdistance = 10.0; // in mapunits(one index in the map array is one mapunit)
            double playerRotation = 0; // in radians
            double turnSpeed = (2*Math.PI)/360; // fuck deltatime

            double raySpeed = 0.2;
            double raySpread = 0.1;
            double oobValue = 0; // returned if a bay would spawn out of bounds

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

            while (true)
            {
                char c = Console.ReadKey(true).KeyChar;

                if (c == 'k')
                {
                    playerRotation = (playerRotation + turnSpeed) % (Math.PI * 2);
                }
                else if (c == 'j')
                {
                    playerRotation = (playerRotation - turnSpeed + Math.PI * 2) % (Math.PI * 2);
                }

                (double, double) ray = (Math.Sin(playerRotation), Math.Cos(playerRotation));

                (double, double) rayposition = playerPosition;
                double raydistance = 0;

                //simulate the ray
                for (double i = 0; i < viewdistance; i += raySpeed)
                {
                    if (ray.Item1 > 0)
                    {
                        rayposition.Item1 += ray.Item1*raySpeed;
                    }

                    if (ray.Item1 < 0)
                    {
                        rayposition.Item1 -= ray.Item1 * raySpeed;
                    }

                    if (ray.Item2 > 0)
                    {
                        rayposition.Item2 += ray.Item2 * raySpeed;
                    }

                    if (ray.Item2 < 0)
                    {
                        rayposition.Item2 -= ray.Item2 * raySpeed;
                    }


                    try
                    {
                        if (map[(int)Math.Round(rayposition.Item1), (int)Math.Round(rayposition.Item2)] != 0)
                        {
                            break;
                        }
                    }
                    catch(IndexOutOfRangeException e)
                    {
                        raydistance = 0;
                        break;
                    }

                    raydistance = i;

                }

                //Console.SetCursorPosition(0, 0);

                for (int i = 0; i < raydistance * 10; i++)
                {
                    Console.Write("##");
                }
                Console.WriteLine();
            }
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
