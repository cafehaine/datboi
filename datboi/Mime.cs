using System;

namespace datboi
{
    static class Mime
    {
        public static string GetType(string filename)
        {
            switch(filename.Substring(filename.Length - 3))
            {
                case "svg":
                    return "image/svg+xml";
                case "png":
                    return "image/png";
                case "ico":
                    return "image/x-icon";
                case ".js":
                    return "text/javascript";
                case "css":
                    return "text/css";
                default:
                    throw new Exception("unknown mimetype for " + filename);
            }
        }
    }
}
