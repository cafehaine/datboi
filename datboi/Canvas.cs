using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace datboi
{
    class Canvas
    {
        private object contentLock;
        private Pixel[,] content;
        private static Random rng = new Random();
        private StringBuilder page;
        private int beforeLength;

        public Canvas(string Before, string After, string savePath)
        {
            contentLock = new object();
            beforeLength = Before.Length;
            content = new Pixel[480, 640];
            page = new StringBuilder(Before.Length + After.Length + content.Length);
            page.Append(Before);
            if (!File.Exists(savePath))
            {
                for (int x = 0; x < 640; x++)
                    for (int y = 0; y < 480; y++)
                    {
                        content[y, x].Color = 7;
                        content[y, x].Text = "Hello there, nobody set this pixel yet :(";
                        page.Append(content[y, x].ColorHex);
                    }
            }
            else
            {
                Console.WriteLine("\tLoading previous save...");
                int i = 0;
                foreach (string line in File.ReadLines(savePath))
                {
                    if (line == string.Empty)
                        break;
                    Pixel p = new Pixel();
                    p.Color = byte.Parse(line[0].ToString(), NumberStyles.HexNumber);
                    p.Text = line.Substring(1);
                    content[i / 640, i % 640] = p;
                    page.Append(p.ColorHex);
                    i++;
                }
                Console.WriteLine("\tDone.");
            }
            page.Append(After);
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x < 0 || x > 639 || y < 0 || y > 479)
                return new Pixel();
            lock (contentLock)
            {
                return content[y, x];
            }
        }

        public void SetPixel(int x, int y, byte color, string message)
        {
            if (x < 0 || x > 639 || y < 0 || y > 479)
                return;
            lock (contentLock)
            {
                content[y, x].Color = color;
                content[y, x].Text = message;
            }
            page[beforeLength + x + y * 640] = content[y, x].ColorHex;
        }

        public void Save(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            FileStream stream = File.OpenWrite(path);
            byte[] buffer;
            lock (contentLock)
            {
                foreach (Pixel px in content)
                {
                    stream.WriteByte((byte)px.ColorHex);
                    buffer = Encoding.UTF8.GetBytes(px.Text + '\n');
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            stream.Close();
        }

        public override string ToString()
        {
            return page.ToString();
        }
    }
}
