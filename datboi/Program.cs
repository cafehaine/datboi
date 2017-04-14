using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using WebSocketSharp.Server;

namespace datboi
{
    class Program
    {
        static Canvas canvas;
        static string savePath = "save_canvas.txt";
        static double saveInterval = 300000;
        static string listeningUrl = "http://127.0.0.1:6699/";
        static double timeLimit = 1000;
        static int serverThreads = 8;
        static Regex index = new Regex(@"^(\/|index\.html?)$");
        static Regex file = new Regex(@"^\/(style\.css|script\.js|favicon\.ico)$");
        static Regex getPixel = new Regex(@"^\/getpixel\?x=(\d)+&y=(\d)+$");
        static Regex getBitmap = new Regex(@"^/screen.png$");
        static Dictionary<IPAddress, DateTime> ipHistory = new Dictionary<IPAddress, DateTime>(1024);
        static Queue<HttpListenerContext> contextQueue = new Queue<HttpListenerContext>(10);
        static Thread[] threads;
        static string css;
        static string js;
        static byte[] ico;

        static void Main(string[] args)
        {
            #region Argument parsing
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
                        case "-c":
                        case "--thread-count":
                            i++;
                            serverThreads = int.Parse(args[i]);
                            if (serverThreads < 1)
                                throw new Exception();
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
                Console.WriteLine("-c\t--thread-count\tSpecify the number of threads for the server to use.");
                Console.WriteLine("\tDefault: 8");
                return;
            }
            #endregion

            Console.WriteLine("Starting up...");
            css = File.ReadAllText("style.css");
            js = File.ReadAllText("script.js");
            ico = File.ReadAllBytes("favicon.ico");
            string before = File.ReadAllText("before.html");
            string after = File.ReadAllText("after.html");
            canvas = new Canvas(before, after, savePath);
            HttpListener serv = new HttpListener();
			WebSocketServer ws = new WebSocketServer(IPAddress.Any, 6660);
			ws.AddWebSocketService<Behavior>("/set");
			ws.Start();
            try
            {
                serv.Prefixes.Add(listeningUrl);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid listening url.");
                return;
            }
#if !__MonoCS__
            // Reasonable timeouts
            serv.TimeoutManager.EntityBody = new TimeSpan(0, 0, 0, 0, 500);
            serv.TimeoutManager.DrainEntityBody = new TimeSpan(0, 0, 1);
            serv.TimeoutManager.IdleConnection = new TimeSpan(0, 0, 1);
            serv.TimeoutManager.MinSendBytesPerSecond = 2000;
            serv.TimeoutManager.HeaderWait = new TimeSpan(0, 0, 1);
            serv.TimeoutManager.RequestQueue = new TimeSpan(0, 0, 5);
#endif
            threads = new Thread[serverThreads];
            for (int i = 0; i < serverThreads; i++)
            {
                threads[i] = new Thread(new ThreadStart(Worker));
                threads[i].Start();
            }

            serv.Start();
            Stopwatch watch = new Stopwatch();
            System.Timers.Timer saveTimer = new System.Timers.Timer(saveInterval);
            saveTimer.Elapsed += SaveTimerElapsed;
            Console.WriteLine("Shit waddup.");
            saveTimer.Start();

            while (true)
            {
                HttpListenerContext context = serv.GetContext();
                lock (contextQueue)
                {
                    contextQueue.Enqueue(context);
                }
            }
        }

		public static bool SetPixel(Behavior client, byte[] data)
		{
			ushort x = (ushort)((data[0] << 4) + ((data[1] & 240) >> 4));
			ushort y = (ushort)(((data[1] & 15) << 8) + data[2]);
			byte color = data[3];
			IPAddress ip = client.Context.UserEndPoint.Address;
			bool shouldSet = true;
			Console.WriteLine("WebSocket request from " + ip);
			if (x >= 640 || y >= 480)
				return false;

            DateTime lastRq;
            lock (ipHistory)
            {
                if (ipHistory.TryGetValue(ip, out lastRq))
                {
                    if ((DateTime.Now - lastRq).TotalMilliseconds < timeLimit)
                        shouldSet = false;
                    else
                        ipHistory[ip] = DateTime.Now;
                }
                else
                    ipHistory.Add(ip, DateTime.Now);
            }

			if (shouldSet)
			{
				canvas.SetPixel(x, y, (char)color, "lol");
				Behavior.sendAll(data);
				return true;
			}
			return false;
		}

        /// <summary>
        /// This function runs a "server" thread
        /// </summary>
        static void Worker()
        {
            Stopwatch watch = new Stopwatch();
            while (true)
            {
                HttpListenerContext context = null;
                lock (contextQueue)
                {
                    if (contextQueue.Count > 0)
                        context = contextQueue.Dequeue();
                }
                if (context != null)
                {
                    try
                    {
                        watch.Start();
                        HttpListenerRequest rq = context.Request;
                        string uri = rq.Url.PathAndQuery.ToString();
                        HttpListenerResponse rp = context.Response;
                        rp.ContentEncoding = Encoding.UTF8;

                        if (index.IsMatch(uri))
                        {
                            rp.AddHeader("Content-Type", "text/html");
                            SendString(rp, canvas.ToString());
                        }
                        else if (file.IsMatch(uri))
                        {
                            // Cache static files for at least 1 day.
                            rp.AddHeader("Cache-Control", "max-age=86400");
                            switch (uri)
                            {
                                case "/style.css":
                                    rp.AddHeader("Content-Type", "text/css");
                                    SendString(rp, css);
                                    break;
                                case "/script.js":
                                    rp.AddHeader("Content-Type", "text/javascript");
                                    SendString(rp, js);
                                    break;
                                case "/favicon.ico":
                                    rp.AddHeader("Content-Type", "image/x-icon");
                                    Stream output = rp.OutputStream;
                                    output.Write(ico, 0, ico.Length);
                                    output.Close();
                                    break;
                            }
                        }
                        else if (getPixel.IsMatch(uri))
                        {
                            rp.AddHeader("Content-Type", "text/plain");
                            MatchCollection matches = getPixel.Matches(uri);
                            SendString(rp, canvas.GetPixel(0, 0).ToString());
                        }
                        else if (getBitmap.IsMatch(uri))
                        {
                            rp.AddHeader("Content-Type", "image/png");
                            Stream output = rp.OutputStream;
                            Bitmap bmp = canvas.GetBitmap();
                            bmp.Save(output,
                                System.Drawing.Imaging.ImageFormat.Png);
                            bmp.Dispose();
                            output.Close();
                        }
                        else // 404
                        {
                            rp.StatusCode = 404;
                            rp.AddHeader("Content-Type", "text/html");
                            SendString(rp, @"<!DOCTYPE HTML><html><head><meta charset=""utf-8""><title>4o4</title></head><body>4o4</body></html>");
                        }

                        // DO STUFF

                        Console.WriteLine("HTTP request from " +
                            rq.RemoteEndPoint.Address + " for " + uri + " in " +
                            watch.ElapsedMilliseconds + "ms");
                        rp.Close();
                        watch.Stop();
                        watch.Reset();
                    }
                    catch (Exception)
                    { }
                }
                else
                    Thread.Sleep(100);
            }
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
