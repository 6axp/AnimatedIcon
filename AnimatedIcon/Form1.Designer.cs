
namespace AnimatedIcon
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
            this.components = new System.ComponentModel.Container();
            this.DebugText = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.imgPreview = new System.Windows.Forms.PictureBox();
            this.barPreview = new System.Windows.Forms.TrackBar();
            this.lstPreviewImages = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // DebugText
            // 
            this.DebugText.Location = new System.Drawing.Point(61, 153);
            this.DebugText.Multiline = true;
            this.DebugText.Name = "DebugText";
            this.DebugText.Size = new System.Drawing.Size(356, 451);
            this.DebugText.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(475, 131);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(124, 50);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Sensivity";
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 10;
            this.trackBar1.Location = new System.Drawing.Point(156, 49);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(269, 69);
            this.trackBar1.TabIndex = 3;
            this.trackBar1.Value = 50;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(484, 288);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(124, 50);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // imgPreview
            // 
            this.imgPreview.Location = new System.Drawing.Point(991, 163);
            this.imgPreview.Name = "imgPreview";
            this.imgPreview.Size = new System.Drawing.Size(64, 64);
            this.imgPreview.TabIndex = 5;
            this.imgPreview.TabStop = false;
            // 
            // barPreview
            // 
            this.barPreview.LargeChange = 10;
            this.barPreview.Location = new System.Drawing.Point(684, 418);
            this.barPreview.Maximum = 100;
            this.barPreview.Name = "barPreview";
            this.barPreview.Size = new System.Drawing.Size(269, 69);
            this.barPreview.TabIndex = 6;
            this.barPreview.Value = 50;
            this.barPreview.Scroll += new System.EventHandler(this.barPreview_Scroll);
            // 
            // lstPreviewImages
            // 
            this.lstPreviewImages.FormattingEnabled = true;
            this.lstPreviewImages.ItemHeight = 20;
            this.lstPreviewImages.Location = new System.Drawing.Point(684, 163);
            this.lstPreviewImages.Name = "lstPreviewImages";
            this.lstPreviewImages.Size = new System.Drawing.Size(290, 224);
            this.lstPreviewImages.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1360, 662);
            this.Controls.Add(this.lstPreviewImages);
            this.Controls.Add(this.barPreview);
            this.Controls.Add(this.imgPreview);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.DebugText);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox DebugText;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox imgPreview;
        private System.Windows.Forms.TrackBar barPreview;
        private System.Windows.Forms.ListBox lstPreviewImages;
    }
}

