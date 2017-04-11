namespace datboi
{
    struct Color
    {
        public byte R;
        public byte G;
        public byte B;

        public Color(string hex)
        {
            R = byte.Parse(hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            G = byte.Parse(hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
            B = byte.Parse(hex[3].ToString(), System.Globalization.NumberStyles.HexNumber);
            R = (byte)(R * 16 + R);
            G = (byte)(G * 16 + G);
            B = (byte)(B * 16 + B);
        }
    }

    static class Colors
    {
        public static readonly Color[] Palette = new Color[]{
            new Color("#000"),
            new Color("#005"),
            new Color("#00A"),
            new Color("#00F"),
            new Color("#050"),
            new Color("#055"),
            new Color("#05A"),
            new Color("#05F"),
            new Color("#0A0"),
            new Color("#0A5"),
            new Color("#0AA"),
            new Color("#0AF"),
            new Color("#0F0"),
            new Color("#0F5"),
            new Color("#0FA"),
            new Color("#0FF"),
            new Color("#500"),
            new Color("#505"),
            new Color("#50A"),
            new Color("#50F"),
            new Color("#550"),
            new Color("#555"),
            new Color("#55A"),
            new Color("#55F"),
            new Color("#5A0"),
            new Color("#5A5"),
            new Color("#5AA"),
            new Color("#5AF"),
            new Color("#5F0"),
            new Color("#5F5"),
            new Color("#5FA"),
            new Color("#5FF"),
            new Color("#A00"),
            new Color("#A05"),
            new Color("#A0A"),
            new Color("#A0F"),
            new Color("#A50"),
            new Color("#A55"),
            new Color("#A5A"),
            new Color("#A5F"),
            new Color("#AA0"),
            new Color("#AA5"),
            new Color("#AAA"),
            new Color("#AAF"),
            new Color("#AF0"),
            new Color("#AF5"),
            new Color("#AFA"),
            new Color("#AFF"),
            new Color("#F00"),
            new Color("#F05"),
            new Color("#F0A"),
            new Color("#F0F"),
            new Color("#F50"),
            new Color("#F55"),
            new Color("#F5A"),
            new Color("#F5F"),
            new Color("#FA0"),
            new Color("#FA5"),
            new Color("#FAA"),
            new Color("#FAF"),
            new Color("#FF0"),
            new Color("#FF5"),
            new Color("#FFA"),
            new Color("#FFF")};
    }
}
