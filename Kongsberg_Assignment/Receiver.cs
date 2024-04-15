
using System.Collections.Concurrent;

namespace Kongsberg_Assignment
{
    public class Receiver
    {
        public int ID { get; set; }
        public int SensorID { get; set; }
        public bool Active { get; set; }

        public int PollingInterval { get; set; }

        // TODO consider making a method that periodically clears hashsets.
        private HashSet<string> processedMessages = new HashSet<string>();

        public void Start(ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            if (Active)
            {
                Task.Run(() => ReceiveMessages(messagePool));
            }
        }

        //TODO implement a STOP function as in SensorSimulator

        async Task ReceiveMessages(ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            while (true)
            {
                if (messagePool.TryGetValue(SensorID, out ConcurrentQueue<string> messageQueue))
                {
                    if (messageQueue.TryPeek(out string message))
                    {
                        if (!processedMessages.Contains(message))
                        {
                            AnalyzeMessage(message);
                            processedMessages.Add(message);
                        }
                    }
                }
                // Consider decreasing polling interval if there was a warning, and increase it there were no warnings lately
                await Task.Delay(this.PollingInterval); // Polling interval
            }
        }

        void AnalyzeMessage(string message)
        {
            if (message == null)
            {
                WriteColoredLine($"Message from sensor {SensorID} was empty!", ConsoleColor.Red);
                return;
            }

            var sensorData = SensorMessage.Parse(message);

            var receiverMessage = $"Receiver {this.ID} received message: {message} from sensor {this.SensorID}";
            switch (sensorData.Quality)
            {
                case "Alarm":
                    WriteColoredLine(receiverMessage, ConsoleColor.Red);
                    break;
                case "Warning":
                    WriteColoredLine(receiverMessage, ConsoleColor.Yellow);
                    break;
                case "Normal":
                    WriteColoredLine(receiverMessage, ConsoleColor.Green);
                    break;
                default:
                    Console.WriteLine("Unknown quality status.");
                    break;
            }

        }

        /// <summary>
        /// I am using ANSI color coding (not Console.ForegroundColor) because it is faster and safe with async operations.
        /// </summary>
        void WriteColoredLine(string text, ConsoleColor color)
        {
            (int r, int g, int b) = GetRgbValues(color);
            Console.Write("\u001b[38;2;");
            Console.Write($"{r};{g};{b}m");
            Console.WriteLine(text);
            Console.Write("\u001b[39m");
        }

        (int r, int g, int b) GetRgbValues(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Red:
                    return (255, 0, 0);
                case ConsoleColor.Yellow:
                    return (255, 255, 0);
                case ConsoleColor.Green:
                    return (0, 255, 0);
                default:
                    return (255, 255, 255);
            }
        }
    }
}

