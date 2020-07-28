using System;
using System.Threading.Tasks;
using Alyn.Pointer.Common;

namespace Alyn.Pointer.TobiiAgent
{
    public class MockAgentAnalyzer : IAgentAnalyzer
    {
        private readonly IObservable<(double x, double y)> clicks;

        public MockAgentAnalyzer(IObservable<(double x, double y)> clicks)
        {
            this.clicks = clicks;
        }

        public async void StartWatching(Action<double, double> recognizeMethod)
        {
            if (this.clicks != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                this.clicks.Subscribe(value =>
                {
                    recognizeMethod(value.x, value.y);
                },
                ex => tcs.SetException(ex),
                () => tcs.SetResult(true));

                await tcs.Task;
            }
            else
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
        }

        public void UpdateDelayThreshold(double _) { }
    }
}
