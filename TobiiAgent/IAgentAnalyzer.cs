using System;

namespace TobiiAgent
{
    public interface IAgentAnalyzer
    {
        void StartWatching(Action<double, double> recognizeMethod);

        void UpdateDelayThreshold(double delay);
    }
}