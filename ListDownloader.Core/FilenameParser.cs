using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;


namespace ListDownloader.Core
{
    public class FilenameParser : IFilenameParser
    {
        private string _deny = @"[/|:?*|<>]";
        private Regex _match;

        public FilenameParser()
        {
            _match = new Regex(_deny);
        }

        public string GetFilename(string link)
        {
            var filename = System.Web.HttpUtility.UrlDecode(Path.GetFileName(link));
            var newName = _match.Replace(filename, "");
            return newName;
        }
    }
}
