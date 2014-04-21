using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Diagnostics;
using HsSharedObjects.Main;
using TimeoutException=System.ServiceProcess.TimeoutException;
using HsProperties;

namespace HsConnectRestart
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label1;
		private System.Timers.Timer restartTimer;
        private static SysLog logger = new SysLog(typeof(Form1));
        private Label lblError;
        private Button btnClose;
        private Label label2;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
		    logger.Debug("HsConnectRestart Form1");
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.restartTimer = new System.Timers.Timer();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.restartTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.progressBar1.Location = new System.Drawing.Point(12, 93);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(332, 29);
            this.progressBar1.Step = 20;
            this.progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label1.Location = new System.Drawing.Point(9, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(278, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Restarting";
            // 
            // restartTimer
            // 
            this.restartTimer.Enabled = true;
            this.restartTimer.Interval = 2000D;
            this.restartTimer.SynchronizingObject = this;
            this.restartTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(332, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Please wait, while HsConnect restarts...";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(206, 93);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(138, 29);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.Visible = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblError
            // 
            this.lblError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblError.Location = new System.Drawing.Point(12, 12);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(332, 78);
            this.lblError.TabIndex = 6;
            this.lblError.Text = "Error Message";
            this.lblError.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(356, 134);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnClose);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "HsConnect Restarter";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.restartTimer)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
            Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                logger.Debug("Attempting to restart HsConnect");
                Process[] procs = Process.GetProcessesByName("HsConnectRestart");
                if (procs.Length > 1)
                {
                    logger.Error("Another instance of the HS Restarter is already running");
                    ShowErrorAndExit("Another instance of the HS Restarter is already running");
                }
            }
            catch (InvalidOperationException ioe)
            {
                if (ioe.InnerException != null && ioe.InnerException is Win32Exception)
                    ShowErrorAndExit("Unabled to execute HsConnect Restarter.  Please contact your IT Administrator about running HsConnect Restarter with Administrator privileges.");
                else
                    ShowErrorAndExit(Properties.SupportMessage);
            }
            catch (Exception ex)
            {
                logger.Error("Could not restart: Exception", ex);
                ShowErrorAndExit(Properties.SupportMessage);
            }
		}

		private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
            logger.Debug("Timer elapsed");
			restartTimer.Enabled = false;
//			progressBar1.Minimum = 0;
//			progressBar1.Maximum = 100;
//			progressBar1.Step = 20;
			progressBar1.Value = 0;
//			progressBar1.Visible = true;

			ServiceController cnx = new ServiceController("_hscnx");
			ServiceController updtr = new ServiceController("_hsupdtr");
			TimeSpan timeout = new TimeSpan(0,0,45);
			try
			{
				if(cnx.Status == ServiceControllerStatus.StartPending || cnx.Status == ServiceControllerStatus.Running)
				{
                    logger.Debug("Stopping HsConnect");
					label1.Text = "Stopping HsConnect";
					label1.Refresh();
					cnx.Stop();
					cnx.WaitForStatus(ServiceControllerStatus.Stopped, timeout); 
				}
				progressBar1.PerformStep();
				if(updtr.Status == ServiceControllerStatus.StartPending || updtr.Status == ServiceControllerStatus.Running)
				{
                    logger.Debug("Stopping HS Updater");
					label1.Text = "Stopping HS Updater";
					label1.Refresh();
					updtr.Stop();
					updtr.WaitForStatus(ServiceControllerStatus.Stopped, timeout); 
				}
				progressBar1.PerformStep();
				if(updtr.Status == ServiceControllerStatus.Stopped)
				{
                    logger.Debug("Starting HS Updater");
					label1.Text = "Starting HS Updater";
					label1.Refresh();
					updtr.Start();
					updtr.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0,3,0));
					progressBar1.PerformStep();
                    logger.Debug("Starting HsConnect");
					label1.Text = "Starting HsConnect";
					label1.Refresh();
					cnx.WaitForStatus(ServiceControllerStatus.Running, timeout);
					progressBar1.PerformStep();
				}
			    logger.Debug("Loading tray icon");
				label1.Text = "Loading Tray Icon";
				label1.Refresh();
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.StartInfo.FileName = "_hscnxConsole.exe";
				proc.StartInfo.WorkingDirectory = Application.StartupPath;
				//proc.StartInfo.WorkingDirectory = @"c:\program files\hotschedules\HsConnect\files";
				proc.Start();
				progressBar1.Value = 100;
                logger.Debug("Successfully restarted HsConnect");
				label1.Text = "Successfully Restarted";
				Environment.Exit(0);
			}
			catch(TimeoutException tex)
			{
                logger.Error("HsConnect took too long restarting.", tex);
                ShowErrorAndExit(Properties.SupportMessage);
//				Environment.Exit(0);
			}
			catch(Exception ex)
            {
                logger.Error("HsConnect took too long restarting.", ex);
				ShowErrorAndExit(Properties.SupportMessage);
//				Environment.Exit(0);
			}
		}

        private void ShowErrorAndExit(String message)
        {
            restartTimer.Stop();
            label1.Visible = false;
            label2.Visible = false;
            progressBar1.Visible = false;
            lblError.Visible = true;
            btnClose.Visible = true;
            lblError.Text = message;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

	}
}
