using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace HsCnxConsole
{
	/// <summary>
	/// Summary description for SalesDateForm.
	/// </summary>
	public class SalesDateForm : System.Windows.Forms.Form
	{
		private bool cancel = false;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DateTimePicker dateTimePicker1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker dateTimePicker2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SalesDateForm()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SalesDateForm));
			this.label1 = new System.Windows.Forms.Label();
			this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(272, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please select the date of business:";
			// 
			// dateTimePicker1
			// 
			this.dateTimePicker1.Location = new System.Drawing.Point(88, 40);
			this.dateTimePicker1.MinDate = new System.DateTime(2005, 6, 30, 0, 0, 0, 0);
			this.dateTimePicker1.Name = "dateTimePicker1";
			this.dateTimePicker1.TabIndex = 1;
			this.dateTimePicker1.CloseUp += new System.EventHandler(this.dateTimePicker1_ValueChanged);
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.DarkOrange;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(8, 104);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(96, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "Submit";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.DarkOrange;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.Location = new System.Drawing.Point(112, 104);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(96, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "Cancel";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(16, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Start:";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(16, 68);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 23);
			this.label3.TabIndex = 6;
			this.label3.Text = "End:";
			// 
			// dateTimePicker2
			// 
			this.dateTimePicker2.Location = new System.Drawing.Point(88, 68);
			this.dateTimePicker2.MinDate = new System.DateTime(2005, 6, 30, 0, 0, 0, 0);
			this.dateTimePicker2.Name = "dateTimePicker2";
			this.dateTimePicker2.TabIndex = 5;
			this.dateTimePicker2.CloseUp += new System.EventHandler(this.dateTimePicker2_CloseUp);
			// 
			// SalesDateForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(304, 142);
			this.ControlBox = false;
			this.Controls.Add(this.label3);
			this.Controls.Add(this.dateTimePicker2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.dateTimePicker1);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SalesDateForm";
			this.Text = "HS Connect";
			this.Load += new System.EventHandler(this.SalesDateForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void SalesDateForm_Load(object sender, System.EventArgs e)
		{
			this.dateTimePicker1.MinDate = DateTime.Today.AddDays( - 365.0 );
		}

		public DateTime MinDate
		{
			get{ return this.dateTimePicker1.MinDate; }
			set{ this.dateTimePicker1.MinDate = value; }
		}

		public DateTime MaxDate
		{
			get{ return this.dateTimePicker1.MaxDate; }
			set{ this.dateTimePicker1.MaxDate = value; }
		}

		public DateTime GetSalesStartDate()
		{
			return this.dateTimePicker1.Value;
		}

		public DateTime GetSalesEndDate()
		{
			return this.dateTimePicker2.Value;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Close();	
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.cancel = true;
			this.Close();
		}

		private void dateTimePicker1_ValueChanged(object sender, System.EventArgs e)
		{
			if( this.dateTimePicker2.Value < this.dateTimePicker1.Value )
			{
				this.dateTimePicker2.Value = this.dateTimePicker1.Value;
			}
		}

		private void dateTimePicker2_CloseUp(object sender, System.EventArgs e)
		{
			if( this.dateTimePicker2.Value < this.dateTimePicker1.Value )
			{
				MessageBox.Show( "End date must be equal to or greater than start date." );				
				this.dateTimePicker2.Value = this.dateTimePicker1.Value;
			}
		}

		public bool Cancel
		{
			get{ return this.cancel; }
			set{ this.cancel = value; }
		}

	}
}
