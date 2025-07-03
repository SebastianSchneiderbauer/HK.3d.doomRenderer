namespace HK._3d.doomRenderer
{
    internal class Program
    {
        static int[,] map = new int[21, 11]; // y then x, because c# does a little bit of trolling

        static (double, double) playerPosition = (5, 5);

        static int fov = 90; // in degrees
        static double rad = Math.PI / 180;
        static double fovRadians = rad * fov; // in radians
        static double viewdistance = 10.0; // in mapunits(one index in the map array is one mapunit)
        static double playerRotation = 0; // in radians
        static double turnSpeed = (2 * Math.PI) / 360;

        static double raySpeed = 0.2;
        static double oobValue = 0; // returned if a bay would spawn out of bounds
        static double raySpread = 0.1;

        static int windowHeight = (fov / 16) * 9; // times 2 multiplier;

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
            Console.CursorVisible = false;
            while (true)
            {
                rederFisheyeFrame();

                char c = Console.ReadKey(true).KeyChar;

                if (c == 'k')
                {
                    playerRotation = (playerRotation + turnSpeed) % (Math.PI * 2);
                }
                else if (c == 'j')
                {
                    playerRotation = (playerRotation - turnSpeed + Math.PI * 2) % (Math.PI * 2);
                }
            }
        }

        //wrong fisheye approach
        static void rederFisheyeFrame()
        {
            List<double> rayDistances = new List<double>();

            for (int i = fov / -2; i <= fov / 2; i++)
            {
                double change = i * rad;
                rayDistances.Add(castRay(playerPosition, playerRotation + change));
            }

            drawWalls(rayDistances);
        }

        static void drawWalls(List<double> rayDistances)
        {
            Console.SetCursorPosition(0, 0);

            string walls = "";

            for (int i = windowHeight; i > 0; i--)
            {
                for (int j = 0; j < rayDistances.Count; j++)
                {
                    double distance = rayDistances[j] * (windowHeight / 10);
                    if (distance >= i)
                    {
                        walls += "██";
                    }
                    else
                    {
                        walls += "  ";
                    }
                }

                walls += "\n";
            }

            Console.WriteLine(walls);
            Console.WriteLine(playerRotation);
            Console.WriteLine(rotToVector(playerRotation));
        }

        static double castRay((double, double) origin, double direction)
        {
            (double, double) ray = rotToVector(direction);

            (double, double) rayposition = origin;
            double raydistance = oobValue;

            //simulate the ray
            for (double i = 0; i < viewdistance; i += raySpeed)
            {
                rayposition.Item1 += ray.Item1 * raySpeed;
                rayposition.Item2 += ray.Item2 * raySpeed;

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

        static (double, double) rotToVector(double rotation)
        {
            return (-Math.Cos(rotation), Math.Sin(rotation)); //up is negative
            return (Math.Sin(rotation), Math.Cos(rotation)); //up is negative
        }
    }
}
