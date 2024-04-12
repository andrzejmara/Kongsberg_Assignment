
using System.Collections.Concurrent;

namespace Kongsberg_Assignment
{
    public class Receiver
    {
        public int ID { get; set; }
        public int SensorID { get; set; }
        public bool Active { get; set; }

        public void Start(ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            if (Active)
            {
                Task.Run(() => ReceiveMessages(messagePool));
            }
        }

        async Task ReceiveMessages(ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            while (true)
            {
                // Check if there are messages for this receiver's sensor
                if (messagePool.TryGetValue(this.ID, out ConcurrentQueue<string> messageQueue))
                {
                    // Dequeue and process messages
                    while (messageQueue.TryDequeue(out string message))
                    {
                        AnalyzeMessage(message);
                    }
                }

                // Simulate receiver polling interval
                await Task.Delay(100);
            }
        }

        void AnalyzeMessage(string message)
        {
            if (message == null)
            {
                WriteColoredLine($"Message from sensor {SensorID} was empty!", ConsoleColor.White, ConsoleColor.Red);
                return;
            }

            var sensorData = SensorData.Parse(message);
            switch (sensorData.Quality)
            {
                case "Alarm":
                    WriteColoredLine($"Receiver {this.ID} received message: {message}", ConsoleColor.Red);
                    break;
                case "Warning":
                    WriteColoredLine($"Receiver {this.ID} received message: {message}", ConsoleColor.Yellow);
                    break;
                case "Normal":
                    WriteColoredLine($"Receiver {this.ID} received message: {message}", ConsoleColor.Green);
                    break;
                default:
                    Console.WriteLine("Unknown quality status.");
                    break;
            }

        }

        void WriteColoredLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            ConsoleColor originalForeground = Console.ForegroundColor;
            ConsoleColor originalBackground = Console.BackgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
        }
    }
}

