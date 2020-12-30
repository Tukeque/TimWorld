using System;

namespace TimWorld
{
    class Screen
    {
        public static int Height;
        public static int Width;
        public static char[,] screen;

        public static bool Refresh = false; // TODO make it work lol

        public static void UpdateSize(bool refresh)
        {
            Width = Console.WindowWidth;
            Height = Console.WindowHeight;

            Refresh = refresh;
            if (Refresh) screen = new char[Width + 1, Height];
        }

        public static void DisplayScreen()
        {
            Console.CursorVisible = false;

            string buffer = "";
            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    buffer += screen[x, y];
                }
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(buffer);

            if (Refresh) screen = new char[Width + 1, Height];

            // System.Threading.Thread.Sleep(50);
            Console.CursorVisible = true;
        }

        public static void Print(string text, int X, int Y) // TODO add method that supports the cursor continuing from last print
        {
            char[] textCL = text.ToCharArray();

            int x = X; int y = Y;

            for (int i = 0; i < textCL.Length; i++)
            {
                screen[x, y] = textCL[i];

                int oldX = x % Width;
                x = (x + 1) % Width;

                if (oldX > x)
                {
                    y = (y + 1) % Height;
                }
            }
        }

        public static void Clear()
        {
            screen = new char[Width + 1, Height];

            for (int y = 0; y < Height - 1; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    screen[x, y] = Convert.ToChar(" ");
                }
            }
        }

        public static void Init()
        {
            UpdateSize(false);
            Clear();
        }
    }
}
