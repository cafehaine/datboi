using System;
using System.Text;

namespace datboi
{
    class Canvas
    {
        private Pixel[,] content;
        private Random rng = new Random();
        private StringBuilder page;
        private int beforeLength;

        public Canvas(string Before, string After)
        {
            beforeLength = Before.Length;
            content = new Pixel[480, 640];
            page = new StringBuilder(Before.Length + After.Length + content.Length);
            page.Append(Before);
            for (int x = 0; x < 640; x++)
                for (int y = 0; y < 480; y++)
                {
                    content[y, x].Color = 7;
                    content[y, x].Text = "Hello there, nobody set this pixel yet :(";
                    page.Append(content[y, x].ColorHex);
                }
            page.Append(After);
        }

        public string GetPixelText(int x, int y)
        {
            return content[y, x].Text;
        }

        public void SetPixel(int x, int y, byte color, string message)
        {
            if (x < 0 || x > 639 || y < 0 || y > 479)
                return;
            content[y, x].Color = color;
            content[y, x].Text = message;
            page[beforeLength + x + y * 640] = content[y, x].ColorHex;
        }

        public override string ToString()
        {
            return page.ToString();
        }
    }
}
