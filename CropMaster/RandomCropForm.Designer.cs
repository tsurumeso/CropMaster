namespace CropMaster
{
    partial class RandomCropForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownEx1 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx2 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx3 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx4 = new CropMaster.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx4)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "幅: ";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(251, 141);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 14);
            this.label2.TabIndex = 7;
            this.label2.Text = "最小値:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(188, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 14);
            this.label3.TabIndex = 10;
            this.label3.Text = "最大値:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(159, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 14);
            this.label4.TabIndex = 5;
            this.label4.Text = "pix";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(159, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 14);
            this.label5.TabIndex = 9;
            this.label5.Text = "pix";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(315, 116);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(23, 14);
            this.label6.TabIndex = 12;
            this.label6.Text = "pix";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(158, 141);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "キャンセル";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(12, 39);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(98, 18);
            this.radioButton1.TabIndex = 2;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "サイズを指定する";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(12, 90);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(140, 18);
            this.radioButton2.TabIndex = 6;
            this.radioButton2.Text = "サイズをランダムに決定する";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 14);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 14);
            this.label8.TabIndex = 0;
            this.label8.Text = "クロップ数:";
            // 
            // numericUpDownEx1
            // 
            this.numericUpDownEx1.Location = new System.Drawing.Point(71, 12);
            this.numericUpDownEx1.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownEx1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx1.Name = "numericUpDownEx1";
            this.numericUpDownEx1.Size = new System.Drawing.Size(70, 21);
            this.numericUpDownEx1.TabIndex = 15;
            this.numericUpDownEx1.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // numericUpDownEx2
            // 
            this.numericUpDownEx2.Location = new System.Drawing.Point(83, 114);
            this.numericUpDownEx2.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownEx2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx2.Name = "numericUpDownEx2";
            this.numericUpDownEx2.Size = new System.Drawing.Size(70, 21);
            this.numericUpDownEx2.TabIndex = 16;
            this.numericUpDownEx2.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // numericUpDownEx3
            // 
            this.numericUpDownEx3.Location = new System.Drawing.Point(239, 114);
            this.numericUpDownEx3.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownEx3.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx3.Name = "numericUpDownEx3";
            this.numericUpDownEx3.Size = new System.Drawing.Size(70, 21);
            this.numericUpDownEx3.TabIndex = 17;
            this.numericUpDownEx3.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // numericUpDownEx4
            // 
            this.numericUpDownEx4.Location = new System.Drawing.Point(83, 63);
            this.numericUpDownEx4.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownEx4.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx4.Name = "numericUpDownEx4";
            this.numericUpDownEx4.Size = new System.Drawing.Size(70, 21);
            this.numericUpDownEx4.TabIndex = 18;
            this.numericUpDownEx4.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // RandomCropForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 176);
            this.ControlBox = false;
            this.Controls.Add(this.numericUpDownEx4);
            this.Controls.Add(this.numericUpDownEx3);
            this.Controls.Add(this.numericUpDownEx2);
            this.Controls.Add(this.numericUpDownEx1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Meiryo UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximumSize = new System.Drawing.Size(366, 215);
            this.MinimumSize = new System.Drawing.Size(366, 215);
            this.Name = "RandomCropForm";
            this.Text = "ランダムクロップ";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Label label8;
        private NumericUpDownEx numericUpDownEx1;
        private NumericUpDownEx numericUpDownEx2;
        private NumericUpDownEx numericUpDownEx3;
        private NumericUpDownEx numericUpDownEx4;
    }
}