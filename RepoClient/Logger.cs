using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDRepo
{
    public class Logger
    {
        private static Logger instance = null;
        private Logger() {
            fileStream = new StreamWriter(Path.Combine(Path.GetTempPath(), "RepoAdaptor.log"), true);
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        public void Log(string text)
        {
            fileStream.WriteLine(text);
            fileStream.Flush();
        }

        private StreamWriter fileStream;
    }
}
