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
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _messageProcessingTask;
        private TimeSpan _period;

        public SensorSimulator(Sensor sensor, ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool)
        {
            Sensor = sensor;
            // Just to make sure, the random generation is thread safe.
            _random = new ThreadLocal<Random>(() =>
            {
                return new Random(Guid.NewGuid().GetHashCode());
            });
            // Calculate Herz to seconds
            _period = TimeSpan.FromSeconds(1.0 / Sensor.Frequency);
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
                _messagePool.AddOrUpdate(Sensor.ID, new ConcurrentQueue<string>(), (id, queue) => queue).Enqueue(message);
                Console.WriteLine($"Sensor {Sensor.ID} sent a message: {message}");
                await Task.Delay(_period);
            }
        }

        private int GenerateRandomValue(int minValue, int maxValue)
        {
            return _random.Value.Next(minValue, maxValue + 1);
        }

        private double _alarmTreshold = 0.1;
        private double _warningTreshold = 1.5;

        private string ClassifyValue(int value, int minValue, int maxValue)
        {
            double range = (maxValue - minValue) * _alarmTreshold;
            double lowerAlarm = minValue + range;
            double upperAlarm = maxValue - range;
            double lowerWarning = minValue + range * _warningTreshold;
            double upperWarning = maxValue - range * _warningTreshold;

            if (value < lowerAlarm || value > upperAlarm)
                return "Alarm";
            else if (value < lowerWarning || value > upperWarning)
                return "Warning";
            else
                return "Normal";
        }

        ///Simulate sending the message
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
                await Task.Delay(_period); // Polling interval
            }
        }

        private async Task TransmitMessageAsync(string message)
        {
            // Simulate message transmission delay (e.g., network latency)
            await Task.Delay(TimeSpan.FromSeconds(0.01));
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            // Wait for message processing task to complete
            _messageProcessingTask.Wait();
        }
    }


}
