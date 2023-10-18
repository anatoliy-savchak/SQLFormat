namespace SQL_Format
{
	partial class FormSQLFormat
	{
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSQLFormat));
            this.TabCtrl = new System.Windows.Forms.TabControl();
            this.TPSource = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MSource = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TabCtrl.SuspendLayout();
            this.TPSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabCtrl
            // 
            this.TabCtrl.Controls.Add(this.TPSource);
            this.TabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabCtrl.Location = new System.Drawing.Point(0, 0);
            this.TabCtrl.Name = "TabCtrl";
            this.TabCtrl.SelectedIndex = 0;
            this.TabCtrl.Size = new System.Drawing.Size(1410, 571);
            this.TabCtrl.TabIndex = 0;
            // 
            // TPSource
            // 
            this.TPSource.Controls.Add(this.splitContainer2);
            this.TPSource.Location = new System.Drawing.Point(4, 22);
            this.TPSource.Name = "TPSource";
            this.TPSource.Padding = new System.Windows.Forms.Padding(3);
            this.TPSource.Size = new System.Drawing.Size(1402, 545);
            this.TPSource.TabIndex = 0;
            this.TPSource.Text = "Source";
            this.TPSource.UseVisualStyleBackColor = true;
            this.TPSource.Leave += new System.EventHandler(this.TPSource_Leave);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.MSource);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer2.Panel2Collapsed = true;
            this.splitContainer2.Size = new System.Drawing.Size(1396, 539);
            this.splitContainer2.SplitterDistance = 465;
            this.splitContainer2.TabIndex = 0;
            // 
            // MSource
            // 
            this.MSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MSource.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MSource.Location = new System.Drawing.Point(0, 0);
            this.MSource.MaxLength = 10000000;
            this.MSource.Multiline = true;
            this.MSource.Name = "MSource";
            this.MSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MSource.Size = new System.Drawing.Size(1396, 539);
            this.MSource.TabIndex = 1;
            this.MSource.Text = resources.GetString("MSource.Text");
            this.MSource.WordWrap = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(150, 46);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // FormSQLFormat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1410, 571);
            this.Controls.Add(this.TabCtrl);
            this.Name = "FormSQLFormat";
            this.Text = "SQL Format";
            this.TabCtrl.ResumeLayout(false);
            this.TPSource.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl TabCtrl;
		private System.Windows.Forms.TabPage TPSource;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TextBox MSource;
		private System.Windows.Forms.GroupBox groupBox2;
	}
}