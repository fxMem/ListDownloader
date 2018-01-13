using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDownloader.Core
{
    public interface ISaver
    {
        Task Save(string link, Stream data);
    }
}
