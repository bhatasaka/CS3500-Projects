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
            this.billTotalTextBox = new System.Windows.Forms.TextBox();
            this.finalTipBox = new System.Windows.Forms.TextBox();
            this.computeTipButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // billTotalLabel
            // 
            this.billTotalLabel.AutoSize = true;
            this.billTotalLabel.Location = new System.Drawing.Point(279, 176);
            this.billTotalLabel.Name = "billTotalLabel";
            this.billTotalLabel.Size = new System.Drawing.Size(152, 25);
            this.billTotalLabel.TabIndex = 0;
            this.billTotalLabel.Text = "Enter Bill Total";
            this.billTotalLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // billTotalTextBox
            // 
            this.billTotalTextBox.Location = new System.Drawing.Point(519, 176);
            this.billTotalTextBox.Name = "billTotalTextBox";
            this.billTotalTextBox.Size = new System.Drawing.Size(100, 31);
            this.billTotalTextBox.TabIndex = 1;
            this.billTotalTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // finalTipBox
            // 
            this.finalTipBox.Location = new System.Drawing.Point(519, 286);
            this.finalTipBox.Name = "finalTipBox";
            this.finalTipBox.Size = new System.Drawing.Size(100, 31);
            this.finalTipBox.TabIndex = 2;
            this.finalTipBox.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // computeTipButton
            // 
            this.computeTipButton.Location = new System.Drawing.Point(284, 270);
            this.computeTipButton.Name = "computeTipButton";
            this.computeTipButton.Size = new System.Drawing.Size(135, 63);
            this.computeTipButton.TabIndex = 3;
            this.computeTipButton.Text = "Compute Tip";
            this.computeTipButton.UseVisualStyleBackColor = true;
            this.computeTipButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 582);
            this.Controls.Add(this.computeTipButton);
            this.Controls.Add(this.finalTipBox);
            this.Controls.Add(this.billTotalTextBox);
            this.Controls.Add(this.billTotalLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label billTotalLabel;
        private System.Windows.Forms.TextBox billTotalTextBox;
        private System.Windows.Forms.TextBox finalTipBox;
        private System.Windows.Forms.Button computeTipButton;
    }
}

