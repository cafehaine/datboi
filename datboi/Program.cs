using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace datboi
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Console.WriteLine("Caching files...");
            string css = File.ReadAllText("style.css");
            string js = File.ReadAllText("script.js");
            Console.WriteLine("Setting up canvas and ip buffer...");
            SortedList<IPAddress, DateTime> ipHistory = new SortedList<IPAddress, DateTime>(1024);
            Canvas canvas = new Canvas();
            HttpListener serv = new HttpListener();
            serv.Prefixes.Add("http://127.0.0.1:6699/");
            serv.Start();
            Console.WriteLine("Misc...");
            Regex index = new Regex(@"^(\/|index\.html?)$");
            Regex file = new Regex(@"^\/(style\.css|script\.js)$");
            Regex pixel = new Regex(@"^\/gettext\?x=(\d)+&y=(\d)+$");
            Console.WriteLine("Ready.");
            Stopwatch watch = new Stopwatch();

            while (true)
            {
                HttpListenerContext context = serv.GetContext();
                HttpListenerRequest request = context.Request;
                Console.WriteLine("Request from " + request.RemoteEndPoint.Address + ":");
                Console.WriteLine('\t' + request.Url.PathAndQuery.ToString());
                string queryString = request.Url.PathAndQuery.ToString();

                Console.WriteLine("\tWriting response.");
                HttpListenerResponse response = context.Response;
                response.ContentEncoding = Encoding.UTF8;
                watch.Start();
                if (index.IsMatch(queryString))
                {
                    response.AddHeader("Content-Type", "text/html");
                    canvas.GeneratePage(response);
                }
                else if (file.IsMatch(queryString))
                {
                    switch (queryString)
                    {
                        case "/style.css":
                            response.AddHeader("Content-Type", "text/css");
                            SendString(response, css);
                            break;
                        case "/script.js":
                            response.AddHeader("Content-Type", "text/javascript");
                            SendString(response, js);
                            break;
                    }
                }
                else if (pixel.IsMatch(queryString))
                {
                    response.AddHeader("Content-Type", "text/plain");
                    MatchCollection matches = pixel.Matches(queryString);
                    SendString(response, canvas.GetPixelText(0,0));
                }
                else // 404
                {
                    response.AddHeader("Content-Type", "text/html");
                    SendString(response, @"<!DOCTYPE HTML><html><head><meta charset=""utf-8""><title>4o4</title></head><body>4o4</body></html>");
                }
                watch.Stop();
                response.Close();
                Console.WriteLine("\tResponse sent. Generated in " + watch.Elapsed.Milliseconds + "ms");
                watch.Reset();
            }
        }

        static void SendString(HttpListenerResponse response, string text)
        {
            Stream str = response.OutputStream;
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            str.Write(buffer, 0, buffer.Length);
            str.Close();
        }
    }
}
