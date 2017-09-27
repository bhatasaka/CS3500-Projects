using System;
using System.Windows.Forms;

namespace TipCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void computeTipButtonClick(object sender, EventArgs e)
        {
            computeTip();
        }

        private void initialTotalBillTextbox_TextChanged(object sender, EventArgs e)
        {
            if (validTextEntered())
            {
                computeTip();
            }
        }

        private void tipTextbox_TextChanged(object sender, EventArgs e)
        {
            if (validTextEntered())
            {
                computeTip();
            }
        }

        private bool validTextEntered()
        {
            if(Double.TryParse(tipTextbox.Text, out double result) && Double.TryParse(initialBillTotalTextbox.Text, out double result2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void computeTip()
        {
            double initialBillTotal = Convert.ToDouble(initialBillTotalTextbox.Text);
            double tip = initialBillTotal * Convert.ToDouble(tipTextbox.Text) / 100;
            finalTipBox.Text = tip.ToString();
            totalFinalAmountTextbox.Text = tip + initialBillTotal + "";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
