using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TimWorld
{
    class Blocks
    {
        public enum Block
        {
            Air      = 0,
            Stone    = 1,
            Dirt     = 2,
            Tree     = 3,
            Metal    = 4,
            Table    = 5,
            Stack    = 6,
            Water    = 7,
            FarmLand = 8,
            Planks   = 9
        }
    }

    class Items
    {
        public enum ItemEnum
        {
            Nothing      = 0,
            // all blocks
            Stone        = 1,
            Dirt         = 2,
            Tree         = 3,
            Metal        = 4,
            Table        = 5,
            Stack        = 6,
            Water        = 7,
            FarmLand     = 8,
            Planks       = 9,
            // non-block items
            WoodPickaxe  = 10,
            FarmFood     = 11,
            MetalPickaxe = 12,
            WoodWeapon   = 13,
            MetalWeapon  = 14
        }

        public class Item
        {
            public ItemEnum item;
            public int count;

            public int x, y, z;

            public Item(ItemEnum Item, int Count, int X, int Y, int Z)
            {
                item = Item; count = Count;
                x = X; y = Y; z = Z;
            }

            public Item(ItemEnum Item, int Count)
            {
                item = Item; count = Count;
                x = 0; y = 0; z = 0;
            }

            public Item()
            {
                item = ItemEnum.Nothing; count = 0;
                x = 0; y = 0; z = 0;
            }
        }

        public class Stack
        {
            public List<Item> items = new List<Item>();
            public int limit = 4;
            public int x, y, z;
            
            public Stack(List<Item> Items, int X, int Y, int Z)
            {
                items = Items;
                x = X; y = Y; z = Z;
            }

            public void Insert(Item item)
            {
                items.Add(item);
            }

            public Item Grab()
            {
                Item item = items[^1];
                items.RemoveAt(items.Count - 1);
                return item;
            }

            public static Stack Find(Stack[] stacks, int X, int Y, int Z)
            {
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (stacks[i].x == X && stacks[i].y == Y && stacks[i].z == Z)
                    {
                        return stacks[i];
                    }
                }

                // error! // TODO Consol error
                return new Stack(new List<Item>(), -1, -1, -1);
            }

            public static Stack Find(List<Stack> stacks, int X, int Y, int Z)
            {
                return Find(stacks.ToArray(), X, Y, Z);
            }

            public static int FindIndex(Stack[] stacks, int X, int Y, int Z)
            {
                for (int i = 0; i < stacks.Length; i++)
                {
                    if (stacks[i].x == X && stacks[i].y == Y && stacks[i].z == Z)
                    {
                        return i;
                    }
                }

                // error! // TODO Consol error
                return -1;
            }

            public static int FindIndex(List<Stack> stacks, int X, int Y, int Z)
            {
                return FindIndex(stacks.ToArray(), X, Y, Z);
            }
        }

        public class FarmLand
        {
            public int grow;
            public bool grown;
            public int x, y, z;

            public FarmLand(int X, int Y, int Z)
            {
                x = X; y = Y; z = Z;
                grow = 0; grown = false;
            }

            public void Update(int growChance)
            {
                if (!grown)
                {
                    if (Extra.Chance(growChance))
                    {
                        grow++;

                        if (grow == Game.farmLandGrowLimit)
                        {
                            grown = true; grow = 0;
                        }
                    }
                }
            }

            public void Reset()
            {
                grown = false;
                grow = 0;
            }

            public static FarmLand Find(FarmLand[] farmLands, int X, int Y, int Z)
            {
                for (int i = 0; i < farmLands.Length; i++)
                {
                    if (farmLands[i].x == X && farmLands[i].y == Y && farmLands[i].z == Z)
                    {
                        return farmLands[i];
                    }
                }

                // error! // TODO Consol error
                return new FarmLand(-1, -1, -1);
            }

            public static FarmLand Find(List<FarmLand> farmLands, int X, int Y, int Z)
            {
                return Find(farmLands.ToArray(), X, Y, Z);
            }

            public static int FindIndex(FarmLand[] farmLands, int X, int Y, int Z)
            {
                for (int i = 0; i < farmLands.Length; i++)
                {
                    if (farmLands[i].x == X && farmLands[i].y == Y && farmLands[i].z == Z)
                    {
                        return i;
                    }
                }

                // error! // TODO Consol error
                return -1;
            }

            public static int FindIndex(List<FarmLand> farmLands, int X, int Y, int Z)
            {
                return FindIndex(farmLands.ToArray(), X, Y, Z);
            }
        }

        class Crafting // useless for now // TODO make work and good
        {
            public static Item Recipe(Item result)
            {
                switch ((byte)result.item)
                {
                    case 9: // Planks
                        return new Item(ItemEnum.Tree, 1);

                    case 10: // WoodPickaxe
                        return new Item(ItemEnum.Planks, 2);

                    case 12: // MetalPickaxe
                        return new Item(ItemEnum.Metal, 2);

                    case 13: // WoodWeapon
                        return new Item(ItemEnum.Planks, 1);

                    case 14: // MetalWeapon
                        return new Item(ItemEnum.Metal, 1);
                }

                return new Item();
            }
        }
    }

    class Extra
    {
        public static Random random;

        public static void InitRandom(int seed)
        {
            random = new Random(seed);
        }

        public static float NextFloat()
        {
            double a = random.NextDouble();
            return (float)(a * 2) - 1;
        }

        public static Tim.DIR RandomDirection()
        {
            int x = random.Next(0, 4);

            Tim.DIR dir = Tim.DIR.NORTH;

            switch (x)
            {
                case 1:
                    dir = Tim.DIR.WEST;
                    break;

                case 2:
                    dir = Tim.DIR.EAST;
                    break;

                case 3:
                    dir = Tim.DIR.NORTH;
                    break;

                case 4:
                    dir = Tim.DIR.SOUTH;
                    break;
            }

            return dir;
        }

        public static Tim.SUBDIR RandomSubDirection()
        {
            int x = random.Next(0, 3);

            Tim.SUBDIR subdir = Tim.SUBDIR.STRAIGHT;

            switch(x)
            {
                case 0:
                    subdir = Tim.SUBDIR.STRAIGHT;
                    break;

                case 1:
                    subdir = Tim.SUBDIR.UP;
                    break;

                case 2:
                    subdir = Tim.SUBDIR.DOWN;
                    break;
            }

            return subdir;
        }

        public static Tim.DIR DirectionCycle(Tim.DIR x, bool LR)
        {
            Tim.DIR newX = Tim.DIR.NORTH;

            switch (x)
            {
                case Tim.DIR.NORTH:
                    if (LR) newX = Tim.DIR.WEST;
                    else    newX = Tim.DIR.EAST;
                    break;

                case Tim.DIR.SOUTH:
                    if (LR) newX = Tim.DIR.EAST;
                    else    newX = Tim.DIR.WEST;
                    break;

                case Tim.DIR.EAST:
                    if (LR) newX = Tim.DIR.NORTH;
                    else    newX = Tim.DIR.SOUTH;
                    break;

                case Tim.DIR.WEST:
                    if (LR) newX = Tim.DIR.SOUTH;
                    else    newX = Tim.DIR.NORTH;
                    break;
            }

            return newX;
        }

        public static Tim.SUBDIR SubDirectionCycle(Tim.SUBDIR x, bool UD)
        {
            Tim.SUBDIR newX = Tim.SUBDIR.STRAIGHT;

            switch (x)
            {
                case Tim.SUBDIR.STRAIGHT:
                    if (UD) newX = Tim.SUBDIR.UP;
                    else    newX = Tim.SUBDIR.DOWN;
                    break;

                case Tim.SUBDIR.UP:
                    if (UD) newX = Tim.SUBDIR.UP;
                    else    newX = Tim.SUBDIR.STRAIGHT;
                    break;

                case Tim.SUBDIR.DOWN:
                    if (UD) newX = Tim.SUBDIR.STRAIGHT;
                    else    newX = Tim.SUBDIR.DOWN;
                    break;
            }

            return newX;
        }

        public static int[] DirectionToCoordinate(Tim.DIR Dir, Tim.SUBDIR Subdir)
        {
            // get coordinate offset based on a direction and subdirection
            int[] coord = new int[] { 0, 0, 0 };

            switch (Dir)
            {
                case Tim.DIR.NORTH:
                    coord[2] = 1;
                    break;

                case Tim.DIR.SOUTH:
                    coord[2] = -1;
                    break;

                case Tim.DIR.EAST:
                    coord[0] = 1;
                    break;

                case Tim.DIR.WEST:
                    coord[0] = -1;
                    break;
            }

            switch (Subdir)
            {
                // Tim.SUBDIR.STRAIGHT is passed because it just leaves coord unnafected.

                case Tim.SUBDIR.UP:
                    coord = new int[] { 0, 1, 0 };
                    break;

                case Tim.SUBDIR.DOWN:
                    coord = new int[] { 0, -1, 0 };
                    break;
            }

            return coord;
        }

        public static bool Chance(float chance)
        {
            float a = random.Next(0, 101);
            a = (float)random.NextDouble() * 100;

            if (chance > a) return true;
            else if (chance == a) return true ? NextFloat() > 0 : false;

            return false;
        }
    }

    class ProgressBar
    {
        int max;
        int current;
        int old;
        int segments;
        int x, y;
        string segmentString;
        string fillerString;
        string title;
        Stopwatch sw;

        public void QuickUpdate()
        {
            sw.Stop();

            int msLeft = (int)((max - current) * sw.ElapsedMilliseconds);

            old = current;
            current++;

            if (old % (max / segments) > current % (max / segments))
            {
                // add a segment
                segmentString += "─";
                Screen.Print(segmentString + fillerString + " ", x, y);

                fillerString = "";
                for (int i = 0; i < segments - segmentString.Length; i++) fillerString += " ";

                int newX = x;
                int newY = y;
                for (int i = 0; i < segments + 1; i++)
                {
                    int newOldX = newX;
                    newX = (newX + 1) % Screen.Width;
                    if (newOldX > newX) newY = (newY + 1) % Screen.Height;
                }

                Screen.Print(MSToFormattedTime(msLeft), newX, newY);
            }

            sw.Restart();
        }

        static string MSToFormattedTime(int millisecs)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(millisecs);
            // Make sure you use the appropriate decimal separator
            if (millisecs >= 3600000) return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
            else return string.Format("{0:D2}:{1:D2}",
                t.Minutes,
                t.Seconds);
        }

        public ProgressBar(string Title, int Max, int Segments, int X, int Y)
        {
            title = Title + " ";

            max = Max;
            current = 0; old = 0;
            segments = Segments;
            x = X; y = Y;
            Screen.Print(title, x, y);

            for (int i = 0; i < title.Length; i++)
            {
                int oldX = x;
                x = (x + 1) % Screen.Width;
                if (oldX > x) y = (y + 1) % Screen.Height;
            }

            segmentString = "";
            fillerString = "";
            for (int i = 0; i < segments - segmentString.Length; i++) fillerString += " ";

            #region StarterUpdate
            Screen.Print(segmentString + fillerString + " ", x, y);

            int newX = x;
            int newY = y;
            for (int i = 0; i < segments + 1; i++)
            {
                int newOldX = newX;
                newX = (newX + 1) % Screen.Width;
                if (newOldX > newX) newY = (newY + 1) % Screen.Height;
            }

            Screen.Print(MSToFormattedTime(60000), newX, newY);
            #endregion

            sw = new Stopwatch();
            sw.Start();
        }
    }

    class SpecialMath
    {
        public static float ByteToFloat(byte by)
        {
            float sign = (by & 0x80 >> 7) == 1 ? 1 : -1;
            float a = (by & 0x40 >> 6) * 0.5f;
            float b = (by & 0x20 >> 5) * 0.25f;
            float c = (by & 0x10 >> 4) * 0.125f;
            float d = (by & 0x08 >> 3) * 0.0625f;
            float e = (by & 0x04 >> 2) * 0.03125f;
            float f = (by & 0x02 >> 1) * 0.015625f;
            float g = (by & 0x01) * 0.0078125f;

            float total = (a + b + c + d + e + f + g) * sign;
            return total;
        }

        // 0 = neg; 1 = pos

        public static byte FloatToByte(float fl)
        {
            byte by = 0x00;

            if (fl >= 0) { by |= 0x80; } else { fl *= -1; }
            if (fl >= 0.5f) { by |= 0x40; fl -= 0.5f; }
            if (fl >= 0.25f) { by |= 0x20; fl -= 0.25f; }
            if (fl >= 0.125f) { by |= 0x10; fl -= 0.125f; }
            if (fl >= 0.0625f) { by |= 0x08; fl -= 0.0625f; }
            if (fl >= 0.03125f) { by |= 0x04; fl -= 0.03125f; }
            if (fl >= 0.015625f) { by |= 0x02; fl -= 0.015625f; }
            if (fl >= 0.0078125f) { by |= 0x01; fl -= 0.0078125f; }

            return by;
        }

        public static float Sigmoid(float x)
        {
            return 1 / (1 + MathF.Exp(x));
        }

        public static int Modulus(int x, int y)
        {
            int r = x % y;
            return r < 0 ? r + y : r;
        }

        public static bool[] NibbleToBin(byte x)
        {
            bool[] final = new bool[4] { false, false, false, false };

            if (x >= 8) { final[3] = true; x -= 8; }
            if (x >= 4) { final[2] = true; x -= 4; }
            if (x >= 2) { final[1] = true; x -= 2; }
            if (x >= 1) { final[0] = true;         }

            return final;
        }

        public static byte[] BinToNeurons(bool[] x)
        {
            byte[] final = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

            if (x[3] == true) final[3] = 0xFF;
            if (x[2] == true) final[2] = 0xFF;
            if (x[1] == true) final[1] = 0xFF;
            if (x[0] == true) final[0] = 0xFF;

            return final;
        }

        public static byte BinToByte(bool[] x)
        {
            byte final = 0;

            if (x[3] == true) final |= 8;
            if (x[2] == true) final |= 4;
            if (x[1] == true) final |= 2;
            if (x[0] == true) final |= 1;

            return final;
        }

        public static byte FloatToByte(float[] x)
        {
            byte final = 0;

            bool[] newX = new bool[4];
            if (x[3] >= 0) newX[3] = true;
            if (x[2] >= 0) newX[2] = true;
            if (x[1] >= 0) newX[1] = true;
            if (x[0] >= 0) newX[0] = true;

            final = BinToByte(newX);

            return final;
        }

        public static byte[] NibbleToNeurons(byte x)
        {
            byte[] final = BinToNeurons(NibbleToBin(x));

            return final;
        }
    }
}
