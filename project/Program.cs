namespace HK._3d.doomRenderer
{
    internal class Program
    {
        static int[,] map = new int[11, 11]; // y then x, because c# does a little bit of trolling

        static (double, double) playerPosition = (2, 2);

        static int fov = 111; // in degrees
        static double rad = Math.PI / 180;
        static double viewdistance = 10.0; // in mapunits(one index in the map array is one mapunit)
        static double playerRotation = 0; // in radians
        static double turnSpeed = (2 * Math.PI) / 360;

        static double raySpeed = 0.2;
        static double oobValue = 0; // returned if a bay would spawn out of bounds

        static double movementSpeed = 0.2; // how many mapunits the player moves per frame

        static int windowHeight = (fov / 16) * 9; // times 2 multiplier;

        static void Main(string[] args)
        {
            // id´s of maptiles:
            // 0 = empty
            // 1 = wall

            // create test map
            createTestMap(1);

            // "game" loop
            Console.CursorVisible = false;
            while (true)
            {
                renderFrame();

                char c = Console.ReadKey(true).KeyChar;

                if (c == 'w' || c == 's')
                {
                    double multiplier = c == 'w' ? 1 : -1;
                    (double, double) playerMovement = rotToVector(playerRotation);

                    if (map[(int)Math.Round(playerPosition.Item1 + playerMovement.Item1 * movementSpeed * multiplier), (int)Math.Round(playerPosition.Item2 + playerMovement.Item2 * movementSpeed * multiplier)] == 0)
                    {
                        playerPosition.Item1 += playerMovement.Item1 * movementSpeed * multiplier;
                        playerPosition.Item2 += playerMovement.Item2 * movementSpeed * multiplier;
                    }
                }
                else if (c == 'd')
                {
                    playerRotation = (playerRotation + turnSpeed) % (Math.PI * 2);
                }
                else if (c == 'a')
                {
                    playerRotation = (playerRotation - turnSpeed + Math.PI * 2) % (Math.PI * 2);
                }
            }
        }

        static void createTestMap(int type)
        {
            if (type == 0)
            {
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
            }
            else if (type == 1)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    if (i == 0 || i == map.GetLength(0) - 1)
                    {
                        for (int j = 0; j < map.GetLength(1); j++)
                        {
                            map[i, j] = 1;
                        }
                    }
                    else if (i >= 4 && i <= map.GetLength(0) - 5)
                    {
                        for (int j = 0; j < map.GetLength(1); j++)
                        {
                            if (j == 0 || (j >= 4  && j <= map.GetLength(1) - 5) || j == map.GetLength(1) - 1)
                            {
                                map[i, j] = 1;
                            }
                        }
                    }
                    else if ((i > 0 && i < 4) || (i < map.GetLength(1)-1 && i > map.GetLength(1) - 5))
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
            }

            printMap();
        }

        // right nopn fisheye approach
        static void renderFrame()
        {
            List<double> rayDistances = new List<double>();

            for (int i = fov / -2; i <= fov / 2; i++)
            {
                double change = i * rad;
                double hypotenuse = castRay(playerPosition, playerRotation + change);
                double angle = 90 * rad - Math.Abs(change);
                double oppositeSide = Math.Sin(angle) * hypotenuse;

                rayDistances.Add(oppositeSide);
            }

            drawWalls(rayDistances);
        }

        // wrong fisheye approach
        static void renderFisheyeFrame()
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
                        walls += "  ";
                    }
                    else
                    {
                        walls += "██";
                    }
                }

                walls += "\n";
            }

            Console.WriteLine(walls);
            Console.WriteLine(playerRotation);
            Console.WriteLine(playerPosition);
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
            return (-Math.Cos(rotation), Math.Sin(rotation)); //first y then x, because c# arrays do be like that
        }

        static void printMap()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j]+" ");
                }

                Console.WriteLine();
            }
            Console.ReadKey(true);
        }
    }
}
