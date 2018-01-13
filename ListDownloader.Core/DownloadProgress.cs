using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDownloader.Core
{
    public struct DownloadProgress
    {
        public int Downloaded { get; set; }

        public int Total { get; set; }

        public DownloadProgress(int downloaded, int total)
        {
            Downloaded = downloaded;
            Total = total;
        }
    }
}
