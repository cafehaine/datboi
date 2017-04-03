namespace datboi
{
    struct Pixel
    {
        public byte Color;
        public string Text;

        public static string hexColor(byte Color)
        {
            int r = (Color & 0x08) != 0 ? 128 : 0;
            int g = (Color & 0x04) != 0 ? 128 : 0;
            int b = (Color & 0x02) != 0 ? 128 : 0;

            if ((Color & 0x01) != 0)
            {
                r += 127;
                g += 127;
                b += 127;
            }
            return string.Format("#{0:X}{1:X}{2:X}",r,g,b);
        }
    }
}
