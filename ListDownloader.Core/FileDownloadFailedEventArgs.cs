using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDownloader.Core
{
    public class FileDownloadFailedEventArgs : EventArgs
    {
        public string Link { get; set; }

        public string ErrorMessage { get; set; }

    }
}
