namespace TestControl
{
  partial class MainForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.ipAddressPortControl1 = new FlexControls.IPAddressPortControl();
      this.ipAddressControl1 = new FlexControls.IPAddressControl();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(61, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "IP Address:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 41);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(92, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "IP Address + Port:";
      // 
      // ipAddressPortControl1
      // 
      this.ipAddressPortControl1.AllowInternalTab = false;
      this.ipAddressPortControl1.AutoHeight = true;
      this.ipAddressPortControl1.BackColor = System.Drawing.SystemColors.Window;
      this.ipAddressPortControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.ipAddressPortControl1.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.ipAddressPortControl1.Location = new System.Drawing.Point(110, 38);
      this.ipAddressPortControl1.Name = "ipAddressPortControl1";
      this.ipAddressPortControl1.ReadOnly = false;
      this.ipAddressPortControl1.Size = new System.Drawing.Size(125, 20);
      this.ipAddressPortControl1.TabIndex = 1;
      this.ipAddressPortControl1.Text = "... : ";
      // 
      // ipAddressControl1
      // 
      this.ipAddressControl1.AllowInternalTab = false;
      this.ipAddressControl1.AutoHeight = true;
      this.ipAddressControl1.BackColor = System.Drawing.SystemColors.Window;
      this.ipAddressControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.ipAddressControl1.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.ipAddressControl1.Location = new System.Drawing.Point(110, 12);
      this.ipAddressControl1.Name = "ipAddressControl1";
      this.ipAddressControl1.ReadOnly = false;
      this.ipAddressControl1.Size = new System.Drawing.Size(87, 20);
      this.ipAddressControl1.TabIndex = 0;
      this.ipAddressControl1.Text = "...";
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 77);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.ipAddressPortControl1);
      this.Controls.Add(this.ipAddressControl1);
      this.Name = "MainForm";
      this.Text = "Test Control";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private FlexControls.IPAddressControl ipAddressControl1;
    private FlexControls.IPAddressPortControl ipAddressPortControl1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
  }
}

