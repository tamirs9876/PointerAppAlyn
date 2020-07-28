using System;

namespace Alyn.Pointer.TobiiAgent
{
    public interface IAgentAnalyzer
    {
        void StartWatching(Action<double, double> recognizeMethod);

        void UpdateDelayThreshold(double delay);
    }
}