using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

using HsSharedObjects.Client;
using HsSharedObjects.Machine;
using HsSharedObjects.Main;

namespace HsCnxConsole
{
	/// <summary>
	/// Summary description for LoginForm.
	/// </summary>
	public class LoginForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private PictureBox pictureBox1;
        private Button button3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LoginForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
            System.Resources.ResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "HotSchedules User Name";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(192, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "HotSchedules Password";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(216, 64);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(120, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(216, 96);
            this.textBox2.Name = "textBox2";
            this.textBox2.PasswordChar = '*';
            this.textBox2.Size = new System.Drawing.Size(120, 20);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.OrangeRed;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(16, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Submit ";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.OrangeRed;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(265, 128);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel ";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(48, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(248, 40);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.OrangeRed;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(106, 128);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(111, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "Test Connection";
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(352, 166);
            this.ControlBox = false;
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.Text = "HS Connect Login";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void LoginForm_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			DialogResult tryCancel = MessageBox.Show( "If you click OK, you will not be able to run HS Connect." , "Cancel Login" , MessageBoxButtons.OKCancel );
			if( tryCancel.ToString().Equals( "OK" ) ) this.Close();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			/*	ClientSettingsService settingsService = new ClientSettingsService();
				int clientId = settingsService.checkLogin( this.textBox1.Text , this.textBox2.Text , Identification.MacAddress );
				if( clientId < 0 ) MessageBox.Show( "Invalid login. Please try again." );
				if( clientId > 0 ) // a client id was returned
				{
					LoadClient loadClient = new LoadClient( clientId );
					ClientDetails details = loadClient.GetClientDetails();
					Run.icon = new TrayIcon( details );
					ClientSyncManager syncManager = new ClientSyncManager( loadClient.GetClientDetails() );
					this.Close();
					syncManager.Execute();
				}*/
		//	if(Run.hsCnx.login(this.textBox1.Text, this.textBox2.Text))
			//	this.Close();
			String clientName = Run.hsCnx.login(this.textBox1.Text, this.textBox2.Text);
			if(!clientName.Equals(""))
			{	
				char[] splitter = {'|'};
				String[] split = clientName.Split(splitter);
				DialogResult verify = MessageBox.Show( "Please verify that this site is " + split[1] ,"Verify",MessageBoxButtons.YesNo );
				if(verify == DialogResult.Yes)
				{
					int clientId = Int32.Parse(split[0]);
					Run.hsCnx.login(clientId, true);
					this.Close();
				}
				else
					MessageBox.Show( "Aborting Login of " + split[1] + ". Please try again." );

			}
			else
				MessageBox.Show( "Invalid login. Please try again." );
		}

        private void button3_Click(object sender, EventArgs e)
        {
            FormTestConnection ftc = new FormTestConnection();
            ftc.ShowDialog();
        }

	}
}
