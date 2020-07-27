using System;
using System.Text;
using System.Threading.Tasks;

namespace YoloPipe
{
    public class YoloJsonPipe
    {
        private readonly YoloPipe _pipe;
        private EventHandler<DataReadyEventArgs> JsonReady;
        public YoloJsonPipe(string exe, string arguments, string workingFolder)
        {
            _pipe = new YoloPipe(exe, arguments, workingFolder);

        }

        public void Start()
        {
            _pipe.Start();
        }

        public async Task<string> ReadJsonObjextAsync()
        {
            var b = new StringBuilder();
            string line;
            do
            {
                line = await _pipe.ReadNext();
                if (line != "#" && line != null)
                {
                    b.AppendLine(line);
                }
            } while (line != "#" && line != null);

            var result = b.ToString();
            if (result == "")
            {
                result = null;
            }

            return result;
        }

    }
}
