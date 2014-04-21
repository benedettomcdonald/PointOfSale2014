using HsSharedObjects;
using HsSharedObjects.Main;
using HsSharedObjects.Client;

using System;
//using System.Net;
//using System.Drawing;
using System.Collections;
//using System.ComponentModel;
using System.Windows.Forms;
//using System.Xml;
//using System.Xml.XPath;

namespace HsCnxConsole
{
	/// <summary>
	/// Summary description for EmployeeSyncForm.
	/// </summary>
	public class Wizard : System.Windows.Forms.Form
	{
		#region EmployeeSyncForm declare variables

		private Hashtable posEmployeeMap;
		private ArrayList posJobs;
		private ArrayList hsJobs;
		private ArrayList hsScheds;
		private ArrayList syncJobs;
		private Hashtable posEmployeeMapById;
		private Hashtable hsEmployeeMap;
		private Hashtable syncEmployeeMap;
		private Hashtable hsRoleMap;

		private ClientDetails details;

		private String clientId = "-1";
	//	private EmployeeList syncEmployeeList;
	//	private JobList hsJobList;
		private SysLog logger;
	//	private EmployeeManager empManager;
	//	private JobManager jobManager;

		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ListBox listBox3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListBox listBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.ListBox listBox4;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.ListBox listBox5;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ListBox listBox6;
		private System.Windows.Forms.ListBox listBox7;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.Button button12;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private WizardSharedMethods wiz;

		#endregion

		public Wizard( ClientDetails details )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.details = details;
			logger = new SysLog( this.GetType() );
			wiz = Run.wiz;
			posEmployeeMapById = new Hashtable();
			posEmployeeMap = new Hashtable();
			posJobs = new ArrayList();
			hsJobs = new ArrayList();
			hsScheds = new ArrayList();
			syncJobs = new ArrayList();
			hsEmployeeMap = new Hashtable();
			syncEmployeeMap = new Hashtable();
			logger.Debug( "WIZARD -- loading POS data" );
			loadPosData();
			logger.Debug( "WIZARD -- loading HS data" );
			loadHsData();
			LoadLists();
			LoadJobs();
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
			this.panel2 = new System.Windows.Forms.Panel();
			this.button11 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.listBox3 = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.panel3 = new System.Windows.Forms.Panel();
			this.button12 = new System.Windows.Forms.Button();
			this.button10 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.listBox7 = new System.Windows.Forms.ListBox();
			this.listBox6 = new System.Windows.Forms.ListBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label18 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.listBox5 = new System.Windows.Forms.ListBox();
			this.panel7 = new System.Windows.Forms.Panel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label12 = new System.Windows.Forms.Label();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.button7 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.listBox4 = new System.Windows.Forms.ListBox();
			this.label11 = new System.Windows.Forms.Label();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.button11);
			this.panel2.Controls.Add(this.button8);
			this.panel2.Controls.Add(this.button5);
			this.panel2.Controls.Add(this.button3);
			this.panel2.Controls.Add(this.button4);
			this.panel2.Controls.Add(this.panel1);
			this.panel2.Controls.Add(this.label9);
			this.panel2.Controls.Add(this.label10);
			this.panel2.Controls.Add(this.label8);
			this.panel2.Controls.Add(this.label7);
			this.panel2.Controls.Add(this.listBox3);
			this.panel2.Controls.Add(this.label5);
			this.panel2.Controls.Add(this.label6);
			this.panel2.Controls.Add(this.button2);
			this.panel2.Controls.Add(this.button1);
			this.panel2.Controls.Add(this.label3);
			this.panel2.Controls.Add(this.label4);
			this.panel2.Controls.Add(this.listBox2);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.listBox1);
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(728, 568);
			this.panel2.TabIndex = 19;
			this.panel2.Visible = false;
			// 
			// button11
			// 
			this.button11.BackColor = System.Drawing.Color.Orange;
			this.button11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button11.ForeColor = System.Drawing.Color.Black;
			this.button11.Location = new System.Drawing.Point(88, 160);
			this.button11.Name = "button11";
			this.button11.Size = new System.Drawing.Size(40, 23);
			this.button11.TabIndex = 39;
			this.button11.Text = "ALL ";
			this.button11.Click += new System.EventHandler(this.button11_Click);
			// 
			// button8
			// 
			this.button8.BackColor = System.Drawing.Color.DarkOrange;
			this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button8.Location = new System.Drawing.Point(8, 536);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(88, 23);
			this.button8.TabIndex = 38;
			this.button8.Text = "<< Back ";
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button5
			// 
			this.button5.BackColor = System.Drawing.Color.DarkOrange;
			this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button5.Location = new System.Drawing.Point(24, 104);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(120, 23);
			this.button5.TabIndex = 37;
			this.button5.Text = "Run Auto Sync ";
			this.button5.Click += new System.EventHandler(this.click_autosync);
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.Color.DarkOrange;
			this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button3.Location = new System.Drawing.Point(592, 536);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(120, 23);
			this.button3.TabIndex = 36;
			this.button3.Text = "Sychronize";
			this.button3.Click += new System.EventHandler(this.click_syncronize);
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.Color.DarkOrange;
			this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button4.Location = new System.Drawing.Point(504, 536);
			this.button4.Name = "button4";
			this.button4.TabIndex = 35;
			this.button4.Text = "Cancel ";
			this.button4.Click += new System.EventHandler(this.click_cancel);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Black;
			this.panel1.Location = new System.Drawing.Point(0, 520);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(728, 3);
			this.panel1.TabIndex = 34;
			// 
			// label9
			// 
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label9.Location = new System.Drawing.Point(528, 192);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(184, 32);
			this.label9.TabIndex = 33;
			this.label9.Text = "Review the list before sycronizing.";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label10
			// 
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label10.Location = new System.Drawing.Point(528, 160);
			this.label10.Name = "label10";
			this.label10.TabIndex = 32;
			this.label10.Text = "Step 4:";
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.Color.AliceBlue;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(160, 88);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(560, 56);
			this.label8.TabIndex = 31;
			this.label8.Text = "NOTE: To add an employee to the synchronize list without matching him/her, simply" +
				" double-click that employee’s name.";
			// 
			// label7
			// 
			this.label7.BackColor = System.Drawing.Color.AliceBlue;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label7.Location = new System.Drawing.Point(8, 8);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(712, 80);
			this.label7.TabIndex = 30;
			this.label7.Text = @"The POS Employee Sync will allow you to properly synchronize your employees from your POS into the HotSchedules Scheduling Platform.  Please select an employee from the first list and then select the matching counterpart from the second list and click ’Add’.  Once you have finished creating the list of synchronized employees, click ‘Synchronize’.";
			// 
			// listBox3
			// 
			this.listBox3.HorizontalScrollbar = true;
			this.listBox3.Location = new System.Drawing.Point(528, 232);
			this.listBox3.Name = "listBox3";
			this.listBox3.Size = new System.Drawing.Size(184, 277);
			this.listBox3.TabIndex = 29;
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(400, 192);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(120, 32);
			this.label5.TabIndex = 28;
			this.label5.Text = "Add the employee to the sync list.";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(400, 160);
			this.label6.Name = "label6";
			this.label6.TabIndex = 27;
			this.label6.Text = "Step 3:";
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.Color.DarkOrange;
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button2.Location = new System.Drawing.Point(400, 264);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(88, 23);
			this.button2.TabIndex = 26;
			this.button2.Text = "<< Remove ";
			this.button2.Click += new System.EventHandler(this.click_remove);
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.Color.DarkOrange;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(400, 232);
			this.button1.Name = "button1";
			this.button1.TabIndex = 25;
			this.button1.Text = "Add >>";
			this.button1.Click += new System.EventHandler(this.click_add);
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(208, 192);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(184, 32);
			this.label3.TabIndex = 24;
			this.label3.Text = "Select an employee from HotSchedules.";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(208, 160);
			this.label4.Name = "label4";
			this.label4.TabIndex = 23;
			this.label4.Text = "Step 2:";
			// 
			// listBox2
			// 
			this.listBox2.HorizontalScrollbar = true;
			this.listBox2.Location = new System.Drawing.Point(208, 232);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(184, 277);
			this.listBox2.TabIndex = 22;
			this.listBox2.DoubleClick += new System.EventHandler(this.double_click_hs_emp);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(8, 192);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 32);
			this.label2.TabIndex = 21;
			this.label2.Text = "Select an employee from your POS.";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 160);
			this.label1.Name = "label1";
			this.label1.TabIndex = 20;
			this.label1.Text = "Step 1:";
			// 
			// listBox1
			// 
			this.listBox1.HorizontalScrollbar = true;
			this.listBox1.Location = new System.Drawing.Point(8, 232);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(184, 277);
			this.listBox1.TabIndex = 19;
			this.listBox1.DoubleClick += new System.EventHandler(this.double_click_pos_emp);
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.LightSteelBlue;
			this.panel3.Controls.Add(this.button12);
			this.panel3.Controls.Add(this.button10);
			this.panel3.Controls.Add(this.button9);
			this.panel3.Controls.Add(this.label16);
			this.panel3.Controls.Add(this.label13);
			this.panel3.Controls.Add(this.listBox7);
			this.panel3.Controls.Add(this.listBox6);
			this.panel3.Controls.Add(this.comboBox1);
			this.panel3.Controls.Add(this.label18);
			this.panel3.Controls.Add(this.label15);
			this.panel3.Controls.Add(this.label14);
			this.panel3.Controls.Add(this.listBox5);
			this.panel3.Controls.Add(this.panel7);
			this.panel3.Controls.Add(this.panel5);
			this.panel3.Controls.Add(this.button7);
			this.panel3.Controls.Add(this.button6);
			this.panel3.Controls.Add(this.panel4);
			this.panel3.Controls.Add(this.listBox4);
			this.panel3.Controls.Add(this.label11);
			this.panel3.Location = new System.Drawing.Point(0, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(728, 568);
			this.panel3.TabIndex = 40;
			// 
			// button12
			// 
			this.button12.BackColor = System.Drawing.Color.DarkOrange;
			this.button12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button12.Location = new System.Drawing.Point(160, 184);
			this.button12.Name = "button12";
			this.button12.Size = new System.Drawing.Size(96, 23);
			this.button12.TabIndex = 61;
			this.button12.Text = "All";
			this.button12.Click += new System.EventHandler(this.button12_Click_1);
			// 
			// button10
			// 
			this.button10.BackColor = System.Drawing.Color.DarkOrange;
			this.button10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button10.Location = new System.Drawing.Point(384, 72);
			this.button10.Name = "button10";
			this.button10.Size = new System.Drawing.Size(96, 23);
			this.button10.TabIndex = 60;
			this.button10.Text = "RESET ";
			this.button10.Click += new System.EventHandler(this.button10_Click);
			// 
			// button9
			// 
			this.button9.BackColor = System.Drawing.Color.DarkOrange;
			this.button9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button9.Location = new System.Drawing.Point(384, 40);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(96, 23);
			this.button9.TabIndex = 59;
			this.button9.Text = "ADD >";
			this.button9.Click += new System.EventHandler(this.AddJob_Click);
			// 
			// label16
			// 
			this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label16.Location = new System.Drawing.Point(272, 8);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(96, 23);
			this.label16.TabIndex = 58;
			this.label16.Text = "HS Jobs";
			// 
			// label13
			// 
			this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label13.Location = new System.Drawing.Point(160, 8);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(96, 23);
			this.label13.TabIndex = 57;
			this.label13.Text = "POS Jobs";
			// 
			// listBox7
			// 
			this.listBox7.HorizontalScrollbar = true;
			this.listBox7.Location = new System.Drawing.Point(488, 40);
			this.listBox7.Name = "listBox7";
			this.listBox7.Size = new System.Drawing.Size(104, 147);
			this.listBox7.TabIndex = 56;
			this.listBox7.SelectedIndexChanged += new System.EventHandler(this.listBox7_SelectedIndexChanged);
			// 
			// listBox6
			// 
			this.listBox6.HorizontalScrollbar = true;
			this.listBox6.Location = new System.Drawing.Point(272, 40);
			this.listBox6.Name = "listBox6";
			this.listBox6.Size = new System.Drawing.Size(104, 147);
			this.listBox6.TabIndex = 55;
			// 
			// comboBox1
			// 
			this.comboBox1.Location = new System.Drawing.Point(600, 40);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 54;
			this.comboBox1.Text = "None Selected";
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label18
			// 
			this.label18.BackColor = System.Drawing.Color.AliceBlue;
			this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label18.Location = new System.Drawing.Point(8, 40);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(144, 144);
			this.label18.TabIndex = 51;
			this.label18.Text = "Match up your pos job names to their corresponding HotSchedule\'s role.";
			// 
			// label15
			// 
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label15.Location = new System.Drawing.Point(8, 8);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(136, 23);
			this.label15.TabIndex = 45;
			this.label15.Text = "Job Definition";
			// 
			// label14
			// 
			this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label14.Location = new System.Drawing.Point(8, 232);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(200, 23);
			this.label14.TabIndex = 44;
			this.label14.Text = "Employee Sync";
			// 
			// listBox5
			// 
			this.listBox5.HorizontalScrollbar = true;
			this.listBox5.Location = new System.Drawing.Point(160, 40);
			this.listBox5.Name = "listBox5";
			this.listBox5.Size = new System.Drawing.Size(96, 134);
			this.listBox5.TabIndex = 40;
			this.listBox5.DoubleClick += new System.EventHandler(this.DblClickPosJob);
			// 
			// panel7
			// 
			this.panel7.BackColor = System.Drawing.Color.Black;
			this.panel7.Location = new System.Drawing.Point(0, 216);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(728, 3);
			this.panel7.TabIndex = 39;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.AliceBlue;
			this.panel5.Controls.Add(this.label12);
			this.panel5.Controls.Add(this.radioButton2);
			this.panel5.Controls.Add(this.radioButton1);
			this.panel5.Location = new System.Drawing.Point(8, 368);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(456, 144);
			this.panel5.TabIndex = 38;
			// 
			// label12
			// 
			this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label12.Location = new System.Drawing.Point(8, 8);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(448, 64);
			this.label12.TabIndex = 2;
			this.label12.Text = "The next step will also give you the ability to \'Auto Syncronize\' your employees." +
				"  Please choose from the options below on how you would like those employees mat" +
				"ched.";
			// 
			// radioButton2
			// 
			this.radioButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.radioButton2.Location = new System.Drawing.Point(8, 112);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(368, 24);
			this.radioButton2.TabIndex = 1;
			this.radioButton2.Text = "Match first name,  last name and birthday.";
			// 
			// radioButton1
			// 
			this.radioButton1.Checked = true;
			this.radioButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.radioButton1.Location = new System.Drawing.Point(8, 80);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(280, 24);
			this.radioButton1.TabIndex = 0;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Match first and last name only.";
			// 
			// button7
			// 
			this.button7.BackColor = System.Drawing.Color.DarkOrange;
			this.button7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button7.Location = new System.Drawing.Point(592, 536);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(120, 23);
			this.button7.TabIndex = 37;
			this.button7.Text = "Next >>";
			this.button7.Click += new System.EventHandler(this.click_next);
			// 
			// button6
			// 
			this.button6.BackColor = System.Drawing.Color.DarkOrange;
			this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button6.Location = new System.Drawing.Point(504, 536);
			this.button6.Name = "button6";
			this.button6.TabIndex = 36;
			this.button6.Text = "Cancel ";
			this.button6.Click += new System.EventHandler(this.click_cancel);
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.Black;
			this.panel4.Controls.Add(this.panel6);
			this.panel4.Location = new System.Drawing.Point(0, 520);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(728, 3);
			this.panel4.TabIndex = 33;
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.Black;
			this.panel6.Location = new System.Drawing.Point(0, 0);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(728, 3);
			this.panel6.TabIndex = 34;
			// 
			// listBox4
			// 
			this.listBox4.Location = new System.Drawing.Point(472, 264);
			this.listBox4.Name = "listBox4";
			this.listBox4.Size = new System.Drawing.Size(240, 251);
			this.listBox4.TabIndex = 32;
			// 
			// label11
			// 
			this.label11.BackColor = System.Drawing.Color.AliceBlue;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label11.Location = new System.Drawing.Point(8, 264);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(456, 96);
			this.label11.TabIndex = 31;
			this.label11.Text = @"Please review the list of employees on the right.  These are the employees that are currently syncronized in HotSchedules.  On the next screen you will have an opportunity to syncronize other employees, and even add employees to HotSchedules from your POS system list.";
			// 
			// Wizard
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.LightSteelBlue;
			this.ClientSize = new System.Drawing.Size(728, 566);
			this.ControlBox = false;
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Wizard";
			this.Text = "POS Sync";
			this.Load += new System.EventHandler(this.EmployeeSyncForm_Load);
			this.Closed += new System.EventHandler(this.Wizard_Closed);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void loadPosData()
		{
			wiz.LoadPosData();
		}

		private bool loadHsData()
		{
			bool loaded = false;
			hsRoleMap = new Hashtable();
			try
			{
				logger.Debug( "getting hs emp list" );
				wiz.LoadHsData();
			}
			catch( Exception ex )
			{
				logger.Error( ex.ToString() );
			}
			return loaded;
		}

		private void LoadLists()
		{
			this.listBox1.Items.Clear();
			this.listBox2.Items.Clear();
			this.listBox3.Items.Clear();
			this.listBox4.Items.Clear();
			foreach(String key  in wiz.PosEmployeeMap.Keys)
			{
				SharedEmployee emp = (SharedEmployee)wiz.PosEmployeeMap[key];
				if(emp.IsNew)
				{
					this.listBox1.Items.Add( key );
				}
				else
				{
					this.listBox1.Items.Insert(0 , key);
				}
			}
			foreach(String hskey  in wiz.HsEmployeeMap.Keys)
			{
				SharedEmployee hsemp = (SharedEmployee)wiz.HsEmployeeMap[hskey];
				if(hsemp.IsNew)
				{
					this.listBox2.Items.Add( hskey );
				}
				else
				{
					this.listBox2.Items.Insert(0 , hskey);
				}
			}
			foreach(String synckey  in wiz.SyncEmployeeMap.Keys)
			{
				SharedEmployee syncemp = (SharedEmployee)wiz.SyncEmployeeMap[synckey];
				if(syncemp.IsNew)
				{
					this.listBox3.Items.Insert(0 , synckey);
				}
				else
				{
					this.listBox4.Items.Insert(0 , synckey);
				}
			}
		}

		private void LoadJobs()
		{
			logger.Debug( "WIZARD -- loading HS job list" );
			//LOAD HS JOB LIST
			posJobs.Clear();
			hsJobs.Clear();
			hsScheds.Clear();
			syncJobs.Clear();
			this.listBox5.Items.Clear();
			this.listBox6.Items.Clear();
			this.listBox7.Items.Clear();
			this.comboBox1.Items.Clear();

			wiz.LoadJobs();

			foreach(SharedJob job in wiz.PosJobs)
			{
				this.listBox5.Items.Add(job.Name);
			}
			this.comboBox1.Items.Add("None Selected");
			foreach(SharedSchedule sched in wiz.HsScheds)
			{
				this.comboBox1.Items.Add(sched.Name);
			}
			foreach(SharedJob job in wiz.SyncJobs)
			{
				this.listBox7.Items.Add(job.Name);
			}
			foreach(SharedJob job in wiz.HsJobs)
			{
				this.listBox6.Items.Add(job.Name);
			}
		}

		private void EmployeeSyncForm_Load(object sender, System.EventArgs e)
		{
			if( this.listBox4.Items.Count < 1 ) this.listBox4.Items.Add( "No employees have been syncronized." );
		}

		private void click_cancel(object sender, System.EventArgs e)
		{
			this.details.UseWizard = ClientDetails.WIZARD_OFF;
			Run.wiz.Cancel();
			this.Close();
		}

		private void AddPosEmployee( SharedEmployee emp , bool isNew )
		{
			String key = Run.wiz.AddPosEmployee(emp, isNew);
			if( isNew )
			{
				this.listBox1.Items.Add( key );
			} 
			else this.listBox1.Items.Insert( 0 , key );			
			// add employee to the POS emp hash
		}
		private void ReAddPosEmployee( SharedEmployee emp )
		{
			SharedEmployee newEmp = Run.wiz.ReAddPosEmployee(emp);
			AddPosEmployee(newEmp, false);
		}

		private void AddHsEmployee( SharedEmployee emp , bool isNew )
		{
			String key = Run.wiz.AddHsEmployee(emp, isNew);			
			if( isNew )
			{
				this.listBox2.Items.Add( key );
			} 
			else this.listBox2.Items.Insert( 0 , key );
			// add employee to the HS emp hash if the key doesn't exist
		}

		private void AddSyncEmployee( SharedEmployee emp , bool shouldSync )
		{
			String key = Run.wiz.AddSyncEmployee(emp, shouldSync);
			// add emps to the preloaded
			if( !shouldSync ) this.listBox4.Items.Insert( 0 , key );
			// add emps to the final sync list
			if( shouldSync ) this.listBox3.Items.Insert( 0 , key );
		}

		private void RemovePosEmployee( SharedEmployee emp )
		{
			String key = Run.wiz.RemovePosEmployee(emp);
			// add the employee to the POS emp list box
			this.listBox1.Items.Remove( key );
		}

		private void RemoveHsEmployee( SharedEmployee emp )
		{
			String key = Run.wiz.RemoveHsEmployee(emp);
			// add the employee to the HS emp list box
			this.listBox2.Items.Remove( key );
		}

		private void RemoveSyncEmployee( SharedEmployee emp )
		{
			// this key will be the display for the emp and uniquely identify them in a Hashtable
			String key = Run.wiz.RemoveSyncEmployee(emp);
			this.listBox3.Items.Remove( key );
		}

		private void click_add(object sender, System.EventArgs e)
		{
			// check to make sure an employee from each list has been selected
			if( this.listBox2.SelectedItem != null && this.listBox1.SelectedItem != null )
			{
				String hsBox = this.listBox2.SelectedItem.ToString();
				SharedEmployee hsEmp = (SharedEmployee)Run.wiz.HsEmployeeMap[hsBox];
				String posBox = this.listBox1.SelectedItem.ToString();
				SharedEmployee posEmp = (SharedEmployee)Run.wiz.PosEmployeeMap[posBox];

				hsEmp.PosId = posEmp.PosId;
				AddSyncEmployee( hsEmp , true );
				RemovePosEmployee( posEmp );
				RemoveHsEmployee( hsEmp );
			} 
			else MessageBox.Show( "You must select one employee from each list." );
		}

		private void click_remove(object sender, System.EventArgs e)
		{
			// check to make sure an item has been selected
			if( this.listBox3.SelectedItem != null )
			{
				if(  this.listBox3.SelectedItem.ToString().EndsWith( "[From HS]" ) )
				{
					MessageBox.Show( "You cannot remove this employee, he/she is already syncronized in HotSchedules." );
				} 
				else 
				{
					String syncBox = this.listBox3.SelectedItem.ToString();
					SharedEmployee syncEmp = (SharedEmployee)Run.wiz.SyncEmployeeMap[syncBox];
				
					ReAddPosEmployee( syncEmp);
					// reset the hsEmp's ExtId back to -1
					syncEmp.PosId = -1;
					RemoveSyncEmployee( syncEmp );
					if( wiz.HsEmployeeMap[syncBox] == null && syncEmp.HsUserName != null)
					{ 
						AddHsEmployee( syncEmp , false );
					}
				}
				// Alert if they do not have a selection
			} 
			else MessageBox.Show( "You must select an employee to be removed." );
		}

		private void double_click_pos_emp(object sender, System.EventArgs e)
		{
			SharedEmployee posEmp = (SharedEmployee) Run.wiz.PosEmployeeMap[ this.listBox1.SelectedItem.ToString() ];
			AddSyncEmployee( posEmp , true );
			RemovePosEmployee( posEmp );
		}

		private void double_click_hs_emp(object sender, System.EventArgs e)
		{
			MessageBox.Show( "It is not necessary to add a HotSchedules employee to " +
				"the syncronized list. All HotSchedules employees are already included." );
		}


		private void click_syncronize(object sender, System.EventArgs e)
		{
			this.panel2.Enabled = false;
			int[] counts = Run.wiz.Synchronize();
			this.Hide();
			MessageBox.Show( counts[0].ToString() + " employees have been synchronized.\n" + 
				counts[1].ToString() + " jobs have been synchronized." );
			this.Close();
		}

		private void click_autosync(object sender, System.EventArgs e)
		{
			Run.wiz.AutoSync(this.radioButton1.Checked, this.radioButton2.Checked);
			LoadLists();
		}


		private void click_next(object sender, System.EventArgs e)
		{
			this.panel3.Visible = false;
			this.panel5.Visible = false;
			this.panel2.Visible = true;
		}

		private void button8_Click(object sender, System.EventArgs e)
		{
			this.panel3.Visible = true;
			this.panel5.Visible = true;
			this.panel2.Visible = false;
		}

		private void Wizard_Closed(object sender, System.EventArgs e)
		{
			this.Hide();
		}

		private void AddJob_Click(object sender, System.EventArgs e)
		{
			int posIndex = this.listBox5.SelectedIndex;
			int hsIndex = this.listBox6.SelectedIndex;
			if( posIndex != -1 && hsIndex != -1 )
			{
				SharedJob posJob = (SharedJob) Run.wiz.PosJobs[posIndex];
				SharedJob hsJob = (SharedJob) Run.wiz.HsJobs[hsIndex];
				if( posJob != null && hsJob != null )
				{
					Run.wiz.AddJob(posIndex, hsIndex, hsJob, posJob);
				// remove from POS jobs
					this.listBox5.Items.RemoveAt( posIndex );
				// remove from HS jobs display
					this.listBox6.Items.RemoveAt( hsIndex );
				// add to sync list
					int index = this.listBox7.Items.Add( hsJob.Name + "[" + hsJob.ExtId + "]" );
				} 
				else MessageBox.Show( "There was an error selecting jobs, please select again." );
			} 
			else MessageBox.Show( "You must select a POS job AND a HS job!" );
		}

		private void button10_Click(object sender, System.EventArgs e)
		{
			this.LoadJobs();
		}

		private void DblClickPosJob(object sender, System.EventArgs e)
		{
			int posIndex = this.listBox5.SelectedIndex;
			
			if( posIndex != -1 )
			{
				SharedJob posJob = (SharedJob) Run.wiz.PosJobs[posIndex];
				// remove from POS jobs
				this.listBox5.Items.RemoveAt( posIndex );
				// add to sync list
				int index = this.listBox7.Items.Add( posJob.Name + "[" + posJob.ExtId + "]" );
				Run.wiz.DoubleClickPosJob(posJob, posIndex);
			}
		}

		private void listBox7_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SharedJob job = (SharedJob) Run.wiz.SyncJobs[ this.listBox7.SelectedIndex ];
			int schedId = job.RoleId;
			if( schedId >= 0 )
			{
				for( int i=0; i<Run.wiz.HsScheds.Count; i++ )
				{
					SharedSchedule sched = (SharedSchedule) Run.wiz.HsScheds[ i ];
					if( sched.HsId == job.RoleId )
					{
						this.comboBox1.SelectedIndex = i + 1;
						break;
					}
				}
			}
			else
			{
				this.comboBox1.SelectedIndex = 0;
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int selected = this.listBox7.SelectedIndex;
			if( selected >= 0)
			{
				SharedSchedule sched = new SharedSchedule();
				if(this.comboBox1.SelectedIndex > 0)
				{
					sched = (SharedSchedule) Run.wiz.HsScheds[ this.comboBox1.SelectedIndex - 1];
				}
				if( sched != null )
				{
					SharedJob job = (SharedJob) Run.wiz.SyncJobs[ selected ];
					if(job.RoleId == sched.HsId)
						return;
					Run.wiz.UpdateJobSched(selected, sched);
				}
			}
			else 
			{
				MessageBox.Show( "You must select a job from the Sync List." );
				this.comboBox1.SelectedIndex = 0;
			}
		}


		private void button11_Click(object sender, System.EventArgs e)
		{
			while( this.listBox1.Items.Count > 0 )
			{
				SharedEmployee posEmp = (SharedEmployee) Run.wiz.PosEmployeeMap[ this.listBox1.Items[this.listBox1.Items.Count-1].ToString() ];
				AddSyncEmployee( posEmp , true );
				RemovePosEmployee( posEmp );
			}
		}

		private void button12_Click_1(object sender, System.EventArgs e)
		{
			int posIndex = -1;
			while( this.listBox5.Items.Count > 0 )
			{
				posIndex = this.listBox5.Items.Count - 1;
				SharedJob posJob = (SharedJob) Run.wiz.PosJobs[posIndex];
				// remove from POS jobs
				this.listBox5.Items.RemoveAt( posIndex );
				// add to sync list
				int index = this.listBox7.Items.Add( posJob.Name + "[" + posJob.ExtId + "]" );
				Run.wiz.Button12(posJob, posIndex, index);
			}
			
		}
		public String ClientId
		{
			get
			{
				return this.clientId;
			}
			set
			{	
				this.clientId = value;
			}
		}
	}
}
