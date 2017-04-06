namespace datboi
{
    struct Pixel
    {
        public string Text;
        public char ColorHex;
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

        private static char hexColor(byte Color)
        {
            return Color.ToString("X")[0];
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
