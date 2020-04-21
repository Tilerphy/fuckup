using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WakemeUP
{
    public partial class WakemeUp : Form
    {
        public NotifyIcon notifyIcon;
        public string Today;
        public delegate void StartJob();
        public Dictionary<string, bool> Clicked;
        private string CurrentStep { get; set; }
        public Button MainButton { get; set; }
        public WakemeUp()
        {
            Today = DateTime.Now.ToString("yyyy年MM月dd日 ") ;
            Clicked = new Dictionary<string, bool>();
            InitializeComponent();
            notifyIcon = new NotifyIcon();
            this.notifyIcon.Visible = true;
            this.notifyIcon.Icon = new Icon("icon\\chick.ico");
            
            this.Load += WakemeUp_Load;
            
        }

        private void WakemeUp_Load(object sender, EventArgs e)
        {
            MainButton = new Button();
            string[] customButton = File.Exists("config\\customButtonName.txt") ? File.ReadAllLines("config\\customButtonName.txt") :new string[] {"100","朕知道了","255,55,55" };
            MainButton.Text = customButton[1];
            MainButton.Font = new Font(MainButton.Font.FontFamily, int.Parse(customButton[0]), FontStyle.Bold);
            string[] rgb = customButton[2].Split(',');
            MainButton.ForeColor = Color.FromArgb(int.Parse(rgb[0].Trim()), int.Parse(rgb[1].Trim()), int.Parse(rgb[2].Trim()));
            MainButton.Height = this.Height;
            MainButton.Width = this.Width;
            MainButton.Click += MainButton_Click;
            this.Controls.Add(MainButton);
            Task.Run(new Action(Start));
            
        }

        private void Start() 
        {
            while (true)
            {

                this.Invoke(new StartJob(CreateIconInworkBar));
                Thread.Sleep(5000);
            }
        }

        private void MainButton_Click(object sender, EventArgs e)
        {
            if (!Clicked.ContainsKey(CurrentStep))
            {
                Clicked.Add(CurrentStep, true);
            }
            else
            {
                Clicked[CurrentStep] = true;
            }
            this.Visible = false;
            this.TopMost = false;
            this.WindowState = FormWindowState.Minimized;
            
            this.notifyIcon.BalloonTipText = string.Format("{0} 已标记", CurrentStep);
            this.notifyIcon.ShowBalloonTip(1000);
        }

        public void  CreateIconInworkBar() 
        {
            string waitingConfirmStep = string.Empty;
            JObject config = JObject.Parse(File.ReadAllText("config\\config.json"));
            string step = InWhichTime(config);
            if (!string.IsNullOrEmpty(step) &&!Clicked.ContainsKey(Today+step)) 
            {
                
                this.notifyIcon.BalloonTipText = config[step]["Message"].ToString();
                this.notifyIcon.BalloonTipTitle = Today + step;
                this.CurrentStep = Today + step;
                
                this.notifyIcon.ShowBalloonTip(3000);
                

                this.Visible = true;
                this.TopMost = true;
                this.WindowState = FormWindowState.Normal;
            }
            

            
        }

        private string InWhichTime(JObject config) 
        {
            foreach (JProperty jp in config.Properties()) 
            {
                string[] during = config[jp.Name]["During"].ToString().Split(",");
                TimeSpan startTime = new TimeSpan(int.Parse(during[0].Split(":")[0]), int.Parse(during[0].Split(":")[1]), 0);
                TimeSpan endTime = new TimeSpan(int.Parse(during[1].Split(":")[0]), int.Parse(during[1].Split(":")[1]), 0);
                TimeSpan currentTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
                if (currentTime <= endTime && currentTime >= startTime) 
                {
                    return jp.Name;
                }

            }
            return string.Empty;
        }
    }

}
