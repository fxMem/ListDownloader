using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace ListDownloader.Core
{
    public class SyncDownloader : IDownloader
    {
        private HttpClient _client;
        private IFilenameParser _parser;

        private string _directory;

        private int _linksTotalCount;
        private int _downloadsSucessful;
        private int _downloadsFailed;

        public SyncDownloader(
            IFilenameParser parser)
        {
            _client = new HttpClient();
            _parser = parser;
        }

        public void DownloadAll(IEnumerable<string> links, Action<string, byte[]> operation)
        {
            throw new NotImplementedException();
        }

        async public void DownloadAll(IEnumerable<string> links, string directoryForSaving)
        {
            _directory = directoryForSaving;
            _linksTotalCount = links.Count();

            foreach (var link in links)
            {
                await downloadToFile(link);
                fileDownloadSuccesful();
            }
        }

        public event EventHandler<FileDownloadedEventArgs> FileDownloaded;

        public event EventHandler<FileDownloadFailedEventArgs> FileDownloadFailed;

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

            onDownloadSuccesful
                (
                    new FileDownloadedEventArgs
                    {
                        FilesDownloaded = _downloadsSucessful,
                        FilesTotal = _linksTotalCount
                    }
                );
        }

        private void fileDownloadFailed(string link, string msg)
        {
            _downloadsFailed++;
            
            onDownloadFailed
                (
                     new FileDownloadFailedEventArgs
                     {
                         Link = link,
                         ErrorMessage = msg
                     }

                );
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
