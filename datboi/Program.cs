using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace datboi
{
    class Program
    {
        static Canvas canvas;
        static string savePath = "save_canvas.txt";
        static double saveInterval = 300000;
        static string listeningUrl = "http://127.0.0.1:6699/";

        static void Main(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-s":
                        case "--save-path":
                            i++;
                            savePath = args[i];
                            break;
                        case "-i":
                        case "--save-interval":
                            i++;
                            saveInterval = double.Parse(args[i]);
                            break;
                        case "-u":
                        case "--listening-url":
                            i++;
                            listeningUrl = args[i];
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Datboi -- usage:");
                Console.WriteLine("-s\t--save-path\tSpecify the path to be used to save the canvas.");
                Console.WriteLine("\tDefault: \"save_canvas.txt\"");
                Console.WriteLine("-i\t--save-interval\tSpecify interval between each save of the canvas (ms).");
                Console.WriteLine("\tDefault: 300000");
                Console.WriteLine("-u\t--listening-url\tSpecify the url to listen to.");
                Console.WriteLine("\tDefault: \"http://127.0.0.1:6699/\"");
                return;
            }

            Console.WriteLine("Starting...");
            Console.WriteLine("Caching files...");
            string css = File.ReadAllText("style.css");
            string js = File.ReadAllText("script.js");
            byte[] ico = File.ReadAllBytes("favicon.ico");
            string before = File.ReadAllText("before.html");
            string after = File.ReadAllText("after.html");
            Console.WriteLine("Setting up canvas and ip buffer...");
            Dictionary<IPAddress, DateTime> ipHistory = new Dictionary<IPAddress, DateTime>(1024);
            canvas = new Canvas(before, after, savePath);
            HttpListener serv = new HttpListener();
            serv.Prefixes.Add(listeningUrl);
            serv.Start();
            Console.WriteLine("Misc...");
            Regex index = new Regex(@"^(\/|index\.html?)$");
            Regex file = new Regex(@"^\/(style\.css|script\.js|favicon\.ico)$");
            Regex getPixel = new Regex(@"^\/getpixel\?x=(\d)+&y=(\d)+$");
            Regex setPixel = new Regex(@"^\/pixel$");
            Regex post = new Regex(@"^x=(\d+)&y=(\d+)&color=([0-9A-F])$");
            Stopwatch watch = new Stopwatch();
            Timer saveTimer = new Timer(saveInterval);
            saveTimer.Elapsed += SaveTimer_Elapsed;
            Console.WriteLine("Shit waddup.");
            saveTimer.Start();

            while (true)
            {
                try
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
                        if (request.InputStream != null)
                        {
                            byte[] buffer = new byte[40];
                            int b = 0;
                            int i = 0;
                            while ((b = request.InputStream.ReadByte()) != -1)
                            {
                                buffer[i] = (byte)b;
                                i++;
                            }
                            string rq = Encoding.ASCII.GetString(buffer).Trim((char)0);
                            if (post.IsMatch(rq))
                            {
                                bool shouldSet = true;
                                if (ipHistory.ContainsKey(request.RemoteEndPoint.Address))
                                {
                                    if ((DateTime.Now - ipHistory[request.RemoteEndPoint.Address]).Seconds < 1)
                                        shouldSet = false;
                                    else
                                        ipHistory[request.RemoteEndPoint.Address] = DateTime.Now;
                                }
                                else
                                    ipHistory.Add(request.RemoteEndPoint.Address, DateTime.Now);

                                if (shouldSet)
                                {
                                    GroupCollection captures = post.Match(rq).Groups;
                                    canvas.SetPixel(int.Parse(captures[1].Value), int.Parse(captures[2].Value), byte.Parse(captures[3].Value, System.Globalization.NumberStyles.HexNumber), "lol");
                                }
                            }
                        }
                        response.AddHeader("Content-Type", "text/html");
                        SendString(response, canvas.ToString());
                    }
                    else if (file.IsMatch(queryString))
                    {
                        // Cache static files for at least 1 day.
                        response.AddHeader("Cache-Control", "max-age=86400");
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
                            case "/favicon.ico":
                                response.AddHeader("Content-Type", "image/x-icon");
                                Stream output = response.OutputStream;
                                output.Write(ico, 0, ico.Length);
                                output.Close();
                                break;
                        }
                    }
                    else if (getPixel.IsMatch(queryString))
                    {
                        response.AddHeader("Content-Type", "text/plain");
                        MatchCollection matches = getPixel.Matches(queryString);
                        SendString(response, canvas.GetPixel(0, 0).ToString());
                    }
                    else if (setPixel.IsMatch(queryString))
                    {
                        int x = 0;
                        int y = 0;
                        bool set = false;
                        if (request.InputStream != null)
                        {
                            byte[] buffer = new byte[200];
                            int b = 0;
                            int i = 0;
                            while ((b = request.InputStream.ReadByte()) != -1)
                            {
                                buffer[i] = (byte)b;
                                i++;
                            }
                            string rq = Encoding.ASCII.GetString(buffer).Trim((char)0);
                            if (post.IsMatch(rq))
                            {
                                bool shouldSet = true;
                                if (ipHistory.ContainsKey(request.RemoteEndPoint.Address))
                                {
                                    if ((DateTime.Now - ipHistory[request.RemoteEndPoint.Address]).Seconds < 1)
                                        shouldSet = false;
                                    else
                                        ipHistory[request.RemoteEndPoint.Address] = DateTime.Now;
                                }
                                else
                                    ipHistory.Add(request.RemoteEndPoint.Address, DateTime.Now);

                                if (shouldSet)
                                {
                                    GroupCollection captures = post.Match(rq).Groups;
                                    x = int.Parse(captures[1].Value);
                                    y = int.Parse(captures[2].Value);   
                                    canvas.SetPixel(x, y, byte.Parse(captures[3].Value, System.Globalization.NumberStyles.HexNumber), "lol");
                                    set = true;
                                }
                            }
                        }
                        response.AddHeader("Content-Type", "text/plain");
                        SendString(response, set ? "ok" : "denied");
                    }
                    else // 404
                    {
                        response.StatusCode = 404;
                        response.AddHeader("Content-Type", "text/html");
                        SendString(response, @"<!DOCTYPE HTML><html><head><meta charset=""utf-8""><title>4o4</title></head><body>4o4</body></html>");
                    }
                    watch.Stop();
                    response.Close();
                    Console.WriteLine("\tResponse sent. Generated in " + watch.Elapsed.Milliseconds + "ms");
                    watch.Reset();
                }
                catch (Exception)
                { }
            }
        }

        private static void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Saving canvas...");
            canvas.Save(savePath);
            Console.WriteLine("Canvas saved.");
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
