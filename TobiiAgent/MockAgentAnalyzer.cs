using Common;
using System;
using System.Threading.Tasks;

namespace TobiiAgent
{
    public class MockAgentAnalyzer : IAgentAnalyzer
    {
        public async void StartWatching(Action<double, double> recognizeMethod)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            var size = OS.GetNativeResolution();
            while (true)
            {
                if (random.NextDouble() < 0.2)
                {
                    var x = random.Next(0, size.Width);
                    var y = random.Next(0, size.Height);
                    recognizeMethod(x, y);
                }

                await Task.Delay(random.Next(1500, 5000));
            }
        }

        public void UpdateDelayThreshold(double _) { }
    }
}
