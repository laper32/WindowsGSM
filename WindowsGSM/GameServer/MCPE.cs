﻿using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO.Compression;
using System.Net;

namespace WindowsGSM.GameServer
{
    class MCPE
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private readonly string ServerID;

        public string Error;

        public const string FullName = "Minecraft Pocket Edition Server | PocketMine-MP";

        public string port = "19132";
        public string defaultmap = "world";
        public string maxplayers = "20";
        public string additional = "";

        public MCPE(string serverid)
        {
            ServerID = serverid;
        }

        public void CreateServerCFG(string serverName, string serverPort, string rcon_password)
        {
            string serverConfigPath = Functions.Path.GetServerFiles(ServerID) + @"\server.properties";

            File.Create(serverConfigPath).Dispose();

            using (TextWriter textwriter = new StreamWriter(serverConfigPath))
            {
                textwriter.WriteLine("#Properties Config file");
                textwriter.WriteLine("#Generate by WindowsGSM.exe");
                textwriter.WriteLine("language=eng");
                textwriter.WriteLine("motd=" + serverName);
                textwriter.WriteLine("server-name=" + serverName);
                textwriter.WriteLine("server-port=" + serverPort);
                textwriter.WriteLine("gamemode=0");
                textwriter.WriteLine("max-players=20");
                textwriter.WriteLine("spawn-protection=16");
                textwriter.WriteLine("white-list=off");
                textwriter.WriteLine("enable-query=on");
                textwriter.WriteLine("enable-rcon=off");
                textwriter.WriteLine("announce-player-achievements=on");
                textwriter.WriteLine("force-gamemode=off");
                textwriter.WriteLine("hardcore=off");
                textwriter.WriteLine("pvp=on");
                textwriter.WriteLine("difficulty=1");
                textwriter.WriteLine("generator-settings=");
                textwriter.WriteLine("level-name=world");
                textwriter.WriteLine("level-seed=");
                textwriter.WriteLine("level-type=DEFAULT");
                textwriter.WriteLine("rcon.password=" + rcon_password);
                textwriter.WriteLine("auto-save=on");
                textwriter.WriteLine("view-distance=8");
                textwriter.WriteLine("xbox-auth=on");
            }
        }

        public (Process Process, string Error, string Notice) Start()
        {
            string workingDir = Functions.Path.GetServerFiles(ServerID);

            string phpPath = workingDir + @"\bin\php\php.exe";
            if (!File.Exists(phpPath))
            {
                return (null, "php.exe not found (" + phpPath + ")", "");
            }

            string PMMPPath = workingDir + @"\PocketMine-MP.phar";
            if (!File.Exists(PMMPPath))
            {
                return (null, "PocketMine-MP.phar not found (" + PMMPPath + ")", "");
            }

            string serverConfigPath = workingDir + @"\server.properties";
            if (!File.Exists(serverConfigPath))
            {
                return (null, "server.properties not found (" + serverConfigPath + ")", "");
            }

            WindowsFirewall firewall = new WindowsFirewall("php.exe", phpPath);
            if (!firewall.IsRuleExist())
            {
                firewall.AddRule();
            }

            Process p = new Process();
            p.StartInfo.WorkingDirectory = workingDir;
            p.StartInfo.FileName = phpPath;
            p.StartInfo.Arguments = @"-c bin\php PocketMine-MP.phar";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            p.Start();

            return (p, "", "");
        }

        public async Task<bool> Stop(Process p)
        {
            SetForegroundWindow(p.MainWindowHandle);
            SendKeys.SendWait("stop");
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("{ENTER}");

            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

            int attempt = 0;
            while (attempt++ < 10)
            {
                if (p != null)
                {
                    SetForegroundWindow(p.MainWindowHandle);
                    SendKeys.SendWait("{ENTER}");

                    if (p.HasExited)
                    {
                        return true;
                    }
                }

                await Task.Delay(1000);
            }

            return false;
        }

        public async Task<bool> Install()
        {
            string serverFilesPath = Functions.Path.GetServerFiles(ServerID);

            //Download PHP-7.2-Windows-x64
            string filename = "PHP-7.3-Windows-x64.zip";
            string installer = "https://jenkins.pmmp.io/job/PHP-7.3-Aggregate/lastSuccessfulBuild/artifact/PHP-7.3-Windows-x64.zip";

            string PHPzipPath = Path.Combine(serverFilesPath, filename);
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += ExtractPHP;
                webClient.DownloadFileAsync(new Uri(installer), PHPzipPath);
            }
            catch
            {
                Error = "Fail to download " + filename;
                return false;
            }

            string PHPPath = MainWindow.WGSM_PATH + @"\servers\" + ServerID + @"\serverfiles\bin\php\php.exe";
            bool isDownloaded = false;
            while (!isDownloaded)
            {
                if (!File.Exists(PHPzipPath) && File.Exists(PHPPath))
                {
                    isDownloaded = true;
                    break;
                }

                await Task.Delay(1000);
            }

            //Download PocketMine-MP.phar
            installer = "https://jenkins.pmmp.io/job/PocketMine-MP/lastSuccessfulBuild/artifact/PocketMine-MP.phar";
            string PMMPPath = serverFilesPath + @"\PocketMine-MP.phar";
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileAsync(new Uri(installer), PMMPPath);
            }
            catch
            {
                Error = "Fail to download PocketMine-MP.phar";
                return false;
            }

            isDownloaded = false;
            while (!isDownloaded)
            {
                if (File.Exists(PMMPPath))
                {
                    isDownloaded = true;
                    break;
                }

                await Task.Delay(1000);
            }

            return true;
        }

        private async void ExtractPHP(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string installPath = MainWindow.WGSM_PATH + @"\servers\" + ServerID + @"\serverfiles";
            string zipPath = installPath + @"\PHP-7.3-Windows-x64.zip";

            if (File.Exists(zipPath))
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, installPath));
                await Task.Run(() => File.Delete(zipPath));
            }
        }

        public async Task<bool> Update()
        {
            //Download PocketMine-MP.phar
            string installer = "https://jenkins.pmmp.io/job/PocketMine-MP/lastSuccessfulBuild/artifact/PocketMine-MP.phar";
            string PMMPPath = MainWindow.WGSM_PATH + @"\servers\" + ServerID + @"\serverfiles\PocketMine-MP.phar";
            
            if (File.Exists(PMMPPath))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        File.Delete(PMMPPath);
                    }
                    catch
                    {
                        
                    }
                });

                if (File.Exists(PMMPPath))
                {
                    Error = "Fail to delete PocketMine-MP.phar";
                    return false;
                }
            }

            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileAsync(new Uri(installer), PMMPPath);
            }
            catch
            {
                Error = "Fail to download PocketMine-MP.phar";
                return false;
            }

            bool isDownloaded = false;
            while (!isDownloaded)
            {
                if (File.Exists(PMMPPath))
                {
                    isDownloaded = true;
                    break;
                }

                await Task.Delay(1000);
            }

            return isDownloaded;
        }
    }
}
