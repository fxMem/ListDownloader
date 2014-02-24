using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Ninject;
using ListDownloader.Core;

namespace ListDownloader.CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFilePath, destDirectoryPath;
            int threadCount;

            if (args.Length == 3)
            {
                sourceFilePath = args[0];
                destDirectoryPath = args[1];
                threadCount = Int32.Parse(args[2]);
            }
            else
            {
                Console.WriteLine("Arguments must be: " + System.AppDomain.CurrentDomain.FriendlyName + " <0> <1> <2>");
                Console.WriteLine("<0> Path to links file");
                Console.WriteLine("<1> Destination dir (Created if not exists)");
                Console.WriteLine("<2> Threads count");
                return;
            }

            if (! ( checkSourceFile(sourceFilePath) && 
                checkDestDirectory(destDirectoryPath) && 
                checkTreadCount(threadCount)) )
            {
                return;
            }
            
            Console.WriteLine("Starting downloading..");

            var kernel = setupNinject(threadCount);

            var downloader = kernel.Get<IDownloader>();
            var links = File.ReadAllLines(sourceFilePath);

            var timer = new Stopwatch();
            downloader.FileDownloaded += (o, e) => { showProgress(e.FilesDownloaded, e.FilesTotal, timer); };

            timer.Start();
            downloader.DownloadAll(links, destDirectoryPath);

            Console.ReadKey();
            timer.Stop();
        }

        private static void showProgress(int downloaded, int total, Stopwatch t)
        {
            Console.Write("\r Loaded: [{0} / {1}], {2:##0.}% total.", downloaded, total, ((double)downloaded / total) * 100);

            if (downloaded == total)
            {
                Console.WriteLine();
                Console.WriteLine("Time = {0}", t.Elapsed);
            }
        }

        private static IKernel setupNinject(int threadNumber)
        {
            var kernel = new StandardKernel();
            kernel.Bind<IFilenameParser>().To<FilenameParser>();
            //kernel.Bind<IDownloader>().To<SyncDownloader>();
            kernel.Bind<IDownloader>().ToConstructor
                (
                    arg
                        =>
                    new AsyncDownloader(threadNumber, arg.Inject<IFilenameParser>())
                );

            return kernel;
        }

        private static bool checkSourceFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Source file doesnt exist...");
                return false;
            }
                
            return true;
        }

        private static bool checkDestDirectory(string destDirectoryPath)
        {
            if (!Directory.Exists(destDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(destDirectoryPath);
                    Console.WriteLine("Directory " + destDirectoryPath + " created!");
                }
                catch
                {
                    Console.WriteLine("Cant find or create directory. Aborting...");
                    return false;
                }
            }

            return true;
        }

        private static bool checkTreadCount(int threadCount)
        {
            if (threadCount <= 0)
            {
                Console.WriteLine("Threads number cant be 0 or less...");
                return false;
            }
                

            System.Net.ServicePointManager.DefaultConnectionLimit = threadCount;

            return true;
        }
    }
}
