namespace datboi
{
    struct Pixel
    {
        public string Text;
        public string ColorHex;
        public byte Color
        {
            get
            {
                return color;
            }

            set
            {
                color = value;
                ColorHex = hexColor(value);
            }
        }
        private byte color;

        private static string hexColor(byte Color)
        {
            return Color.ToString("X");
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
