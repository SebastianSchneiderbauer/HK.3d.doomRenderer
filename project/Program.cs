namespace HK._3d.doomRenderer
{
    internal class Program
    {
        static int[,] map = new int[11, 11];

        static (double, double) playerPosition = (6, 6);

        static int fov = 60; // in degrees
        static double viewdistance = 10.0; // in mapunits(one index in the map array is one mapunit)
        static double playerRotation = 0; // in radians
        static double turnSpeed = (2 * Math.PI) / 360; // fuck deltatime

        static double raySpeed = 0.2;
        static double raySpread = 0.1;
        static double oobValue = 0; // returned if a bay would spawn out of bounds

        static void Main(string[] args)
        {
            // id´s of maptiles:
            // 0 = empty
            // 1 = wall

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

            // "game" loop
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

                double raydistance = castRay(playerPosition, playerRotation);

                for (int i = 0; i < raydistance * 10; i++)
                {
                    Console.Write("##");
                }
                Console.WriteLine();
            }
        }

        static double castRay((double,double)origin, double direction)
        {
            (double, double) ray = (Math.Sin(direction), Math.Cos(direction));

            (double, double) rayposition = origin;
            double raydistance = oobValue;

            //simulate the ray
            for (double i = 0; i < viewdistance; i += raySpeed)
            {
                if (ray.Item1 > 0)
                {
                    rayposition.Item1 += ray.Item1 * raySpeed;
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
                catch (IndexOutOfRangeException e)
                {
                    break;
                }

                raydistance = i;

            }

            return raydistance;
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
