namespace TwitchVodDownloaderSharp.UI
{
    partial class VodDownloadPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.previewBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new TwitchVodDownloaderSharp.UI.MouseTransparentPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.statusLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.qualityLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.recordedLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.label6 = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.label5 = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.lableR = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.lengthLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.gameLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.channelLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.label3 = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.label2 = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.label1 = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.titleLabel = new TwitchVodDownloaderSharp.UI.MouseTransparentLabel();
            this.downloadProgress = new TwitchVodDownloaderSharp.UI.MouseTransparentProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // previewBox
            // 
            this.previewBox.Location = new System.Drawing.Point(3, 3);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(160, 90);
            this.previewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewBox.TabIndex = 0;
            this.previewBox.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.statusLabel);
            this.panel1.Controls.Add(this.qualityLabel);
            this.panel1.Controls.Add(this.recordedLabel);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lableR);
            this.panel1.Controls.Add(this.lengthLabel);
            this.panel1.Controls.Add(this.gameLabel);
            this.panel1.Controls.Add(this.channelLabel);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.titleLabel);
            this.panel1.Controls.Add(this.downloadProgress);
            this.panel1.Location = new System.Drawing.Point(162, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(535, 96);
            this.panel1.TabIndex = 5;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(505, 0);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(30, 20);
            this.cancelButton.TabIndex = 18;
            this.cancelButton.Text = "X";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            this.cancelButton.MouseLeave += new System.EventHandler(this.CancelButton_MouseLeave);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(463, 56);
            this.statusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(69, 13);
            this.statusLabel.TabIndex = 17;
            this.statusLabel.Text = "Downloading";
            // 
            // qualityLabel
            // 
            this.qualityLabel.AutoSize = true;
            this.qualityLabel.Location = new System.Drawing.Point(340, 24);
            this.qualityLabel.Margin = new System.Windows.Forms.Padding(0);
            this.qualityLabel.Name = "qualityLabel";
            this.qualityLabel.Size = new System.Drawing.Size(155, 13);
            this.qualityLabel.TabIndex = 16;
            this.qualityLabel.Text = "9999x9999@99.99fps (Source)";
            // 
            // recordedLabel
            // 
            this.recordedLabel.AutoSize = true;
            this.recordedLabel.Location = new System.Drawing.Point(72, 56);
            this.recordedLabel.Margin = new System.Windows.Forms.Padding(0);
            this.recordedLabel.Name = "recordedLabel";
            this.recordedLabel.Size = new System.Drawing.Size(129, 13);
            this.recordedLabel.TabIndex = 15;
            this.recordedLabel.Text = "99/99/9999 99:99:99 AM";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(413, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Status:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(287, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Quality:";
            // 
            // lableR
            // 
            this.lableR.AutoSize = true;
            this.lableR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lableR.Location = new System.Drawing.Point(3, 56);
            this.lableR.Name = "lableR";
            this.lableR.Size = new System.Drawing.Size(66, 13);
            this.lableR.TabIndex = 12;
            this.lableR.Text = "Recorded:";
            // 
            // lengthLabel
            // 
            this.lengthLabel.AutoSize = true;
            this.lengthLabel.Location = new System.Drawing.Point(340, 56);
            this.lengthLabel.Margin = new System.Windows.Forms.Padding(0);
            this.lengthLabel.Name = "lengthLabel";
            this.lengthLabel.Size = new System.Drawing.Size(49, 13);
            this.lengthLabel.TabIndex = 11;
            this.lengthLabel.Text = "99:99:99";
            // 
            // gameLabel
            // 
            this.gameLabel.AutoSize = true;
            this.gameLabel.Location = new System.Drawing.Point(72, 40);
            this.gameLabel.Name = "gameLabel";
            this.gameLabel.Size = new System.Drawing.Size(260, 13);
            this.gameLabel.TabIndex = 10;
            this.gameLabel.Text = "Dummy Game With Kindof Long Name: GOTY Edition";
            // 
            // channelLabel
            // 
            this.channelLabel.AutoSize = true;
            this.channelLabel.Location = new System.Drawing.Point(72, 24);
            this.channelLabel.Name = "channelLabel";
            this.channelLabel.Size = new System.Drawing.Size(118, 13);
            this.channelLabel.TabIndex = 9;
            this.channelLabel.Text = "Dummy Streamer Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(287, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Length:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Game:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Channel:";
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(3, 3);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(3);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(394, 18);
            this.titleLabel.TabIndex = 5;
            this.titleLabel.Text = "Dummy Stream Title That To Show In The Designer";
            // 
            // downloadProgress
            // 
            this.downloadProgress.Location = new System.Drawing.Point(3, 70);
            this.downloadProgress.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(532, 23);
            this.downloadProgress.TabIndex = 3;
            // 
            // VodDownloadPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.previewBox);
            this.Name = "VodDownloadPanel";
            this.Size = new System.Drawing.Size(700, 96);
            this.MouseEnter += new System.EventHandler(this.VodDownloadPanel_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.VodDownloadPanel_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox previewBox;
        private MouseTransparentPanel panel1;
        private MouseTransparentLabel titleLabel;
        private MouseTransparentProgressBar downloadProgress;
        private MouseTransparentLabel label2;
        private MouseTransparentLabel label1;
        private MouseTransparentLabel lengthLabel;
        private MouseTransparentLabel gameLabel;
        private MouseTransparentLabel channelLabel;
        private MouseTransparentLabel label3;
        private MouseTransparentLabel statusLabel;
        private MouseTransparentLabel qualityLabel;
        private MouseTransparentLabel recordedLabel;
        private MouseTransparentLabel label6;
        private MouseTransparentLabel label5;
        private MouseTransparentLabel lableR;
        private System.Windows.Forms.Button cancelButton;
    }
}
