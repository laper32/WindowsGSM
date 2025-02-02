﻿using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace WindowsGSM.Discord
{
    class Webhook 
    {
        private readonly string WebhookUrl;

        public Webhook(string webhookurl)
        {
            WebhookUrl = webhookurl;
        }

        public async Task<bool> Send(string serverid, string servergame, string serverstatus, string servername, string serverip, string serverport)
        {
            if (string.IsNullOrWhiteSpace(WebhookUrl))
            {
                return false;
            }

            string color;
            switch (serverstatus)
            {
                case "Started":
                    color = "65280"; break; //Green
                case "Stopped":
                    color = "16755200"; break; //Orange
                case "Restarted":
                    color = "65535"; break; //Cyan
                case "Crashed":
                    color = "16711680"; break; //Red
                default:
                    color = "16777215"; break;
            }

            string status = serverstatus;
            switch (serverstatus)
            {
                case "Started":
                    status += " :ok:"; break;
                case "Stopped":
                    status += " :octagonal_sign:"; break;
                case "Restarted":
                    status += " :arrows_counterclockwise:"; break;
                case "Crashed":
                    status += " :warning:"; break;
            }

            string gameicon = @"https://github.com/BattlefieldDuck/WindowsGSM/blob/master/WindowsGSM/Images/";
            switch (servergame)
            {
                case (GameServer.CSGO.FullName):
                    gameicon += @"games/csgo.png?raw=true"; break;
                case (GameServer.GMOD.FullName):
                    gameicon += @"games/gmod.png?raw=true"; break;
                case (GameServer.TF2.FullName):
                    gameicon += @"games/tf2.png?raw=true"; break;
                case (GameServer.MCPE.FullName):
                    gameicon += @"games/mcpe.png?raw=true"; break;
                case (GameServer.RUST.FullName):
                    gameicon += @"games/rust.png?raw=true"; break;
                case (GameServer.CS.FullName):
                    gameicon += @"games/cs.png?raw=true"; break;
                case (GameServer.CSCZ.FullName):
                    gameicon += @"games/cscz.png?raw=true"; break;
                case (GameServer.HL2DM.FullName):
                    gameicon += @"games/hl2dm.png?raw=true"; break;
                case (GameServer.L4D2.FullName):
                    gameicon += @"games/l4d2.png?raw=true"; break;
                default:
                    gameicon += @"windowsgsm.png?raw=true"; break;
            }

            string time = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.mssZ");

            string wgsmPath = "https://github.com/BattlefieldDuck/WindowsGSM/blob/master/WindowsGSM/Images/windowsgsm.png?raw=true";
            string json = @"
            {
                ""username"": ""WindowsGSM"",
                ""avatar_url"": """ + wgsmPath  + @""",
                ""embeds"": [
                {
                    ""title"": ""Status (ID: " + serverid + @")"",
                    ""type"": ""rich"",
                    ""description"": """ + status + @""",
                    ""color"": " + color + @",
                    ""fields"": [
                    {
                        ""name"": ""Game Server"",
                        ""value"": """ + servergame + @"""
                    },
                    {
                        ""name"": ""Server IP:Port"",
                        ""value"": """ + serverip + ":"+ serverport + @"""
                    }],
                    ""author"": {
                        ""name"": """ + servername + @""",
                        ""icon_url"": """ + gameicon + @"""
                    },
                    ""footer"": {
                        ""text"": ""WindowsGSM - Alert"",
                        ""icon_url"": """ + wgsmPath + @"""
                    },
                    ""timestamp"": """ + time + @"""
                }]
            }";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var httpResponse = await httpClient.PostAsync(WebhookUrl, content);

                    if (httpResponse.Content != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }
        }
    }
}
