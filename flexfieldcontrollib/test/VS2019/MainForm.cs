using System;
using System.Windows.Forms;

using FlexControls;

namespace TestControl
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      using (DefaultControl ctrl = new DefaultControl())
      {
        Console.WriteLine(ctrl.Text);
      }
    }
  }
}
