using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Alyn.Pointer.YoloPipe
{
    public class YoloPipe
    {
        private string _exeName = "cmd.exe";
        private string _arguments = "/c dir";
        private readonly string workingFolder;
        private Process process;
        private StreamReader reader;

        public YoloPipe(string exe, string args, string workingFolder = null)
        {
            _exeName = exe;
            _arguments = args;
            this.workingFolder = workingFolder;
        }

        public void Start()
        {
            var pi = new ProcessStartInfo(_exeName, _arguments)
            {
                WorkingDirectory = workingFolder,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process = Process.Start(pi);
            process.Exited += Process_Exited;
            reader = process.StandardOutput;
            /*_reader.ReadLine()*/;
            Debug.WriteLine(reader.ReadLine());
        }

        private void Process_Exited(object sender, EventArgs e)
        {
        }

        // Returns the next string, or null if done.
        public async Task<string> ReadNext()
        {
            var st = await reader.ReadLineAsync();
            return st;
        }
    }
}
