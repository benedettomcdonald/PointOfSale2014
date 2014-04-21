using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Diagnostics;

namespace HsConnect.Forms
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ApprovalForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;

		private int simpleTrades = 0;
		private int shiftSwaps = 0;
		private static bool isOn = false;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ApprovalForm()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ApprovalForm));
			this.panel1 = new System.Windows.Forms.Panel();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.Controls.Add(this.button3);
			this.panel1.Controls.Add(this.button2);
			this.panel1.Controls.Add(this.button1);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.textBox2);
			this.panel1.Controls.Add(this.textBox1);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(184, 176);
			this.panel1.TabIndex = 0;
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.Orange;
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button3.ForeColor = System.Drawing.Color.Black;
			this.button3.Location = new System.Drawing.Point(8, 96);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(75, 24);
			this.button3.TabIndex = 5;
			this.button3.Text = "Submit";
			this.button3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.button3.Visible = false;
			this.button3.Click += new System.EventHandler(this.Login);
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.Orange;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.ForeColor = System.Drawing.Color.Black;
			this.button2.Location = new System.Drawing.Point(96, 96);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 24);
			this.button2.TabIndex = 2;
			this.button2.Text = "Cancel";
			this.button2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.Orange;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.ForeColor = System.Drawing.Color.Black;
			this.button1.Location = new System.Drawing.Point(8, 96);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 24);
			this.button1.TabIndex = 1;
			this.button1.Text = "Login ";
			this.button1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.button1.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(0, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 72);
			this.label1.TabIndex = 0;
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.Black;
			this.label3.Location = new System.Drawing.Point(8, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 23);
			this.label3.TabIndex = 7;
			this.label3.Text = "Password";
			this.label3.Visible = false;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.Black;
			this.label2.Location = new System.Drawing.Point(8, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 6;
			this.label2.Text = "Username";
			this.label2.Visible = false;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(88, 48);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(80, 20);
			this.textBox2.TabIndex = 4;
			this.textBox2.Text = "";
			this.textBox2.Visible = false;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(88, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(80, 20);
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = "";
			this.textBox1.Visible = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(56, 144);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(120, 24);
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// ApprovalForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(184, 168);
			this.ControlBox = false;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.panel1);
			this.ForeColor = System.Drawing.Color.Orange;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Location = new System.Drawing.Point(450, 50);
			this.MinimizeBox = false;
			this.Name = "ApprovalForm";
			this.Opacity = 0;
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public int SimpleTrades
		{
			get{ return this.simpleTrades; }
			set{ this.simpleTrades = value; }
		}

		public int ShiftSwaps
		{
			get{ return this.shiftSwaps; }
			set{ this.shiftSwaps = value; }
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			this.label1.BackColor = Color.Transparent;
			this.label1.Text = "There are ["+this.simpleTrades+"] pick-ups and ["+this.shiftSwaps+"] swaps to approve or deny";
			Thread t1 = new Thread( new ThreadStart( ShowPanel ) );
			t1.Start();			
		}

		private void ShowPanel()
		{
			double opacity = 0.0;
			int nextX = SystemInformation.WorkingArea.Width - (this.Size.Width);
			int nextY = SystemInformation.WorkingArea.Height;
			int finalX = SystemInformation.WorkingArea.Width - this.Size.Width;
			int finalY = SystemInformation.WorkingArea.Height - (this.Size.Height);
			this.Location = new Point( nextX , nextY );
			while( this.Location.Y > finalY )
			{
				this.Location = new Point( nextX , nextY );
				this.Opacity = opacity;
				opacity += .01;
				nextY--;
				Thread.Sleep( 2 );
			}
		}

		public void Display()
		{
			if( isOn == false )
			{
				this.Show();
				isOn = true;
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.Close();
			isOn = false;
		}

		private void button1_Click_1(object sender, System.EventArgs e)
		{
			this.label1.Visible = false;
			this.button1.Visible = false;
			this.button3.Visible = true;
			this.textBox1.Visible = true;
			this.textBox2.Visible = true;
			this.label2.Visible = true;
			this.label3.Visible = true;
		}

		private void Login(object sender, System.EventArgs e)
		{
			String user = this.textBox1.Text;
			String pass = this.textBox2.Text;
			Process.Start( "iexplore.exe",  "http://207.200.59.6/hs/login.hs?username="+user+"&password="+pass );
			this.Close();
		}
	}
}
