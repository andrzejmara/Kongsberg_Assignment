using System.Collections.Concurrent;

namespace Kongsberg_Assignment
{
    public class SensorSimulator
    {
        private readonly List<Sensor> _sensors;
        private readonly Random _random;
        private readonly ConcurrentDictionary<int, ConcurrentQueue<string>> _messagePool;
        private readonly ConcurrentDictionary<int, int> _sensorReceiverMap;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _messageProcessingTask;

        public SensorSimulator(List<Sensor> sensors, ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool, ConcurrentDictionary<int, int> sensorReceiverMap)
        {
            _sensors = sensors;
            _random = new Random();
            _messagePool = messagePool;
            _sensorReceiverMap = sensorReceiverMap;
            _cancellationTokenSource = new CancellationTokenSource();
            _messageProcessingTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token));
            StartMessageGeneration();
        }

        private void StartMessageGeneration()
        {
            foreach (var sensor in _sensors)
            {
                Task.Run(() => GenerateAndEnqueueMessages(sensor, _cancellationTokenSource.Token));
            }
        }

        private async Task GenerateAndEnqueueMessages(Sensor sensor, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int value = GenerateRandomValue(sensor.MinValue, sensor.MaxValue);
                string quality = ClassifyValue(value, sensor.MinValue, sensor.MaxValue);
                string message = new SensorData(sensor.ID, sensor.Type, value, quality).ToMessage();
                if (sensor.ID == 1)
                {
                    Console.WriteLine($"Sensor 1 sent a message: {message}");
                }
                _messagePool.AddOrUpdate(sensor.ID, new ConcurrentQueue<string>(), (id, queue) => queue).Enqueue(message);
                // Calculate Herz to seconds
                var period = TimeSpan.FromSeconds(1.0 / sensor.Frequency);
                await Task.Delay(period);
            }
        }

        private int GenerateRandomValue(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue + 1);
        }

        private string ClassifyValue(int value, int minValue, int maxValue)
        {
            double range = (maxValue - minValue) * 0.1;
            double lowerAlarm = minValue + range;
            double upperAlarm = maxValue - range;
            double lowerWarning = minValue + range * 1.5;
            double upperWarning = maxValue - range * 1.5;

            if (value < lowerAlarm || value > upperAlarm)
                return "Alarm";
            else if (value < lowerWarning || value > upperWarning)
                return "Warning";
            else
                return "Normal";
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var sensorId in _sensorReceiverMap.Keys)
                {
                    if (_messagePool.TryGetValue(sensorId, out ConcurrentQueue<string> messageQueue))
                    {
                        while (messageQueue.TryDequeue(out string message))
                        {
                            await TransmitMessageAsync(message);
                        }
                    }
                }
                await Task.Delay(100); // Polling interval
            }
        }

        private async Task TransmitMessageAsync(string message)
        {
            // Simulate message transmission delay (e.g., network latency)
            await Task.Delay(TimeSpan.FromSeconds(0.1));

            Console.WriteLine(message); // For demonstration purpose
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            // Wait for message processing task to complete
            _messageProcessingTask.Wait();
        }
    }


}
