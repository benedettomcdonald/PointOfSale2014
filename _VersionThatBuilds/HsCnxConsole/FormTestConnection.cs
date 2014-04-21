using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using HsProperties;

namespace HsCnxConsole
{
    public class FormTestConnection : Form
    {
        private bool flag = true;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox8;
        private HsBackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ProgressBar progressBar1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTestConnection));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new HsBackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.button1.Font = new System.Drawing.Font("Lucida Bright", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(56, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(298, 90);
            this.button1.TabIndex = 0;
            this.button1.Text = "TEST CONNECTION";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(334, 319);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "OK";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(11, 119);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(220, 16);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "Ready to Test...";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(11, 169);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(220, 16);
            this.textBox3.TabIndex = 6;
            this.textBox3.Text = "Connect to soap - ";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.Location = new System.Drawing.Point(237, 119);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(138, 16);
            this.textBox4.TabIndex = 7;
            this.textBox4.Text = "...";
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox6.Location = new System.Drawing.Point(237, 169);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(100, 16);
            this.textBox6.TabIndex = 9;
            this.textBox6.Text = "...";
            this.textBox6.TextChanged += new System.EventHandler(this.textBox6_TextChanged);
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox7.Location = new System.Drawing.Point(11, 211);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(220, 16);
            this.textBox7.TabIndex = 10;
            this.textBox7.Text = "Call Webservices - ";
            // 
            // textBox8
            // 
            this.textBox8.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox8.Location = new System.Drawing.Point(237, 211);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(100, 16);
            this.textBox8.TabIndex = 11;
            this.textBox8.Text = "...";
            this.textBox8.TextChanged += new System.EventHandler(this.textBox8_TextChanged);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new HsCnxConsole.HsBackgroundWorker.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new HsCnxConsole.HsBackgroundWorker.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new HsCnxConsole.HsBackgroundWorker.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(57)))), ((int)(((byte)(14)))));
            this.progressBar1.Location = new System.Drawing.Point(47, 262);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(307, 23);
            this.progressBar1.TabIndex = 12;
            this.progressBar1.Visible = false;
            // 
            // FormTestConnection
            // 
            this.ClientSize = new System.Drawing.Size(421, 354);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormTestConnection";
            this.Text = "HSC - Test Connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public FormTestConnection()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;

            FormData data = new FormData();
            data.overallStatus = true;
            data.hitSoap = false;
            data.callWss = false;

            backgroundWorker1.RunWorkerAsync(data);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(5);
            FormData data = e.Argument as FormData;

            HttpWebRequest req = null;
            Boolean useSSL = Properties.UseSSL;
            backgroundWorker1.ReportProgress(33);

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 3000);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);

            IPHostEntry iph;
            IPEndPoint ipe;
            try
            {
                //here we use useSSL to indicate that this is a prod build (ssl on)
                //or a test build pointed at a QA or dev box (ssl off)
                if (useSSL)
                {
                    iph = Dns.Resolve("soap.hotschedules.com");
                    //ipe = new IPEndPoint("soap.hotschedules.com", 8020);
                    //s.Connect("soap.hotschedules.com", 8020);
                    //data.hitSoap = true;
                }
                else
                {
                    iph = Dns.Resolve("" + Properties.BaseURL);
                    //s.Connect("" + Properties.BaseURL, 8020);
                    //data.hitSoap = true;
                }

                foreach (IPAddress address in iph.AddressList)
                {
                    ipe = new IPEndPoint(address, 8020);
                    s.Connect(ipe);

                    if (s.Connected)
                    {
                        data.hitSoap = true;
                    }
                }
            }
            catch (Exception ex)
            {
                data.hitSoap = false;
                data.overallStatus = false;
            }
            finally
            {
                s.Close();
            }

            backgroundWorker1.ReportProgress(66);

            //test Wss call
            string ret = "";

            HttpWebRequest req2 = null;
            if (useSSL)
            {
                req2 = WebRequest.Create("https://" + Properties.BaseURL + "/hsws/services/ClientSettingsWss?GetClientDetails") as HttpWebRequest;
            }
            else
            {
                req2 = WebRequest.Create("http://" + Properties.BaseURL + ":8020/hsws/services/ClientSettingsWss?GetClientDetails") as HttpWebRequest;
            }
            //req2.Proxy = null;
            req2.Method = "GET";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)(req2.GetResponse());
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    data.callWss = true;
                }
                else
                {
                    data.callWss = false;
                    data.overallStatus = false;
                }
            }
            catch (Exception ex)
            {
                data.callWss = false;
                data.overallStatus = false;
            }
            
            backgroundWorker1.ReportProgress(100);

            e.Result = data;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FormData data = e.Result as FormData;

            if (data.overallStatus == true)
            {
                textBox4.ForeColor = Color.Green;
                textBox1.Text = "Test Results - ";
                textBox4.Text = "CONNECTION OK! =D";
            }
            else
            {
                textBox4.ForeColor = Color.Red;
                textBox1.Text = "Test Results - ";
                textBox4.Text = "CONNECTION FAIL! =[";
            }

            if (data.hitSoap == true)
            {
                textBox6.ForeColor = Color.Green;
                textBox6.Text = "SUCCESS";
            }
            else
            {
                textBox6.ForeColor = Color.Red;
                textBox6.Text = "FAIL";
            }

            if (data.callWss == true)
            {
                textBox8.ForeColor = Color.Green;
                textBox8.Text = "SUCCESS";
            }
            else
            {
                textBox8.ForeColor = Color.Red;
                textBox8.Text = "FAIL";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            progressBar1.Text = e.ProgressPercentage.ToString();
        }
    }//TC Form

    class FormData
    {
        public bool mystatus;
        public bool myhitsoap;
        public bool mycallwss;

        public bool overallStatus
        {
            get
            {
                return mystatus;
            }
            set
            {
                mystatus = value;
            }
        }
        public bool hitSoap
        {
            get
            {
                return myhitsoap;
            }
            set
            {
                myhitsoap = value;
            }
        }
        public bool callWss
        {
            get
            {
                return mycallwss;
            }
            set
            {
                mycallwss = value;
            }
        }
    }
}
