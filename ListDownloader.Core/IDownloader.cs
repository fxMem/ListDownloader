using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDownloader.Core
{
    public interface IDownloader
    {
        void DownloadAll(IEnumerable<string> links, Action<string, byte[]> operation);
        void DownloadAll(IEnumerable<string> links, string directoryForSaving);

        event EventHandler<FileDownloadedEventArgs> FileDownloaded;

        event EventHandler<FileDownloadFailedEventArgs> FileDownloadFailed;
    }
}
