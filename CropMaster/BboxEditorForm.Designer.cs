namespace CropMaster
{
    partial class BboxEditorForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.File_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OutputAs_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.表示VToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowGrid_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDownEx1 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx2 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx3 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx4 = new CropMaster.NumericUpDownEx();
            this.numericUpDownEx5 = new CropMaster.NumericUpDownEx();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx5)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 27);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(255, 255);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.File_ToolStripMenuItem,
            this.表示VToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(279, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // File_ToolStripMenuItem
            // 
            this.File_ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OutputAs_ToolStripMenuItem});
            this.File_ToolStripMenuItem.Name = "File_ToolStripMenuItem";
            this.File_ToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.File_ToolStripMenuItem.Text = "ファイル(&F)";
            this.File_ToolStripMenuItem.Click += new System.EventHandler(this.File_ToolStripMenuItem_Click);
            // 
            // OutputAs_ToolStripMenuItem
            // 
            this.OutputAs_ToolStripMenuItem.Name = "OutputAs_ToolStripMenuItem";
            this.OutputAs_ToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.OutputAs_ToolStripMenuItem.Text = "名前を付けて保存(&A)...";
            this.OutputAs_ToolStripMenuItem.Click += new System.EventHandler(this.Output_ToolStripMenuItem_Click);
            // 
            // 表示VToolStripMenuItem
            // 
            this.表示VToolStripMenuItem.Checked = true;
            this.表示VToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.表示VToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowGrid_ToolStripMenuItem});
            this.表示VToolStripMenuItem.Name = "表示VToolStripMenuItem";
            this.表示VToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.表示VToolStripMenuItem.Text = "表示(&V)";
            // 
            // ShowGrid_ToolStripMenuItem
            // 
            this.ShowGrid_ToolStripMenuItem.Checked = true;
            this.ShowGrid_ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowGrid_ToolStripMenuItem.Name = "ShowGrid_ToolStripMenuItem";
            this.ShowGrid_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ShowGrid_ToolStripMenuItem.Text = "グリッド";
            this.ShowGrid_ToolStripMenuItem.Click += new System.EventHandler(this.ShowGrid_ToolStripMenuItem_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(12, 288);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(81, 23);
            this.button4.TabIndex = 1;
            this.button4.Text = "<";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(186, 288);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(81, 23);
            this.button5.TabIndex = 2;
            this.button5.Text = ">";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 375);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 14);
            this.label3.TabIndex = 7;
            this.label3.Text = "X :";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 402);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 14);
            this.label5.TabIndex = 8;
            this.label5.Text = "Y :";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 456);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 14);
            this.label1.TabIndex = 10;
            this.label1.Text = "Height :";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 429);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 14);
            this.label7.TabIndex = 9;
            this.label7.Text = "Width :";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(12, 317);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(81, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "拡大";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(99, 317);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(81, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "縮小";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 348);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 14);
            this.label10.TabIndex = 6;
            this.label10.Text = "拡大・縮小幅 :";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(186, 317);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "元に戻す";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // numericUpDownEx1
            // 
            this.numericUpDownEx1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx1.Location = new System.Drawing.Point(186, 346);
            this.numericUpDownEx1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx1.Name = "numericUpDownEx1";
            this.numericUpDownEx1.Size = new System.Drawing.Size(81, 21);
            this.numericUpDownEx1.TabIndex = 11;
            this.numericUpDownEx1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numericUpDownEx2
            // 
            this.numericUpDownEx2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx2.Location = new System.Drawing.Point(186, 373);
            this.numericUpDownEx2.Maximum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.numericUpDownEx2.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.numericUpDownEx2.Name = "numericUpDownEx2";
            this.numericUpDownEx2.Size = new System.Drawing.Size(81, 21);
            this.numericUpDownEx2.TabIndex = 12;
            // 
            // numericUpDownEx3
            // 
            this.numericUpDownEx3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx3.Location = new System.Drawing.Point(186, 400);
            this.numericUpDownEx3.Maximum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.numericUpDownEx3.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            -2147483648});
            this.numericUpDownEx3.Name = "numericUpDownEx3";
            this.numericUpDownEx3.Size = new System.Drawing.Size(81, 21);
            this.numericUpDownEx3.TabIndex = 13;
            // 
            // numericUpDownEx4
            // 
            this.numericUpDownEx4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx4.Location = new System.Drawing.Point(186, 427);
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
            this.numericUpDownEx4.Size = new System.Drawing.Size(81, 21);
            this.numericUpDownEx4.TabIndex = 14;
            this.numericUpDownEx4.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // numericUpDownEx5
            // 
            this.numericUpDownEx5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownEx5.Location = new System.Drawing.Point(186, 454);
            this.numericUpDownEx5.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numericUpDownEx5.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownEx5.Name = "numericUpDownEx5";
            this.numericUpDownEx5.Size = new System.Drawing.Size(81, 21);
            this.numericUpDownEx5.TabIndex = 15;
            this.numericUpDownEx5.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // RectEditorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(279, 487);
            this.Controls.Add(this.numericUpDownEx5);
            this.Controls.Add(this.numericUpDownEx4);
            this.Controls.Add(this.numericUpDownEx3);
            this.Controls.Add(this.numericUpDownEx2);
            this.Controls.Add(this.numericUpDownEx1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Meiryo UI", 8.25F);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(295, 526);
            this.Name = "RectEditorForm";
            this.Text = "選択領域の編集";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx5)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem File_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OutputAs_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowGrid_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 表示VToolStripMenuItem;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button1;
        private NumericUpDownEx numericUpDownEx1;
        private NumericUpDownEx numericUpDownEx2;
        private NumericUpDownEx numericUpDownEx3;
        private NumericUpDownEx numericUpDownEx4;
        private NumericUpDownEx numericUpDownEx5;
    }
}