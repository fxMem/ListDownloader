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
            var kernel = setupNinject(5);

            var downloader = kernel.Get<IDownloader>();
            var links = File.ReadAllLines(@"");

            var timer = new Stopwatch();
            downloader.FileDownloaded += (o, e) => { showProgress(e.FilesDownloaded, e.FilesTotal, timer); };

            timer.Start();
            downloader.DownloadAll(links, @"");

            Console.ReadKey();
        }

        private static void showProgress(int downloaded, int total, Stopwatch t)
        {
            Console.Write("\r Loaded: [{0} / {1}], {2:#00.}% total.", downloaded, total, ((double)downloaded / total) * 100);

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
    }
}
