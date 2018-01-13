using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Ninject;
using ListDownloader.Core;
using System.Threading;

namespace ListDownloader.CUI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                PrintWelcome();
                return;
            }

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var sourceFilePath = args[0];
            var destDirectoryPath = args[1];

            int threadCount = 0;
            Int32.TryParse(args[2], out threadCount);

            if (!CheckFileExists(sourceFilePath))
            {
                Console.WriteLine($"Source file {sourceFilePath} doesn't exist!");
                return;
            }

            if (!TryCreateDirectory(destDirectoryPath))
            {
                Console.WriteLine($"Cannot find destination directory {destDirectoryPath}!");
                return;
            }

            var kernel = SetupNinject(threadCount, destDirectoryPath);

            var downloader = kernel.Get<Downloader>();
            var links = File.ReadAllLines(sourceFilePath);

            var timer = new Stopwatch();
            Console.WriteLine("Starting downloading!");

            timer.Start();
            // Do not be scared of sync wait, it's just demonstration
            downloader.Download(links, 
                CancellationToken.None, 
                new Progress<DownloadProgress>(PrintProgress)).Wait();
            timer.Stop();

            Console.WriteLine("Finished! Elapsed time = {0}", timer.Elapsed);
        }

        private static void PrintWelcome()
        {
            Console.WriteLine("Arguments must be: " + System.AppDomain.CurrentDomain.FriendlyName + " <0> <1> <2>");
            Console.WriteLine("<0> Path to links file");
            Console.WriteLine("<1> Destination dir (Created if not exists)");
            Console.WriteLine("<2> Threads count");
        }

        private static void PrintProgress(DownloadProgress progress)
        {
            Console.Write("\r Loaded: [{0} / {1}], {2}% total. ", 
                progress.Downloaded, 
                progress.Total, 
                (int)Math.Ceiling(progress.Downloaded * 100d / progress.Total));
        }

        private static IKernel SetupNinject(int threadNumber, string outputDirectory)
        {
            var kernel = new StandardKernel();
            kernel.Bind<IFilenameParser>().To<FilenameParser>();

            var directorySaver = kernel.Get<DirectorySaver>();
            directorySaver.SetOutputDirectory(outputDirectory);
            kernel.Bind<ISaver>().ToConstant(directorySaver);

            kernel.Bind<DownloaderSettings>().ToConstant(new DownloaderSettings
            {
                SimultaneousDownloads = threadNumber != 0 ? threadNumber : (int?)null
            });

            return kernel;
        }

        private static bool CheckFileExists(string path)
        {
            return File.Exists(path);
        }

        private static bool TryCreateDirectory(string destDirectoryPath)
        {
            if (!Directory.Exists(destDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(destDirectoryPath);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
