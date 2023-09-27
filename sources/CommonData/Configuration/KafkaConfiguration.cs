namespace CommonData.Configuration;

public record KafkaConfigurationConsumer
{
    public string? Topic { get; set; }
    public string? GroupId { get; set; }
}

public record KafkaConfigurationProducer
{
    public string? Topic { get; set; }
}

public record KafkaConfiguration
{
    public string? BootstrapServers { get; set; }
    public Dictionary<string, KafkaConfigurationConsumer>? Consumers { get; set; }
    public Dictionary<string, KafkaConfigurationProducer>? Producers { get; set; }
}