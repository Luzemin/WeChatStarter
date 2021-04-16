using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WechatMore
{
    public partial class MainForm : Form
    {
        private string wechatPath = string.Empty;
        public MainForm()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            string count = countBox.Text;
            if (string.IsNullOrEmpty(wechatPath) || string.IsNullOrEmpty(count) || !int.TryParse(count, out int openCount))
            {
                return;
            }
            if (openCount < 1 || openCount > 100)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("-command \"");
            for (int i = 0; i < openCount; i++)
            {
                sb.Append("&'" + wechatPath + "';");
            }
            sb.Append("\"");
            string cmd = sb.ToString();

            Process.Start(new ProcessStartInfo("powershell.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = cmd
            });

            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //自动查找微信.exe
            //盲猜
            var drivers = DriveInfo.GetDrives().AsEnumerable();
            string programFiles = Environment.Is64BitOperatingSystem ? "Program Files (x86)" : "Program Files";
            foreach (var driver in drivers)
            {
                var path = driver.Name + programFiles + @"\Tencent\WeChat\WeChat.exe";
                if (File.Exists(path))
                {
                    wechatPath = path;
                    return;
                }
            }

            //从卸载列表中找
            string regPath = Environment.Is64BitOperatingSystem ? @"HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" :
                @"HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string cmd = "(Get-ItemProperty " + regPath + "* | where{$_.PSChildName -eq 'WeChat'}).InstallLocation";
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("powershell.exe")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Arguments = "-command " + cmd,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.Start();
            string outputResult = process.StandardOutput.ReadToEnd();
            string errorResult = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(outputResult))
            {
                wechatPath = outputResult.Trim() + "\\Wechat.exe";
            }
            else
            {
                label1.Visible = true;
                startBtn.Enabled = false;
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Start();
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                Close();
            }
        }
    }
}
