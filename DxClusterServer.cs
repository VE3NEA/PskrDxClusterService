using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.PskrDxClusterService
{
  internal class DxClusterServer : TelnetServer
  {
    internal DxClusterServer() : base()
    {
      EnterNameMessage = "Please enter your callsign: ";
      InvalidNameMessage = "Invalid callsign";
      ServerNameMessage = "PSKReporter DX Cluster Server";
      Port = 7309;
    }

    public override string GetPrompt(TelnetClientEntry client)
    {
      return EOL + client.UserName + " de " + ServerNameMessage + " >";
    }

    protected override bool IsValidName(string name)
    {
      // todo: check if the name is actually a valid callsign
      return base.IsValidName(name);
    }
  }
}
