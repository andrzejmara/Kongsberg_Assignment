using System;
using System.Collections.Concurrent;

namespace Kongsberg_Assignment
{
    public class SensorSimulator
    {
        private readonly Sensor Sensor;
        private readonly ThreadLocal<Random> _random;
        //TODO consider creating a method that periodically cleans the pool.
        private readonly ConcurrentDictionary<int, ConcurrentQueue<string>> _messagePool;
        private readonly ConcurrentDictionary<int, List<int>> _sensorReceiverMap;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _messageProcessingTask;

        public SensorSimulator(Sensor sensor, ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            Sensor = sensor;
            // Just to make sure, the random generation is thread safe.
            _random = new ThreadLocal<Random>(() =>
            {
                return new Random(Guid.NewGuid().GetHashCode());
            });

            _messagePool = messagePool;
            _cancellationTokenSource = new CancellationTokenSource();
            _messageProcessingTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token));
            StartMessageGeneration();
        }

        private void StartMessageGeneration()
        {
            Task.Run(() => GenerateAndEnqueueMessages(_cancellationTokenSource.Token));
        }

        private async Task GenerateAndEnqueueMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int value = GenerateRandomValue(Sensor.MinValue, Sensor.MaxValue);
                string quality = ClassifyValue(value, Sensor.MinValue, Sensor.MaxValue);
                string message = new SensorMessage(Sensor.ID, Sensor.Type, value, quality).ToText();
                Console.WriteLine($"Sensor {Sensor.ID} sent a message: {message}");
                _messagePool.AddOrUpdate(Sensor.ID, new ConcurrentQueue<string>(), (id, queue) => queue).Enqueue(message);
                // Calculate Herz to seconds
                var period = TimeSpan.FromSeconds(1.0 / Sensor.Frequency);
                await Task.Delay(period);
            }
        }

        private int GenerateRandomValue(int minValue, int maxValue)
        {
            return _random.Value.Next(minValue, maxValue + 1);
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
                if (_messagePool.TryGetValue(this.Sensor.ID, out ConcurrentQueue<string> messageQueue))
                {
                    while (messageQueue.TryDequeue(out string message))
                    {
                        await TransmitMessageAsync(message);
                    }
                }
                await Task.Delay(100); // Polling interval
            }
        }

        private async Task TransmitMessageAsync(string message)
        {
            // Simulate message transmission delay (e.g., network latency)
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            // Wait for message processing task to complete
            _messageProcessingTask.Wait();
        }
    }


}
