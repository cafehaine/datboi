using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace datboi
{
    class Canvas
    {
        private string before;
        private string after;
        private Pixel[,] content;
        private Random rng = new Random();

        public Canvas(string Before, string After)
        {
            before = Before;
            after = After;
            content = new Pixel[480, 640];
            for (int x = 0; x < 640; x++)
                for (int y = 0; y < 480; y++)
                {
                    content[y, x].Color = (byte)rng.Next(16);
                    content[y, x].Text = "Hello there, nobody set this pixel yet :(";
                }
        }

        public string GetPixelText(uint x, uint y)
        {
            return content[y, x].Text;
        }

        public void SetPixel(uint x, uint y, byte color, string message)
        {
            content[y, x].Color = color;
            content[y, x].Text = message;
        }

        public void GeneratePage(HttpListenerResponse response)
        {
            Stream str = response.OutputStream;

            StringBuilder output = new StringBuilder(before.Length + after.Length + content.Length);
            output.Append(before);
            for (int y = 0; y < 480; y++)
                for (int x = 0; x < 640; x++)
                    output.Append(content[y,x].ColorHex);
            // Done
            output.Append(after);
            byte[] buffer = Encoding.UTF8.GetBytes(output.ToString());
            response.ContentLength64 = buffer.LongLength;
            try
            {
                str.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught " + e.ToString());
            }
            str.Close();
        }
    }
}
