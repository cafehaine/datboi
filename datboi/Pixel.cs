namespace datboi
{
    struct Pixel
    {
        public string Text;
        public char ColorHex;
        public byte Color
        {
            get
            { return color; }

            set
            {
                color = value;
                ColorHex = value.ToString("X")[0];
            }
        }

        private byte color;

        public override string ToString()
        {
            if (Text == null)
                return "null";
            return "{\"color\":" + color + ",\"text\":\"" + Text + "\"}";
        }
    }
}
