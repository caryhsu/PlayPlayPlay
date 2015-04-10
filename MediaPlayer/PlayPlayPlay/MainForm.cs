using System;
using System.Management;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;

namespace PlayPlayPlay
{
    public partial class MainForm : Form
    {

        public int PAUSE_AFTER_MSEC = 200;

        private List<String> serialPorts = new List<String>();
        private SerialPort mySerialPort;

        public MainForm()
        {
            InitializeComponent();
            InitialSerialPorts();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "Open";
            open.Filter = "All Files|*.*";
            try
            {
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    axWindowsMediaPlayer1.URL = open.FileName;
                    messageToolStripStatusLabel.Text = open.FileName;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WMPLib.IWMPControls3 controls = (WMPLib.IWMPControls3)axWindowsMediaPlayer1.Ctlcontrols;
            controls.pause();
        }

        private void InitialSerialPorts()
        {
            List<String> mySerialPorts = new List<String>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();
                foreach (string s in tList)
                {
                    mySerialPorts.Add(s);
                }
            }
            if (!ListEquals(this.serialPorts, mySerialPorts))
            {
                InitialSerialPortsMenus(mySerialPorts);
                this.serialPorts = mySerialPorts;
            }

        }

        private bool ListEquals(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < list1.Count; i++ )
            {
                string text1 = list1[i];
                string text2 = list2[i];
                if (!text1.Equals(text2)) return false;
            }
            return true;
        }

        private void InitialSerialPortsMenus(List<String> serialPorts)
        {
            portToolStripMenuItem.DropDownItems.Clear();
            foreach (string s in serialPorts)
            {
                ToolStripMenuItem portMenu = new ToolStripMenuItem(s);
                portMenu.CheckOnClick = true;
                portMenu.Click += new EventHandler(portMenu_Click);
                portToolStripMenuItem.DropDownItems.Add(portMenu);
            }
        }


        private void portMenu_Click(object sender, EventArgs e)
        {
            // Set the current clicked item to item
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            // Loop through all items in the subMenu and uncheck them but do check the clicked item
            foreach (ToolStripMenuItem tempItemp in ((ToolStripMenuItem)item.OwnerItem).DropDownItems)
            {
                if (tempItemp == item)
                    tempItemp.Checked = true;
                else
                    tempItemp.Checked = false;
            }

            Console.WriteLine("menu.Text=" + item.Text);
            initMySerialPort(item);
        }

        private void initialMySerialPort1()
        {
            foreach (ToolStripMenuItem tempItemp in portToolStripMenuItem.DropDownItems)
            {
                if (tempItemp.Checked)
                {
                    initMySerialPort(tempItemp);
                }
            }
        }

        private void initMySerialPort(ToolStripMenuItem item)
        {
            this.mySerialPort = new SerialPort("COM4");
            this.mySerialPort.BaudRate = 9600;
            this.mySerialPort.Parity = Parity.None;
            this.mySerialPort.StopBits = StopBits.One;
            this.mySerialPort.DataBits = 8;
            this.mySerialPort.Handshake = Handshake.None;
            this.mySerialPort.ReadBufferSize = 1024000;

            try
            {
                this.mySerialPort.Open();
            }
            catch (Exception ex)
            {
                item.Checked = false;
                this.mySerialPort = null;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 2) // Paused
            {
                //checkPumpTimer.Enabled = true;
                pauseTimer.Enabled = false;
            }
            else if (e.newState == 3) // Playing
            {
                //checkPumpTimer.Enabled = true;
                pauseAfter(PAUSE_AFTER_MSEC);
            }
            else
            {
                //checkPumpTimer.Enabled = true;
            }
        }

        private void pauseAfter(int interval)
        {
            pauseTimer.Enabled = false;
            pauseTimer.Interval = interval;
            pauseTimer.Enabled = true;
        }

        private void pauseTimer_Tick(object sender, EventArgs e)
        {
            pauseTimer.Enabled = false;
            pause();
        }

        private void pause()
        {
            WMPLib.IWMPControls3 controls = (WMPLib.IWMPControls3)axWindowsMediaPlayer1.Ctlcontrols;
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying) // Playing
            {
                controls.pause();
            }
        }

        private void play()
        {
            WMPLib.IWMPControls3 controls = (WMPLib.IWMPControls3)axWindowsMediaPlayer1.Ctlcontrols;
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused) // Paused
            {
                controls.play();
            }
        }

        private void pump()
        {
            play();
            pauseAfter(PAUSE_AFTER_MSEC);
        }

        private void checkPumpTimer_Tick(object sender, EventArgs e)
        {
            //InitialSerialPorts();
            //initialMySerialPort1();
            bool isPump = checkPump();
            if (isPump)
            {
                pump();
            }
        }

        private void serialPortDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            pump();
            Console.WriteLine("Data Received:" + indata);
        }

        private bool checkPump()
        {
            if (this.mySerialPort == null)
            {
                return false;
            }
            string indata = null;
            try
            {
                indata = this.mySerialPort.ReadExisting();
            }
            catch (Exception ex)
            {
                /**
                if (this.mySerialPort != null)
                {
                    try
                    {
                        this.mySerialPort.Open();
                    }
                    catch (Exception e)
                    {
                    }
                }
                 */
            }
            return indata == null || indata.Length > 0;
        }

        private void portToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.InitialSerialPorts();
        }

    }

}
