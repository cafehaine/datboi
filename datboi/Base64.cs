using System;

namespace datboi
{
    static class Base64
    {
        /* 0 -> 9 A -> Z a -> z - _ */
        public static int ToInt(char str)
        {
            if (str == '-')
                return 62;
            if (str == '_')
                return 63;
            if (str >= 48 && str <= 57)
                return str - 48;
            if (str >= 65 && str <= 90)
                return str - 55;
            if (str >= 97 && str <= 122)
                return str - 61;
            throw new Exception("Invalid input.");
        }

        public static string ToStr(int arg)
        {
            if (arg < 0 || arg >= 64)
                throw new Exception("Invalid input.");
            if (arg < 10)
                return arg.ToString();
            if (arg < 36)
                return ((char)(arg + 55)).ToString();
            if (arg < 62)
                return ((char)(arg + 61)).ToString();
            if (arg == 62)
                return "-";
            return "_";
        }
    }
}
