namespace Kongsberg_Assignment
{
    /// <summary>
    /// Class for creation Sensor Message
    /// </summary>
    public class SensorMessage
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Value { get; set; }
        public string Quality { get; set; }

        public SensorMessage(int id, string type, int value, string quality)
        {
            Id = id;
            Type = type;
            Value = value;
            Quality = quality;
        }

        public string ToText()
        {
            return $"$FIX, {Id}, {Type}, {Value}, {Quality}*";
        }

        public static SensorMessage Parse(string message)
        {
            string[] parts = message.Split(',');

            if (parts.Length != 5 || !parts[0].StartsWith("$FIX"))
            {
                throw new ArgumentException("Invalid message format.");
            }
            int id = int.Parse(parts[1].Trim());
            string type = parts[2].Trim();
            int value = int.Parse(parts[3].Trim());
            string quality = parts[4].Trim().Trim('*');
            return new SensorMessage(id, type, value, quality);
        }
    }
}
