using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace Alyn.Pointer.TobiiAgent
{
    public class TobiiAgentAnalyzer : IAgentAnalyzer
    {
        private readonly Host host;
        private readonly FixationDataStream stream;
        private double fixationThreshold = 1.5; // Threshold for the kids gaze time before sending the object to the manager for recognition.
        private bool sentForRecognition;

        public TobiiAgentAnalyzer()
        {
            this.sentForRecognition = false;
            this.host = new Host();
            this.stream = host.Streams.CreateFixationDataStream();
        }

        // This method registers the callbacks for fixation begin, during, and end.
        // On begin, timestamp is reset, during we check if the fixation time is passed the threshold and if the object was not sent for recognition before. If so, we run the method i_RecognizeMethod which is from the manager,
        // And responsible for recognizing the object in location x,y.
        public void StartWatching(Action<double, double> recognizeMethod)
        {
            // Because timestamp of fixation events is relative to the previous ones
            // only, we will store them in this variable.
            var fixationBeginTime = 0d;
            var lastX = 0d;
            var lastY = 0d;

            // On fixation begin
            stream.Next += (o, fixation) =>
            {
                // On the Next event, data comes as FixationData objects, wrapped in a StreamData<T> object.
                var fixationPointX = fixation.Data.X;
                var fixationPointY = fixation.Data.Y;

                switch (fixation.Data.EventType)
                {
                    case FixationDataEventType.Begin:
                        // reset the fixationBeginTime if the X,Y is outside range
                        var diffX = Math.Abs(lastX - fixation.Data.X);
                        var diffY = Math.Abs(lastY - fixation.Data.Y);
                        if (diffX > 50 || diffY > 50)
                        {
                            fixationBeginTime = fixation.Data.Timestamp;
                            lastX = fixation.Data.X;
                            lastY = fixation.Data.Y;
                        }
                        break;

                    case FixationDataEventType.Data:
                        var duration = (fixation.Data.Timestamp - fixationBeginTime) / 1000;
                        if (!sentForRecognition && duration >= fixationThreshold)
                        {
                            this.host.DisableConnection();
                            recognizeMethod.Invoke(fixation.Data.X, fixation.Data.Y);
                            this.host.EnableConnection();
                            sentForRecognition = true;
                        }
                        break;

                    case FixationDataEventType.End:
                        sentForRecognition = false;
                        break;

                    default:
                        throw new InvalidOperationException("Unknown fixation event type, which doesn't have explicit handling.");
                }
            };
        }

        public void UpdateDelayThreshold(double delay)
        {
            this.fixationThreshold = delay;
        }
    }
}
