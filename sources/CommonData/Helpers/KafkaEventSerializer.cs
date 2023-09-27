using System.Text.Json;

using Confluent.Kafka;

namespace CommonData.Helpers;

public class KafkaEventSerializer<T> : IDeserializer<T>, ISerializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        => JsonSerializer.Deserialize<T>(data.ToArray()) ?? default!;

    public byte[] Serialize(T data, SerializationContext context)
    {
        using var ms = new MemoryStream();
        string jsonString = JsonSerializer.Serialize(data);
        var writer = new StreamWriter(ms);

        writer.Write(jsonString);
        writer.Flush();
        ms.Position = 0;

        return ms.ToArray();
    }
}