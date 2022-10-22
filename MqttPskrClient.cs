using System.Text;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using MQTTnet.Packets;
using Newtonsoft.Json;
using System.IO;
using VE3NEA.PskrDxClusterService;

namespace VE3NEA
{
  internal class MqttSpot
  {
    // the format of the json messages from the mqtt server
    public long sq { get; set; }
    public long f { get; set; }
    public string md { get; set; }
    public int rp { get; set; }
    public int t { get; set; }
    public string sc { get; set; }
    public string sl { get; set; }
    public string rc { get; set; }
    public string rl { get; set; }
    public int sa { get; set; }
    public int ra { get; set; }
    public string b { get; set; }
  }



  internal class SpotReceivedEventArgs : EventArgs
  {
    internal MqttSpot mqttSpot;
    internal SpotReceivedEventArgs(MqttSpot mqttSpot) { this.mqttSpot = mqttSpot; }
  }



  internal class MqttSettings
  {
    public bool ArchiveSpots { get; set; } = false;
    public string ArchiveFolder { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SpotArchive");

    public string Host { get; set; } = "mqtt.pskreporter.info";
    public int Port { get; set; } = 1883;

    public string[] Topics { get; set; }  = new[] {
      "pskr/filter/v2/6m/+/+/+/+/+/1/+",
      "pskr/filter/v2/6m/+/+/+/+/+/+/1",
      "pskr/filter/v2/6m/+/+/+/+/+/291/+",
      "pskr/filter/v2/6m/+/+/+/+/+/+/291"
    };
  }



  internal class MqttPskrClient
  {
    private readonly MqttFactory factory;
    private readonly IManagedMqttClient client;
    private readonly ManagedMqttClientOptions options;
    private readonly ICollection<MqttTopicFilter> filters;
    private readonly string ArchiveFolder;

    internal bool ArchiveSpots;


    internal event EventHandler<SpotReceivedEventArgs>? SpotReceived;
    internal event EventHandler? ConnectionChanged;
    internal bool Connected { get; private set; }

    internal MqttPskrClient(MqttSettings settings)
    {
      ArchiveSpots = settings.ArchiveSpots;
      ArchiveFolder = settings.ArchiveFolder;
      if (ArchiveSpots) Directory.CreateDirectory(ArchiveFolder);

      factory = new MqttFactory();

      client = factory.CreateManagedMqttClient();
      client.ApplicationMessageReceivedAsync += MessageReceivedHandler;
      client.ConnectedAsync += (e) => OnConnectionChanged(true);
      client.DisconnectedAsync += (e) => OnConnectionChanged(false);

      var clientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer(settings.Host, settings.Port)
        .Build();

      options = new ManagedMqttClientOptionsBuilder()
        .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
        .WithClientOptions(clientOptions)
        .Build();

      filters = settings.Topics.Select(t => new MqttTopicFilter { Topic = t }).ToList();
    }

    internal async Task Start()
    {
      await client.StartAsync(options);
      await client.SubscribeAsync(filters);
    }

    internal async Task Stop()
    {
      await client.StopAsync();
    }

    private Task OnConnectionChanged(bool connected)
    {
      Connected = connected;
      ConnectionChanged?.Invoke(this, new EventArgs());
      return Task.CompletedTask;
    }

    private Task MessageReceivedHandler(MqttApplicationMessageReceivedEventArgs arg)
    {
      string json = Encoding.ASCII.GetString(arg.ApplicationMessage.Payload);
      MqttSpot spot = JsonConvert.DeserializeObject<MqttSpot>(json);

      SpotReceived?.Invoke(this, new SpotReceivedEventArgs(spot));

      if (ArchiveSpots) SaveSpot(json);

      return Task.CompletedTask;
    }

    private void SaveSpot(string json)
    {
      string filePath = Path.Combine(ArchiveFolder, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".json");
      using (StreamWriter wr = File.AppendText(filePath)) wr.WriteLine(json);
    }
  }
}