using System;
using System.Threading.Tasks;
using Alturos.Yolo;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Alyn.Pointer.DetectionService
{
    public class DetectionService : Detection.DetectionBase
    {
        private readonly ILogger<DetectionService> _logger;
        public DetectionService(ILogger<DetectionService> logger)
        {
            _logger = logger;
        }

        public override Task<Result> GetDetections(Payload request, ServerCallContext context)
        {
            using var yoloWrapper = new YoloWrapper(
                "C:\\Development\\PointerAppAlyn\\YOLOv3-Object-Detection-with-OpenCV\\profiling\\cfg\\yolov3.cfg",
                "C:\\Development\\PointerAppAlyn\\YOLOv3-Object-Detection-with-OpenCV\\profiling\\weights\\yolov3.weights",
                "C:\\Development\\PointerAppAlyn\\YOLOv3-Object-Detection-with-OpenCV\\profiling\\coco.names");
            var items = yoloWrapper.Detect(request.Image.ToByteArray());
            
            var result = new Result();
            foreach (var yoloItem in items)
            {
                Console.WriteLine(yoloItem.Type);
                result.Detections.Add(new DetectionResult
                {
                    Type = yoloItem.Type,
                    X = yoloItem.X,
                    Y = yoloItem.Y,
                    W = yoloItem.Width,
                    H = yoloItem.Height
                });
            }

            return Task.FromResult(result);
        }
    }
}
