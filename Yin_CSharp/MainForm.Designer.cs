namespace Yin_CSharp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtInput = new TextBox();
            lbInput = new Label();
            btnInput = new Button();
            btnTest = new Button();
            label1 = new Label();
            labelF0 = new Label();
            SuspendLayout();
            // 
            // txtInput
            // 
            txtInput.Location = new Point(53, 12);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(410, 23);
            txtInput.TabIndex = 0;
            // 
            // lbInput
            // 
            lbInput.AutoSize = true;
            lbInput.Location = new Point(12, 15);
            lbInput.Name = "lbInput";
            lbInput.Size = new Size(35, 15);
            lbInput.TabIndex = 1;
            lbInput.Text = "Input";
            // 
            // btnInput
            // 
            btnInput.Location = new Point(469, 12);
            btnInput.Name = "btnInput";
            btnInput.Size = new Size(75, 23);
            btnInput.TabIndex = 2;
            btnInput.Text = "browser";
            btnInput.UseVisualStyleBackColor = true;
            btnInput.Click += btnInput_Click;
            // 
            // btnTest
            // 
            btnTest.Location = new Point(550, 11);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(75, 23);
            btnTest.TabIndex = 3;
            btnTest.Text = "test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(53, 38);
            label1.Name = "label1";
            label1.Size = new Size(101, 15);
            label1.TabIndex = 4;
            label1.Text = "tần số dự đoán là:";
            // 
            // labelF0
            // 
            labelF0.AutoSize = true;
            labelF0.Location = new Point(160, 38);
            labelF0.Name = "labelF0";
            labelF0.Size = new Size(0, 15);
            labelF0.TabIndex = 5;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(labelF0);
            Controls.Add(label1);
            Controls.Add(btnTest);
            Controls.Add(btnInput);
            Controls.Add(lbInput);
            Controls.Add(txtInput);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtInput;
        private Label lbInput;
        private Button btnInput;
        private Button btnTest;
        private Label label1;
        private Label labelF0;
    }
}
