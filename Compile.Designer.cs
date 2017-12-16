namespace ArduinoCPP
{
    partial class Compile
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
            this.BUT_CompileIt = new System.Windows.Forms.Button();
            this.CMB_mcu = new System.Windows.Forms.ComboBox();
            this.BUT_UploadIt = new System.Windows.Forms.Button();
            this.TXT_hex = new System.Windows.Forms.TextBox();
            this.CMB_comport = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.FB_arduino = new ArduinoCPP.FileBrowse();
            this.FBpde = new ArduinoCPP.FileBrowse();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.CMB_hal_board = new System.Windows.Forms.ComboBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BUT_CompileIt
            // 
            this.BUT_CompileIt.Location = new System.Drawing.Point(187, 119);
            this.BUT_CompileIt.Name = "BUT_CompileIt";
            this.BUT_CompileIt.Size = new System.Drawing.Size(75, 23);
            this.BUT_CompileIt.TabIndex = 3;
            this.BUT_CompileIt.Text = "Compile It";
            this.BUT_CompileIt.UseVisualStyleBackColor = true;
            this.BUT_CompileIt.Click += new System.EventHandler(this.BUT_CompileIt_Click);
            // 
            // CMB_mcu
            // 
            this.CMB_mcu.FormattingEnabled = true;
            this.CMB_mcu.Items.AddRange(new object[] {
            "atmega2560",
            "atmega1280",
            "atmega328p"});
            this.CMB_mcu.Location = new System.Drawing.Point(100, 91);
            this.CMB_mcu.Name = "CMB_mcu";
            this.CMB_mcu.Size = new System.Drawing.Size(121, 21);
            this.CMB_mcu.TabIndex = 4;
            this.CMB_mcu.Text = "atmega2560";
            // 
            // BUT_UploadIt
            // 
            this.BUT_UploadIt.Location = new System.Drawing.Point(187, 201);
            this.BUT_UploadIt.Name = "BUT_UploadIt";
            this.BUT_UploadIt.Size = new System.Drawing.Size(75, 23);
            this.BUT_UploadIt.TabIndex = 5;
            this.BUT_UploadIt.Text = "Upload It";
            this.BUT_UploadIt.UseVisualStyleBackColor = true;
            this.BUT_UploadIt.Click += new System.EventHandler(this.BUT_UploadIt_Click);
            // 
            // TXT_hex
            // 
            this.TXT_hex.Location = new System.Drawing.Point(105, 175);
            this.TXT_hex.Name = "TXT_hex";
            this.TXT_hex.Size = new System.Drawing.Size(253, 20);
            this.TXT_hex.TabIndex = 6;
            // 
            // CMB_comport
            // 
            this.CMB_comport.FormattingEnabled = true;
            this.CMB_comport.Location = new System.Drawing.Point(162, 148);
            this.CMB_comport.Name = "CMB_comport";
            this.CMB_comport.Size = new System.Drawing.Size(121, 21);
            this.CMB_comport.TabIndex = 7;
            this.CMB_comport.Click += new System.EventHandler(this.CMB_comport_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 469);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(450, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(170, 17);
            this.toolStripStatusLabel1.Text = "   ";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(250, 16);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(331, 205);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FB_arduino
            // 
            this.FB_arduino.FileName = "C:\\arduino-1.0.1\\arduino.exe";
            this.FB_arduino.filter = "Arduino|*.exe";
            this.FB_arduino.label = "Arduino Files";
            this.FB_arduino.Location = new System.Drawing.Point(12, 54);
            this.FB_arduino.Name = "FB_arduino";
            this.FB_arduino.Size = new System.Drawing.Size(424, 35);
            this.FB_arduino.TabIndex = 9;
            // 
            // FBpde
            // 
            this.FBpde.FileName = "C:\\Users\\hog\\Desktop\\DIYDrones\\ardupilot-mega\\";
            this.FBpde.filter = "pde or ino|*.pde;*.ino";
            this.FBpde.label = "Main Pde";
            this.FBpde.Location = new System.Drawing.Point(13, 13);
            this.FBpde.Name = "FBpde";
            this.FBpde.Size = new System.Drawing.Size(424, 35);
            this.FBpde.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 234);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(426, 232);
            this.textBox1.TabIndex = 11;
            // 
            // CMB_hal_board
            // 
            this.CMB_hal_board.FormattingEnabled = true;
            this.CMB_hal_board.Items.AddRange(new object[] {
            "HAL_BOARD_APM1",
            "HAL_BOARD_APM2"});
            this.CMB_hal_board.Location = new System.Drawing.Point(227, 91);
            this.CMB_hal_board.Name = "CMB_hal_board";
            this.CMB_hal_board.Size = new System.Drawing.Size(131, 21);
            this.CMB_hal_board.TabIndex = 12;
            this.CMB_hal_board.Text = "HAL_BOARD_APM2";
            // 
            // Compile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 491);
            this.Controls.Add(this.CMB_hal_board);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FB_arduino);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.CMB_comport);
            this.Controls.Add(this.TXT_hex);
            this.Controls.Add(this.BUT_UploadIt);
            this.Controls.Add(this.CMB_mcu);
            this.Controls.Add(this.BUT_CompileIt);
            this.Controls.Add(this.FBpde);
            this.Name = "Compile";
            this.Text = "Compile by Michael Oborne";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FileBrowse FBpde;
        private System.Windows.Forms.Button BUT_CompileIt;
        private System.Windows.Forms.ComboBox CMB_mcu;
        private System.Windows.Forms.Button BUT_UploadIt;
        private System.Windows.Forms.TextBox TXT_hex;
        private System.Windows.Forms.ComboBox CMB_comport;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private FileBrowse FB_arduino;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox CMB_hal_board;

    }
}