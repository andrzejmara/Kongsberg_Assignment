﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Kongsberg_Assignment
{


    class Program
    {
        static async Task Main(string[] args)
        {

            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string sensorConfigFilePath = Path.Combine(exeDirectory, "config", "sensorConfig.json");
            string sensorConfig = File.ReadAllText(sensorConfigFilePath);

            string receiverConfigFilePath = Path.Combine(exeDirectory, "config", "receiverConfig.json");
            string receiverConfig = File.ReadAllText(receiverConfigFilePath);

            //cool that .net 8 has its own serializer
            List<Sensor> sensors = JsonSerializer.Deserialize<SensorConfig>(sensorConfig).Sensors;
            List<Receiver> receivers = JsonSerializer.Deserialize<ReceiverConfig>(receiverConfig).Receivers;

            // Create a message pool to store messages
            ConcurrentDictionary<int, ConcurrentQueue<string>> messagePool = new ConcurrentDictionary<int, ConcurrentQueue<string>>();

            // Create a sensor-receiver mapping
            ConcurrentDictionary<int, int> sensorReceiverMap = new ConcurrentDictionary<int, int>();
            foreach (var receiver in receivers)
            {
                if (receiver.Active)
                {
                    sensorReceiverMap.TryAdd(receiver.SensorID, receiver.ID);
                }
            }


            // Create a SensorSimulator instance
            var sensorSimulator = new SensorSimulator(sensors, messagePool, sensorReceiverMap);

            // Start receivers
            foreach (var receiver in receivers)
            {
                receiver.Start(messagePool);
            }

            // Keep the program running for demonstration purpose
            await Task.Delay(TimeSpan.FromSeconds(30));

            sensorSimulator.Stop();
        }
    }

    public class SensorConfig
    {
        public required List<Sensor> Sensors { get; set; }
    }

    public class ReceiverConfig
    {
        public required List<Receiver> Receivers { get; set; }
    }
}