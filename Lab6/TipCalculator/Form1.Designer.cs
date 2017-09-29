namespace TipCalculator
{
    partial class Form1
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
            this.billTotalLabel = new System.Windows.Forms.Label();
            this.initialBillTotalTextbox = new System.Windows.Forms.TextBox();
            this.finalTipBox = new System.Windows.Forms.TextBox();
            this.tipTextbox = new System.Windows.Forms.TextBox();
            this.tipLabel = new System.Windows.Forms.Label();
            this.totalfinalAmountLabel = new System.Windows.Forms.Label();
            this.totalFinalAmountTextbox = new System.Windows.Forms.TextBox();
            this.tipAmountLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // billTotalLabel
            // 
            this.billTotalLabel.AutoSize = true;
            this.billTotalLabel.ForeColor = System.Drawing.Color.White;
            this.billTotalLabel.Location = new System.Drawing.Point(202, 83);
            this.billTotalLabel.Name = "billTotalLabel";
            this.billTotalLabel.Size = new System.Drawing.Size(152, 25);
            this.billTotalLabel.TabIndex = 0;
            this.billTotalLabel.Text = "Enter Bill Total";
            // 
            // initialBillTotalTextbox
            // 
            this.initialBillTotalTextbox.BackColor = System.Drawing.SystemColors.Control;
            this.initialBillTotalTextbox.Location = new System.Drawing.Point(442, 83);
            this.initialBillTotalTextbox.Name = "initialBillTotalTextbox";
            this.initialBillTotalTextbox.Size = new System.Drawing.Size(100, 31);
            this.initialBillTotalTextbox.TabIndex = 1;
            this.initialBillTotalTextbox.TextChanged += new System.EventHandler(this.initialTotalBillTextbox_TextChanged);
            // 
            // finalTipBox
            // 
            this.finalTipBox.Location = new System.Drawing.Point(442, 259);
            this.finalTipBox.Name = "finalTipBox";
            this.finalTipBox.Size = new System.Drawing.Size(100, 31);
            this.finalTipBox.TabIndex = 5;
            // 
            // tipTextbox
            // 
            this.tipTextbox.Location = new System.Drawing.Point(442, 166);
            this.tipTextbox.Name = "tipTextbox";
            this.tipTextbox.Size = new System.Drawing.Size(100, 31);
            this.tipTextbox.TabIndex = 3;
            this.tipTextbox.TextChanged += new System.EventHandler(this.tipTextbox_TextChanged);
            // 
            // tipLabel
            // 
            this.tipLabel.AutoSize = true;
            this.tipLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.tipLabel.Location = new System.Drawing.Point(202, 166);
            this.tipLabel.Name = "tipLabel";
            this.tipLabel.Size = new System.Drawing.Size(158, 25);
            this.tipLabel.TabIndex = 2;
            this.tipLabel.Text = "Tip Percentage";
            // 
            // totalfinalAmountLabel
            // 
            this.totalfinalAmountLabel.AutoSize = true;
            this.totalfinalAmountLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.totalfinalAmountLabel.Location = new System.Drawing.Point(202, 347);
            this.totalfinalAmountLabel.Name = "totalfinalAmountLabel";
            this.totalfinalAmountLabel.Size = new System.Drawing.Size(213, 25);
            this.totalfinalAmountLabel.TabIndex = 6;
            this.totalfinalAmountLabel.Text = "Total Amount To Pay";
            // 
            // totalFinalAmountTextbox
            // 
            this.totalFinalAmountTextbox.Location = new System.Drawing.Point(442, 344);
            this.totalFinalAmountTextbox.Name = "totalFinalAmountTextbox";
            this.totalFinalAmountTextbox.Size = new System.Drawing.Size(100, 31);
            this.totalFinalAmountTextbox.TabIndex = 7;
            // 
            // tipAmountLabel
            // 
            this.tipAmountLabel.AutoSize = true;
            this.tipAmountLabel.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.tipAmountLabel.Location = new System.Drawing.Point(202, 262);
            this.tipAmountLabel.Name = "tipAmountLabel";
            this.tipAmountLabel.Size = new System.Drawing.Size(121, 25);
            this.tipAmountLabel.TabIndex = 4;
            this.tipAmountLabel.Text = "Tip Amount";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(770, 502);
            this.Controls.Add(this.tipAmountLabel);
            this.Controls.Add(this.totalFinalAmountTextbox);
            this.Controls.Add(this.totalfinalAmountLabel);
            this.Controls.Add(this.tipLabel);
            this.Controls.Add(this.tipTextbox);
            this.Controls.Add(this.finalTipBox);
            this.Controls.Add(this.initialBillTotalTextbox);
            this.Controls.Add(this.billTotalLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label billTotalLabel;
        private System.Windows.Forms.TextBox initialBillTotalTextbox;
        private System.Windows.Forms.TextBox finalTipBox;
        private System.Windows.Forms.TextBox tipTextbox;
        private System.Windows.Forms.Label tipLabel;
        private System.Windows.Forms.Label totalfinalAmountLabel;
        private System.Windows.Forms.TextBox totalFinalAmountTextbox;
        private System.Windows.Forms.Label tipAmountLabel;
    }
}

