namespace datboi
{
    struct Pixel
    {
        public string Text;
        public char Color;

        public override string ToString()
        {
            if (Text == null)
                return "null";
            return "{\"color\":" + Color + ",\"text\":\"" + Text + "\"}";
        }
    }
}
