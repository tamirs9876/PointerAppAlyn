using Common;
using System;
using System.Threading.Tasks;

namespace TobiiAgent
{
    public class MockAgentAnalyzer : IAgentAnalyzer
    {
        public async void StartWatching(Action<double, double> recognizeMethod)
        {
            var r = new Random();
            var size = OS.GetNativeResolution();
            while (true)
            {
                if (r.NextDouble() < 0.2)
                {
                    var x = r.Next(0, size.Width);
                    var y = r.Next(0, size.Height);
                    recognizeMethod(x, y);
                }

                await Task.Delay(700);
            }
        }

        public void UpdateDelayThreshold(double delay)
        {
            throw new NotImplementedException();
        }
    }
}
