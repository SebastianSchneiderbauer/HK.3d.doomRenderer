using System.Drawing;

namespace HK._3d.doomRenderer
{
    internal class Program
    {
        static int[,] map = new int[11, 11]; // y then x, because c# does a little bit of trolling

        static (double, double) playerPosition = (2, 2);

        static int fov = 100; // in degrees
        static double rad = Math.PI / 180;
        static double viewdistance = 100.0; // in mapunits(one index in the map array is one mapunit)
        static double playerRotation = 0; // in radians
        static double turnSpeed = (2 * Math.PI) / 360;

        static double raySpeed = 0.2;
        static double oobValue = 0; // returned if a bay would spawn out of bounds

        static double movementSpeed = 0.2; // how many mapunits the player moves per frame

        static int windowHeight = (fov / 16) * 9; // times 2 multiplier;

        static void ImportMapFromPng(string path)
        {
            using (var bmp = new Bitmap(path))
            {
                int height = bmp.Height;
                int width = bmp.Width;
                map = new int[height, width];

                bool playerSet = false;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);

                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                        {
                            map[y, x] = 1; // Wall
                        }
                        else if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                        {
                            map[y, x] = 0; // Empty
                        }
                        else if (pixel.R == 255 && pixel.G == 0 && pixel.B == 0)
                        {
                            map[y, x] = 0; // Player spawn is empty
                            if (!playerSet)
                            {
                                playerPosition = (y, x);
                                playerSet = true;
                            }
                        }
                        else
                        {
                            // Treat any other color as empty
                            map[y, x] = 0;
                        }
                    }
                }

                if (!playerSet)
                    throw new Exception("No player spawn (pure red pixel) found in map image.");
            }
        }

        static void Main(string[] args)
        {
            // id´s of maptiles:
            // 0 = empty
            // 1 = wall

            // create map
            //createTestMap(1);
            ImportMapFromPng("../../../maps/map.png");

            // "game" loop
            Console.CursorVisible = false;
            while (true)
            {
                calculateFrame();

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
                else if (c == 'm')
                {
                    printMap();
                }
                else if (c == 'q')
                {
                    Console.Clear();
                    Console.CursorVisible = true;
                    Environment.Exit(0);
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
                            if (j == 0 || (j >= 4 && j <= map.GetLength(1) - 5) || j == map.GetLength(1) - 1)
                            {
                                map[i, j] = 1;
                            }
                        }
                    }
                    else if ((i > 0 && i < 4) || (i < map.GetLength(1) - 1 && i > map.GetLength(1) - 5))
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
        }

        // right non fisheye approach
        static void calculateFrame()
        {
            List<double> rayDistances = new List<double>();

            for (int i = fov / -2; i <= fov / 2; i++)
            {
                double change = i * rad;
                double hypotenuse = castRay(playerPosition, playerRotation + change);
                double correctedDistance = hypotenuse * Math.Cos(change);
                rayDistances.Add(correctedDistance);
            }

            drawWalls(rayDistances);
        }

        static void drawWalls(List<double> rayDistances)
        {
            Console.SetCursorPosition(0, 0);
            string frame = "";

            frame += "┌";

            for (int x = 0; x < rayDistances.Count+1; x++)
            {
                frame += "──";
            }

            frame += "┐\n";

            for (int y = 0; y < windowHeight; y++)
            {
                frame += "│ ";

                for (int x = 0; x < rayDistances.Count; x++)
                {
                    // the only "vibe coded" part. I understand it though
                    int wallHeight = (int)(windowHeight / Math.Max(0.01, rayDistances[x]));
                    int ceiling = (windowHeight - wallHeight) / 2;//you can use this if you want to limit the height of walls
                    int floor = ceiling + wallHeight;
                    double max = rayDistances.Max();

                    if (y >= ceiling && y <= floor)
                    {
                        if ((rayDistances[x] / max) > 0.75)
                        {
                            frame += "░░";
                        }
                        else if ((rayDistances[x] / max) > 0.5)
                        {
                            frame += "▒▒";
                        }
                        else if ((rayDistances[x] / max) > 0.25)
                        {
                            frame += "▓▓";
                        }
                        else
                        {
                            frame += "██";
                        }
                    }
                    else
                    {
                        frame += "  ";
                    }
                }
                frame += " │\n";
            }
            
            frame += "├";

            for (int x = 0; x < rayDistances.Count+1; x++)
            {
                if (x == 11)
                {
                    frame += "┬─";
                }
                else
                {
                    frame += "──";
                }
            }

            frame += "┘";

            Console.Write(frame+ "\n│ W:   forewards       │\n│ S:   backwards       │\n│ A/D: turn left/right │\n│ M:   toggle map      │\n│ Q:   quit            │\n└──────────────────────┘");
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
            (int y, int x) = ((int)Math.Round(playerPosition.Item1), (int)Math.Round(playerPosition.Item2));

            Console.Clear();

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (i == y && j == x)
                    {
                        Console.Write("[]");
                    }
                    else if (map[i, j] == 0)
                    {
                        Console.Write("  ");
                    }
                    else if (map[i, j] == 1)
                    {
                        Console.Write("██");
                    }
                }

                Console.WriteLine();
            }

            Console.Write("press M to toggle map");

            while (true)
            {
                char c = Console.ReadKey(true).KeyChar;
                if (c == 'm')
                {
                    Console.Clear();
                    break;
                }
            }
        }
    }
}
