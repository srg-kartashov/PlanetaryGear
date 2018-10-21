using System;
using System.Windows.Forms;
 

namespace PlanetaryGear
{
    public partial class FormAngularVelocity : Form
    {
        public FormAngularVelocity()
        {
            InitializeComponent();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            double omega = 0;
            if (DialogResult != DialogResult.OK) return;
            try
            {

                omega = Convert.ToDouble(maskedTextBox1.Text.Substring(0, 4));
            }
            catch (FormatException)
            {
                e.Cancel = true;
            }
            if (omega >0.2)
            {
                MessageBox.Show(@"Введите значение меньше 0.2 рад/с");
                e.Cancel = true;
                omega = 0.2;
            }
            Form1.Omega = omega;
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
