using System;

namespace TobiiAgent
{
    public interface IAgentAnalyzer
    {
        void UpdateDelayThreshold(double delay);
        void StartWatching(Action<double, double> recognizeMethod);
    }
}