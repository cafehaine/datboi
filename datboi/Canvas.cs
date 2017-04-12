using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

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
                        content[y, x].ColorCode = 'g';
                        content[y, x].Text = "Hello there, nobody set this pixel yet :(";
                        page.Append(content[y, x].ColorCode);
                    }
            }
            else
            {
                Console.WriteLine("Loading previous save...");
                int i = 0;
                foreach (string line in File.ReadLines(savePath))
                {
                    if (line == string.Empty)
                        break;
                    Pixel p = new Pixel();
                    p.ColorCode = line[0];
                    p.Text = line.Substring(1);
                    content[i / 640, i % 640] = p;
                    page.Append(p.ColorCode);
                    i++;
                }
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

        public void SetPixel(int x, int y, char color, string message)
        {
            if (x < 0 || x > 639 || y < 0 || y > 479)
                return;
            lock (contentLock)
            {
                content[y, x].ColorCode = color;
                content[y, x].Text = message;
            }
            page[beforeLength + x + y * 640] = content[y, x].ColorCode;
        }

        public void Save(string path)
        {
            FileStream stream = File.OpenWrite(path + "_");
            byte[] buffer;
            lock (contentLock)
            {
                foreach (Pixel px in content)
                {
                    buffer = Encoding.UTF8.GetBytes(px.ColorCode + px.Text + '\n');
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            stream.Close();
            if (File.Exists(path))
                File.Delete(path);
            File.Move(path + "_", path);
        }

        public Bitmap GetBitmap()
        {
            Bitmap output = new Bitmap(640, 480);
            BitmapData data = output.LockBits(new Rectangle(0, 0, 640, 480), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            IntPtr pointer = data.Scan0;

            int length = Math.Abs(data.Stride) * output.Height;
            byte[] rgbValues = new byte[length];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(pointer, rgbValues, 0, length);

            // Set every third value to 255. A 24bpp bitmap will look red.
            for (int i = 0; i < length; i += 3)
            {
                Color col = content[i / 3 / 640, i / 3 % 640].Color;
                // Blue Green Red
                rgbValues[i] = col.B;
                rgbValues[i + 1] = col.G;
                rgbValues[i + 2] = col.R;
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pointer, length);
            output.UnlockBits(data);
            return output;
        }

        public override string ToString()
        {
            return page.ToString();
        }
    }
}
