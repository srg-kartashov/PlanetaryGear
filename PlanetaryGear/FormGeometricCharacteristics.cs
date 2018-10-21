using System;
using System.Windows.Forms;

namespace PlanetaryGear
{
    public partial class FormGeometricCharacteristics : Form
    {
        public FormGeometricCharacteristics()
        {
            InitializeComponent();
            numericUpDown1.Value = Form1.R1;
            numericUpDown2.Value = Form1.R2;
            numericUpDown3.Value = Form1.R3;
        }


        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                Form1.R1 = (int)numericUpDown1.Value;
                Form1.R2 = (int)numericUpDown2.Value;
                Form1.R3 = (int)numericUpDown3.Value;
            }
        }



        private void Form2_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = Form1.R1;
            numericUpDown2.Value = Form1.R2;
            numericUpDown3.Value = Form1.R3;
        }
    }
}
