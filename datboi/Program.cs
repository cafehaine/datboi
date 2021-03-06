﻿using System;
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
        static int serverThreads = Environment.ProcessorCount;
        static Regex rgIndex = new Regex(@"^(\/|index\.html?)$");
        static Regex rgFile = new Regex(@"^\/[a-zA-Z_]*\.(svg|css|png|js|ico)$");
        static Regex staticAssets = new Regex(@"^[0-9a-zA-Z_]*\.(html|svg|css|png|js|ico)$");
        static Regex rgGetPixel = new Regex(@"^\/getpixel\?x=(\d)+&y=(\d)+$");
        static Regex rgGetBitmap = new Regex(@"^/screen.png$");
        static Dictionary<IPAddress, DateTime> ipHistory = new Dictionary<IPAddress, DateTime>(1024);
        static Queue<HttpListenerContext> contextQueue = new Queue<HttpListenerContext>(10);
        static threadWorker[] threads;
        static Dictionary<string, byte[]> cachedAssets;
        static Bitmap bitmap;
        public static bool PixelChanged;

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
                            if (saveInterval < 10000)
                            {
                                Console.WriteLine("Save interval is too short, please enter a value greater than 10000 ms.");
                                throw new Exception();
                            }
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

            #region Cache files

            cachedAssets = new Dictionary<string, byte[]>(10);
            foreach (string filePath in Directory.EnumerateFiles("."))
            {
                string file = Path.GetFileName(filePath);
                if (staticAssets.IsMatch(file))
                {
                    cachedAssets.Add(file, File.ReadAllBytes(file));
                }
            }
            #endregion

            canvas = new Canvas(savePath);
            bitmap = canvas.GetBitmap();
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
            threads = new threadWorker[serverThreads];
            for (int i = 0; i < serverThreads; i++)
            {
                threads[i] = new threadWorker();
                threads[i].Lock = new object();
                threads[i].ID = i;
                threads[i].Thread = new Thread(new ThreadStart(threads[i].Worker));
                threads[i].Thread.Start();
            }
            Thread watcher = new Thread(new ThreadStart(Watcher));
            watcher.Start();
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

        static void Watcher()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                for (int i = 0; i < serverThreads; i++)
                {
                    threadWorker tw = threads[i];
                    lock (tw.Lock)
                    {
                        if (tw.TreatingRequest && (tw.StartedTreating - now).TotalSeconds > 5)
                        {
                            Console.WriteLine("Restarting thread " + i);
                            tw.Thread.Abort();
                            tw.Thread.Start();
                        }
                    }
                }
                if (PixelChanged)
                {
                    lock (bitmap)
                    {
                        bitmap = canvas.GetBitmap();
                    }
                    PixelChanged = false;
                }
                Thread.Sleep(100);
            }
        }

        struct threadWorker
        {
            public object Lock;
            public bool TreatingRequest;
            public DateTime StartedTreating;
            public Thread Thread;
            public int ID;

            public void Worker()
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
                        lock (Lock)
                        {
                            TreatingRequest = true;
                            StartedTreating = DateTime.Now;
                        }
                        try
                        {
                            watch.Start();
                            HttpListenerRequest rq = context.Request;
                            string uri = rq.Url.PathAndQuery.ToString();
                            HttpListenerResponse rp = context.Response;
                            rp.ContentEncoding = Encoding.UTF8;

                            if (rgIndex.IsMatch(uri))
                            {
                                SendAsset(rp, "index.html");
                            }
                            else if (rgGetBitmap.IsMatch(uri))
                            {
                                Stream output = rp.OutputStream;
                                lock (bitmap)
                                {
                                    bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                                }
                                output.Close();
                            }
                            else if (rgFile.IsMatch(uri))
                            {
                                // Cache static files for at least 1 day.
                                rp.AddHeader("Cache-Control", "max-age=86400");
                                SendAsset(rp, uri.Substring(1));
                            }
                            else if (rgGetPixel.IsMatch(uri))
                            {
                                rp.AddHeader("Content-Type", "text/plain");
                                MatchCollection matches = rgGetPixel.Matches(uri);
                                SendString(rp, canvas.GetPixel(0, 0).ToString());
                            }
                            else // 404
                            {
                                rp.StatusCode = 404;
                                SendAsset(rp, "404.html");
                            }

                            Console.WriteLine("[" + ID + "] HTTP request from "
                                + rq.RemoteEndPoint.Address + " for " + uri +
                                " in " + watch.ElapsedMilliseconds + "ms");
                            rp.Close();
                            watch.Stop();
                            watch.Reset();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        lock (Lock)
                        {
                            TreatingRequest = false;
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }
        }

        static void SaveTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Saving canvas...");
            canvas.Save(savePath);
            Console.WriteLine("Canvas saved.");
        }

        static void SendAsset(HttpListenerResponse rp, string assetName)
        {
            rp.AddHeader("Content-Type", Mime.GetType(assetName));
            Stream os = rp.OutputStream;
            byte[] asset = null;
            if (cachedAssets.TryGetValue(assetName, out asset))
            {
                rp.ContentLength64 = asset.LongLength;
                os.Write(asset, 0, asset.Length);
                os.Close();
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
