namespace datboi
{
    struct Pixel
    {
        public string Text;
        public char ColorCode
        {
            get { return code; }
            set
            {
                code = value;
                Color = Colors.Palette[Base64.ToInt(value)];
            }
        }
        public Color Color;
        private char code;

        public override string ToString()
        {
            if (Text == null)
                return "null";
            return "{\"color\":" + code + ",\"text\":\"" + Text + "\"}";
        }
    }
}
