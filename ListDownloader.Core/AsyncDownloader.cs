using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

namespace ListDownloader.Core
{
   
    public class AsyncDownloader : IDownloader
    {
        private int _threadNumber;
        private HttpClient _client;
        private IFilenameParser _parser;

        private IEnumerator<string> _linksQueue;
        private string _directory;

        private int _linkDownloadingCount;

        private int _linksTotalCount;
        private int _downloadsSucessful;
        private int _downloadsFailed;


        public event EventHandler<FileDownloadedEventArgs> FileDownloaded;

        public event EventHandler<FileDownloadFailedEventArgs> FileDownloadFailed;

        public AsyncDownloader(
            int threadNumber, 
            IFilenameParser parser)
        {
            _client = new HttpClient();
            _threadNumber = threadNumber;
            _parser = parser;
        }


        public void DownloadAll(IEnumerable<string> links, Action<string, byte[]> operation)
        {
            throw new NotImplementedException();
        }

        async public void DownloadAll(IEnumerable<string> links, string directoryForSaving)
        {
            _directory = directoryForSaving;
            _linksQueue = links.GetEnumerator();
            _linkDownloadingCount = 0;
            _linksTotalCount = links.Count();

            downloadNextLink();
        }

        private void downloadNextLink()
        {
            if (_linkDownloadingCount >= _threadNumber)
            {
                return;
            }

            if (!_linksQueue.MoveNext())
            {
                // All links were loaded
                return;
            }

            _linkDownloadingCount++;
            var nextLink = _linksQueue.Current;

            downloadToFile(nextLink).ContinueWith
                ( 
                    downloading =>
                    {
                        if (downloading.IsCompleted)
                        {
                            fileDownloadSuccesful();
                        }
                        else
                        {
                            fileDownloadFailed(nextLink, downloading.Exception.Message);
                        }
                        
                    }, 
                    TaskContinuationOptions.ExecuteSynchronously
                );

            downloadNextLink();
        }

        async private Task downloadToFile(string link)
        {
            var filepath = Path.Combine(_directory, _parser.GetFilename(link));

            var inputStream = await _client.GetStreamAsync(link);
            using (var fileStream = File.Create(filepath))
            {
                await inputStream.CopyToAsync(fileStream);
            }

        }

        private void fileDownloadSuccesful()
        {
            _downloadsSucessful++;
            _linkDownloadingCount--;

            onDownloadSuccesful
                (
                    new FileDownloadedEventArgs 
                    { 
                        FilesDownloaded = _downloadsSucessful, 
                        FilesTotal = _linksTotalCount 
                    }
                );
            downloadNextLink();
        }

        private void fileDownloadFailed(string link, string msg)
        {
            _downloadsFailed++;
            _linkDownloadingCount--;

            onDownloadFailed
                (
                     new FileDownloadFailedEventArgs
                     {
                         Link = link,
                         ErrorMessage = msg
                     }

                );
            downloadNextLink();
        }

        protected void onDownloadSuccesful(FileDownloadedEventArgs e)
        {
            var temp = FileDownloaded;

            if (temp != null)
            {
                temp(this, e);
            }
        }

        protected void onDownloadFailed(FileDownloadFailedEventArgs e)
        {
            var temp = FileDownloadFailed;

            if (temp != null)
            {
                temp(this, e);
            }
        }

    }
}
