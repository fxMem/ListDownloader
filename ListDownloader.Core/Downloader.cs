using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ListDownloader.Core
{
    public class Downloader
    {
        private ISaver _saver;
        private DownloaderSettings _settings;

        // Simple reporting implementation
        private IProgress<DownloadProgress> _progress;
        private int _totalLinksCount;
        private int _downloadedCount;
        private int _failedCount;

        // Since downloading is IO-bound operation, we can default to relatively high value
        private int _defaultNumberOfThreads = 20;
        private HttpClient _client;

        public Downloader(ISaver saver, DownloaderSettings settings)
        {
            _saver = saver;
            _settings = settings;
        }

        /// <summary>
        /// Not thread safe.
        /// </summary>
        /// <param name="links"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async public Task Download(IEnumerable<string> links, CancellationToken ct, IProgress<DownloadProgress> progress = null)
        {
            var maxThreads = _settings.SimultaneousDownloads ?? _defaultNumberOfThreads;
            System.Net.ServicePointManager.DefaultConnectionLimit = maxThreads;

            // TODO: check for specific collection type
            _totalLinksCount = links.Count();

            _progress = progress;
            _client = new HttpClient();
            var downloadBlock = new ActionBlock<string>(DownloadLink, 
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = maxThreads,
                BoundedCapacity = maxThreads
            });

            ReportProgress();

            foreach (var link in links)
            {
                await downloadBlock.SendAsync(link).ConfigureAwait(false);
            }

            downloadBlock.Complete();
            await downloadBlock.Completion.ConfigureAwait(false);
        }

        async private Task DownloadLink(string link)
        {
            try
            {
                Trace.TraceInformation($"Starting loading {link}");

                var result = await _client.GetStreamAsync(link).ConfigureAwait(false);

                Trace.TraceInformation($"Finished loading {link}! Saving... ");

                await _saver.Save(link, result).ConfigureAwait(false);

                Trace.TraceInformation($"{link} succesfully saved!");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error occured during loading {link}, {e.ToString()}");
                Interlocked.Increment(ref _failedCount);
            }
            finally
            {
                Interlocked.Increment(ref _downloadedCount);
                ReportProgress();
            }
        }

        private void ReportProgress()
        {
            _progress?.Report(new DownloadProgress(_downloadedCount, _totalLinksCount));
        }
    }
}
