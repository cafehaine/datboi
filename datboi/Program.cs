using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using static System.ConsoleColor;

namespace datboi
{
    class Program
    {
        static Canvas canvas;
        static string savePath = "save_canvas.txt";
        static double saveInterval = 300000;
        static string listeningUrl = "http://127.0.0.1:6699/";
        static double timeLimit = 1000;
        static Regex index = new Regex(@"^(\/|index\.html?)$");
        static Regex file = new Regex(@"^\/(style\.css|script\.js|favicon\.ico)$");
        static Regex getPixel = new Regex(@"^\/getpixel\?x=(\d)+&y=(\d)+$");
        static Regex setPixel = new Regex(@"^\/pixel$");
        static Regex getBitmap = new Regex(@"^/screen.png$");
        static Regex post = new Regex(@"^x=(\d+)&y=(\d+)&color=([0-9a-zA-Z-_])$");
        static Dictionary<IPAddress, DateTime> ipHistory = new Dictionary<IPAddress, DateTime>(1024);

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
                        case "-t":
                        case "--interval":
                            i++;
                            timeLimit = double.Parse(args[i]);
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
                Console.WriteLine("-t\t--interval\tSpecify the minimum time between each pixel that an ip can set (ms).");
                Console.WriteLine("\tDefault: 1000");
                return;
            }

            Console.WriteLine("Starting up...");
            string css = File.ReadAllText("style.css");
            string js = File.ReadAllText("script.js");
            byte[] ico = File.ReadAllBytes("favicon.ico");
            string before = File.ReadAllText("before.html");
            string after = File.ReadAllText("after.html");
            canvas = new Canvas(before, after, savePath);
            HttpListener serv = new HttpListener();
            try
            {
                serv.Prefixes.Add(listeningUrl);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid listening url.");
                return;
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Reasonable timeouts
                serv.TimeoutManager.EntityBody = new TimeSpan(0, 0, 0, 0, 500);
                serv.TimeoutManager.DrainEntityBody = new TimeSpan(0, 0, 1);
                serv.TimeoutManager.IdleConnection = new TimeSpan(0, 0, 1);
                serv.TimeoutManager.MinSendBytesPerSecond = 2000;
                serv.TimeoutManager.HeaderWait = new TimeSpan(0, 0, 1);
                serv.TimeoutManager.RequestQueue = new TimeSpan(0, 0, 5);
            }
            serv.Start();
            Stopwatch watch = new Stopwatch();
            Timer saveTimer = new Timer(saveInterval);
            saveTimer.Elapsed += SaveTimerElapsed;
            Console.WriteLine("Shit waddup.");
            saveTimer.Start();

            while (true)
            {
                try
                {
                    HttpListenerContext context = serv.GetContext();
                    HttpListenerRequest request = context.Request;
                    string queryString = request.Url.PathAndQuery.ToString();
                    Console.Write("Request from ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(request.RemoteEndPoint.Address);
                    Console.ResetColor();
                    Console.WriteLine("\tfor " + queryString);

                    Console.Write("\tWriting...");
                    HttpListenerResponse response = context.Response;
                    response.ContentEncoding = Encoding.UTF8;
                    watch.Start();
                    if (index.IsMatch(queryString))
                    {
                        if (request.InputStream != null)
                        {
                            SetPixel(request.InputStream, request.RemoteEndPoint.Address);
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
                        bool set = false;
                        if (request.InputStream != null)
                        {
                            set = SetPixel(request.InputStream, request.RemoteEndPoint.Address);
                        }
                        response.AddHeader("Content-Type", "text/plain");
                        SendString(response, set ? "ok" : "denied");
                    }
                    else if (getBitmap.IsMatch(queryString))
                    {
                        response.AddHeader("Content-Type", "image/png");
                        Stream output = response.OutputStream;
                        canvas.GetBitmap().Save(output, System.Drawing.Imaging.ImageFormat.Png);
                        output.Close();
                    }
                    else // 404
                    {
                        response.StatusCode = 404;
                        response.AddHeader("Content-Type", "text/html");
                        SendString(response, @"<!DOCTYPE HTML><html><head><meta charset=""utf-8""><title>4o4</title></head><body>4o4</body></html>");
                    }
                    watch.Stop();
                    response.Close();
                    Console.Write(" Response sent. Done in ");
                    if (watch.ElapsedMilliseconds >= 10)
                        Console.ForegroundColor = watch.ElapsedMilliseconds < 20 ? Yellow : Red;
                    Console.WriteLine(watch.ElapsedMilliseconds + "ms");
                    Console.ResetColor();
                    watch.Reset();
                }
                catch (Exception)
                { }
            }
        }

        static bool SetPixel(Stream requestStream, IPAddress ip)
        {
            byte[] buffer = new byte[40];
            int b = 0;
            int i = 0;
            while ((b = requestStream.ReadByte()) != -1)
            {
                buffer[i] = (byte)b;
                i++;
            }
            requestStream.Close();
            string rq = Encoding.ASCII.GetString(buffer).Trim((char)0);
            if (post.IsMatch(rq))
            {
                bool shouldSet = true;
                if (ipHistory.ContainsKey(ip))
                {
                    if ((DateTime.Now - ipHistory[ip]).TotalMilliseconds < timeLimit)
                        shouldSet = false;
                    else
                        ipHistory[ip] = DateTime.Now;
                }
                else
                    ipHistory.Add(ip, DateTime.Now);

                if (shouldSet)
                {
                    GroupCollection captures = post.Match(rq).Groups;
                    canvas.SetPixel(int.Parse(captures[1].Value), int.Parse(captures[2].Value), captures[3].Value[0], "lol");
                    return true;
                }
            }
            return false;
        }

        static void SaveTimerElapsed(object sender, ElapsedEventArgs e)
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
