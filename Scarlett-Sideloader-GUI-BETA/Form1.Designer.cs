﻿namespace Scarlett_Sideloader_GUI_BETA
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.aspcookies = new System.Windows.Forms.TextBox();
            this.GroupBoxes = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.appfile = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.upload = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.statusmessage = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.game = new System.Windows.Forms.CheckBox();
            this.app = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(445, 34);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 27);
            this.button1.TabIndex = 0;
            this.button1.Text = "login";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = ".AspNet.Cookies:";
            // 
            // aspcookies
            // 
            this.aspcookies.Location = new System.Drawing.Point(12, 34);
            this.aspcookies.Name = "aspcookies";
            this.aspcookies.Size = new System.Drawing.Size(427, 27);
            this.aspcookies.TabIndex = 2;
            // 
            // GroupBoxes
            // 
            this.GroupBoxes.Enabled = false;
            this.GroupBoxes.Location = new System.Drawing.Point(12, 87);
            this.GroupBoxes.Name = "GroupBoxes";
            this.GroupBoxes.Size = new System.Drawing.Size(427, 92);
            this.GroupBoxes.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Groups:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 232);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "App File:";
            // 
            // appfile
            // 
            this.appfile.Location = new System.Drawing.Point(12, 255);
            this.appfile.Name = "appfile";
            this.appfile.Size = new System.Drawing.Size(427, 27);
            this.appfile.TabIndex = 6;
            this.appfile.TextChanged += new System.EventHandler(this.appfile_TextChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(445, 255);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(60, 27);
            this.button2.TabIndex = 7;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // upload
            // 
            this.upload.Enabled = false;
            this.upload.Location = new System.Drawing.Point(12, 288);
            this.upload.Name = "upload";
            this.upload.Size = new System.Drawing.Size(94, 29);
            this.upload.TabIndex = 8;
            this.upload.Text = "Upload";
            this.upload.UseVisualStyleBackColor = true;
            this.upload.Click += new System.EventHandler(this.upload_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(112, 292);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "Status:";
            // 
            // statusmessage
            // 
            this.statusmessage.AutoSize = true;
            this.statusmessage.Location = new System.Drawing.Point(170, 292);
            this.statusmessage.Name = "statusmessage";
            this.statusmessage.Size = new System.Drawing.Size(0, 20);
            this.statusmessage.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 320);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 20);
            this.label5.TabIndex = 12;
            this.label5.Text = "App Link: ";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(92, 320);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(0, 20);
            this.linkLabel1.TabIndex = 13;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // game
            // 
            this.game.AutoSize = true;
            this.game.Checked = true;
            this.game.CheckState = System.Windows.Forms.CheckState.Checked;
            this.game.Location = new System.Drawing.Point(12, 205);
            this.game.Name = "game";
            this.game.Size = new System.Drawing.Size(70, 24);
            this.game.TabIndex = 14;
            this.game.Text = "Game";
            this.game.UseVisualStyleBackColor = true;
            this.game.CheckedChanged += new System.EventHandler(this.game_CheckedChanged);
            // 
            // app
            // 
            this.app.AutoSize = true;
            this.app.Location = new System.Drawing.Point(88, 205);
            this.app.Name = "app";
            this.app.Size = new System.Drawing.Size(59, 24);
            this.app.TabIndex = 15;
            this.app.Text = "App";
            this.app.UseVisualStyleBackColor = true;
            this.app.CheckedChanged += new System.EventHandler(this.app_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "App Type:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(271, 182);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(96, 20);
            this.label7.TabIndex = 17;
            this.label7.Text = "Upload Type:";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(271, 205);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(82, 24);
            this.checkBox1.TabIndex = 18;
            this.checkBox1.Text = "Release";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(359, 205);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(80, 24);
            this.checkBox2.TabIndex = 19;
            this.checkBox2.Text = "Update";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 357);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.app);
            this.Controls.Add(this.game);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.statusmessage);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.upload);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.appfile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.GroupBoxes);
            this.Controls.Add(this.aspcookies);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Scarlet-Sideloader";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button button1;
        private Label label1;
        private TextBox aspcookies;
        private CheckedListBox GroupBoxes;
        private Label label2;
        private Label label3;
        private TextBox appfile;
        private Button button2;
        private Button upload;
        private Label label4;
        private Label statusmessage;
        private Label label5;
        private LinkLabel linkLabel1;
        private CheckBox game;
        private CheckBox app;
        private Label label6;
        private Label label7;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
    }
}