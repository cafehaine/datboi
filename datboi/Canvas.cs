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
        private Pixel[,] content;
        private Random rng = new Random();

        public Canvas()
        {
            content = new Pixel[480, 640];
            for (int x = 0; x < 640; x++)
                for (int y = 0; y < 480; y++)
                {
                    content[y, x].Color = (byte)rng.Next(18);
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

            StringBuilder output = new StringBuilder(800000);
            output.Append(@"<!DOCTYPE html><html><head><meta charset=""UTF-8""><title>DatBoi</title></head><body>");
            // build page here
            output.Append("<canvas width=\"640\" height=\"480\">");
            for (int y = 0; y < 480; y++)
                for (int x = 0; x < 640; x++)
                    output.Append(content[y,x].ColorHex);
            // Done
            output.Append("</canvas></body></html>");
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
