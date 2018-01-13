using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListDownloader.Core
{
    public class DirectorySaver : ISaver
    {
        private IFilenameParser _parser;
        private string _outputDirectory;

        public DirectorySaver(IFilenameParser parser)
        {
            _parser = parser;
        }

        public void SetOutputDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
            {
                throw new ArgumentException($"Cannot access directory {directoryName}");
            }

            _outputDirectory = directoryName;
        }

        async public Task Save(string link, Stream data)
        {
            var filename = _parser.GetFilename(link);
            var filepath = Path.Combine(_outputDirectory, filename);
            using (var file = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 4, useAsync: true))
            {
                await data.CopyToAsync(file);
            }
        }
    }
}
