using Newtonsoft.Json;

namespace VE3NEA.PskrDxClusterService
{
  internal class Settings
  {
    public static readonly string FileName = 
      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.json");

    public int TelnetPort { get; set; } = 7309;
    public bool TelnetServerEnabled { get; set; } = true;
    public MqttSettings Mqtt { get; set; } = new();

    public void LoadFromFile()
    {
      if (File.Exists(FileName)) 
        JsonConvert.PopulateObject(File.ReadAllText(FileName), this);
      else 
        SaveToFile();
    }

    public void SaveToFile()
    {
      File.WriteAllText(FileName, JsonConvert.SerializeObject(this));
    }
  }
}