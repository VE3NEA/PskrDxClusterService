using Core;

namespace VE3NEA.PskrDxClusterService
{
  internal class PskrDxClusterServer
  {
    private readonly Settings settings = new();
    private readonly DxClusterServer dxClusterServer = new();
    private readonly MqttPskrClient mqttClient;

    internal PskrDxClusterServer()
    {
      settings.LoadFromFile();

      dxClusterServer.Port = settings.TelnetPort;
      dxClusterServer.ClientConnected += ClientChangeHandler;
      dxClusterServer.ClientDisconnected += ClientChangeHandler;
     
      mqttClient = new MqttPskrClient(settings.Mqtt);
      mqttClient.SpotReceived += SpotReceivedHandler;
    }

    internal async Task RunAsync(CancellationToken stoppingToken)
    {
      try
      {
        if (settings.TelnetServerEnabled) dxClusterServer.Start();
        if (settings.Mqtt.ArchiveSpots) await mqttClient.Start();

        await stoppingToken;
      }
      finally
      {
        if (mqttClient != null) await mqttClient.Stop();
        dxClusterServer.Stop();
      }
    }

    private async void ClientChangeHandler(object? sender, ClientEventArgs e)
    {
      // disable mqtt if not archiving and no telnet clients connected
      if (!settings.Mqtt.ArchiveSpots)
        if (dxClusterServer.Clients.Any() && !mqttClient.Connected)
          await mqttClient.Start();
        else if (!dxClusterServer.Clients.Any() && mqttClient.Connected)
          await mqttClient.Stop();
    }

    private void SpotReceivedHandler(object? sender, SpotReceivedEventArgs e)
    {
      string dxClusterSpot = MqttSpotToDxClusterSpot(e.mqttSpot);
      dxClusterServer.SendTextToAll(dxClusterSpot + dxClusterServer.EOL);
    }

    // generate spot in the format compatible with telnet://dm4x.ddns.net:8500
    // "DX de F4JNW:     14075.2  LZ3RG        JN18bc<>KN12    FT8       -13  0513Z"
    private string MqttSpotToDxClusterSpot(MqttSpot spot)
    {
      string deCall = $"DX de {spot.rc}:";
      string spotter_square = spot.rl.Length > 6 ? spot.rl.Substring(0, 6) : spot.rl;
      string dx_square = spot.sl.Length > 6 ? spot.sl.Substring(0, 6) : spot.sl;
      string squares = $"{spotter_square}<>{dx_square}";
      string time = DateTimeOffset.FromUnixTimeSeconds(spot.t).UtcDateTime.ToString("HHmm");

      // frequency in kHz with 1 decimal digit
      double khz = (spot.f / 100) / 10f;

      return
        $"{deCall,-16}" +
        $"{khz,8:0.0}  " +
        $"{spot.sc,-13}" +
        $"{squares,-16}" +
        $"{spot.md,-10}" +
        $"{spot.rp,3}  " +
        $"{time}Z";
    }
  }
}