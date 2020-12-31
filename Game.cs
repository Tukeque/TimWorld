using System;
using System.Collections.Generic;

namespace TimWorld
{
    class Game
    {
        #region Variables
        public static byte[,,] map;
        public static int MapWidth = 4096;
        public static int MapHeight = 16;
        public static int MapLength = 4096;
        public static ProgressBar mapBar;

        public static List<Items.Item> items = new List<Items.Item>();
        public static List<Items.Stack> stacks = new List<Items.Stack>();
        public static List<Items.FarmLand> farmLands = new List<Items.FarmLand>();
        public static float farmLandGrowChance = 25;
        public static int farmLandGrowLimit = 8;

        public static int cameraX = MapWidth / 2;
        public static int cameraZ = MapLength / 2;
        public static int cameraY = 6;

        public static List<Tim> tims = new List<Tim>();
        public static int TimCount = 1024;
        public static int TimThinkingNeurons = 1024;
        public static int TimWorkingNeurons = 320;
        public static float mutationChance = 50;
        public static float mutationAmount = 0.05f;
        public static ProgressBar timBar;
        #endregion

        static void Main(string[] args)
        {
            Console.Title = "TimWorld";
            Setup();
            Console.ReadKey();

            try
            {
                while (true) // while (run) ?
                {
                    Update();
                }
            }
            finally
            {
                Exit();
            }
        }

        static void Setup()
        {
            Screen.Init();
            Screen.Print("Hello, World!", 0, 0);

            // TODO make input thread and get seed like that (Consol)
            Extra.InitRandom(2359234);

            GenerateMap();
            // Console.ReadKey(); // TODO replace with input class readkey
            GenerateTims();
            Console.Beep();
            Console.ReadKey(); // TODO replace with input class readkey
            Exit();            // TODO remove after game loop is done and program can exit safely
        }
        static void Update()
        {
            Screen.UpdateSize(true);    // ???
            // Screen.DisplayScreen();

            // TODO game loop
            // game loop

            // TODO crop tick

            // display map (and handle map movement)
            // TODO

            // handle tims (WIP)
            for (int i = 0; i < TimCount; i++)
            {
                tims[i].CalculateState();
            }

            for (int i = 0; i < TimCount; i++)
            {
                tims[i].UpdateState();
            }

            Screen.DisplayScreen();
        }
        static void Exit()
        {
            Screen.Clear();
            Screen.Print("The Program has finished. Press any key to exit...", 0, 0);
            Screen.DisplayScreen();
            Console.ReadKey(); // TODO replace with input class readkey
            Environment.Exit(0x00);
        }

        static void GenerateMap()
        {
            mapBar = new ProgressBar("Generating Map... ", MapWidth, 20, 0, 1);
            Screen.DisplayScreen();
            map = new byte[MapWidth, MapHeight, MapLength];

            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    for (int z = 0; z < MapLength; z++)
                    {
                        // FUTURE TODO make map generate using perlin noise
                        #region BasicMapGeneration
                        if (y < 4) // stone // 1.5% metal 98.5% stone
                        {
                            if (Extra.Chance(1.5f))
                            {
                                map[x, y, z] = (byte)Blocks.Block.Metal;
                            }
                            else
                            {
                                map[x, y, z] = (byte)Blocks.Block.Stone;
                            }
                        }
                        else if (y == 4) // dirt(surface) // 2.5% pond 2.5% farmland 95% dirt
                        {
                            if (map[x, y, z] == 0) // if its empty
                            {
                                if (Extra.Chance(5f))
                                {
                                    if (Extra.Chance(50f))
                                    {
                                        // pond center
                                        map[x, y, z] = (byte)Blocks.Block.Water;                    // pond outsides
                                        map[x, y, SpecialMath.Modulus(z + 1, MapLength)] = (byte)Blocks.Block.Water;
                                        map[x, y, SpecialMath.Modulus(z - 1, MapLength)] = (byte)Blocks.Block.Water;
                                        map[SpecialMath.Modulus(x + 1, MapWidth),  y, z] = (byte)Blocks.Block.Water;
                                        map[SpecialMath.Modulus(x - 1, MapWidth),  y, z] = (byte)Blocks.Block.Water;
                                    }
                                    else
                                    {
                                        map[x, y, z] = (byte)Blocks.Block.FarmLand; // TODO add to farmLands
                                    }
                                }
                                else
                                {
                                    map[x, y, z] = (byte)Blocks.Block.Dirt;
                                }
                            }
                        }
                        else if (y == 5) // trees // 2.5% trees 97.5% air
                        {
                            if (Extra.Chance(2.5f))
                            {
                                // tree D:
                                map[x, y, z]     = (byte)Blocks.Block.Tree; // trunk 1
                                map[x, y + 1, z] = (byte)Blocks.Block.Tree; // trunk 2

                                // leaf algo. :D !!!
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 2; j++)
                                    {
                                        for (int k = 0; k < 3; k++)
                                        {
                                            map[SpecialMath.Modulus(x + i, MapWidth), y + j, SpecialMath.Modulus(z + k, MapLength)] = (byte)Blocks.Block.Tree;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                mapBar.QuickUpdate();
                Screen.DisplayScreen();
            }
        }

        static void GenerateTims()
        {
            timBar = new ProgressBar("Generating Tims...", TimCount, 20, 0, 2);
            Screen.DisplayScreen();
            tims = new List<Tim>();

            for (int i = 0; i < TimCount; i++)
            {
                tims.Add(new Tim(Extra.random.Next(0, MapWidth), 
                    Extra.random.Next(0, MapHeight), Extra.random.Next(0, MapLength), 
                    Extra.RandomDirection(), Extra.RandomSubDirection(),
                    new NN(TimThinkingNeurons, TimWorkingNeurons), 
                    20.0f, 20.0f, 
                    new Items.Item(), new Items.Item()));
                timBar.QuickUpdate(); Screen.DisplayScreen();
            }
        }
    }
}
