using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace datboi
{
    class Canvas
    {
        private Pixel[,] content;

        public Canvas()
        {
            content = new Pixel[480, 640];
            for (int x = 0; x < 640; x++)
                for (int y = 0; y < 480; y++)
                {
                    content[y, x].Color = 0x0F;
                    content[y, x].Text = string.Empty;
                }
        }

        public void setPixel(uint x, uint y, byte color, string message)
        {
            content[y, x].Color = color;
            content[y, x].Text = message;
        }
    }
}
