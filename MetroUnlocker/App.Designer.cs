namespace MetroUnlocker
{
    partial class App
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param Name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(App));
            this.temporaryButton = new System.Windows.Forms.Button();
            this.developmentCheckbox = new System.Windows.Forms.CheckBox();
            this.signedCheckbox = new System.Windows.Forms.CheckBox();
            this.allUsersCheckbox = new System.Windows.Forms.CheckBox();
            this.statusTextLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.LOBCheckBox = new System.Windows.Forms.CheckBox();
            this.SPPCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Manual = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.Manual.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // temporaryButton
            // 
            resources.ApplyResources(this.temporaryButton, "temporaryButton");
            this.temporaryButton.Name = "temporaryButton";
            this.temporaryButton.UseVisualStyleBackColor = true;
            this.temporaryButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // developmentCheckbox
            // 
            resources.ApplyResources(this.developmentCheckbox, "developmentCheckbox");
            this.developmentCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this, "DevelopmentEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.developmentCheckbox.Name = "developmentCheckbox";
            this.developmentCheckbox.UseVisualStyleBackColor = true;
            // 
            // signedCheckbox
            // 
            resources.ApplyResources(this.signedCheckbox, "signedCheckbox");
            this.signedCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this, "LOBEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.signedCheckbox.Name = "signedCheckbox";
            this.signedCheckbox.UseVisualStyleBackColor = true;
            // 
            // allUsersCheckbox
            // 
            resources.ApplyResources(this.allUsersCheckbox, "allUsersCheckbox");
            this.allUsersCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this, "SpecialProfilesEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.allUsersCheckbox.Name = "allUsersCheckbox";
            this.allUsersCheckbox.UseVisualStyleBackColor = true;
            // 
            // statusTextLabel
            // 
            resources.ApplyResources(this.statusTextLabel, "statusTextLabel");
            this.statusTextLabel.Name = "statusTextLabel";
            // 
            // statusLabel
            // 
            resources.ApplyResources(this.statusLabel, "statusLabel");
            this.statusLabel.Name = "statusLabel";
            // 
            // LOBCheckBox
            // 
            resources.ApplyResources(this.LOBCheckBox, "LOBCheckBox");
            this.LOBCheckBox.Name = "LOBCheckBox";
            this.LOBCheckBox.UseVisualStyleBackColor = true;
            // 
            // SPPCheckBox
            // 
            resources.ApplyResources(this.SPPCheckBox, "SPPCheckBox");
            this.SPPCheckBox.Name = "SPPCheckBox";
            this.SPPCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.signedCheckbox);
            this.groupBox1.Controls.Add(this.allUsersCheckbox);
            this.groupBox1.Controls.Add(this.developmentCheckbox);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.LOBCheckBox);
            this.groupBox2.Controls.Add(this.SPPCheckBox);
            this.groupBox2.Controls.Add(this.temporaryButton);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // Manual
            // 
            resources.ApplyResources(this.Manual, "Manual");
            this.Manual.Controls.Add(this.tabPage1);
            this.Manual.Controls.Add(this.tabPage2);
            this.Manual.Name = "Manual";
            this.Manual.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.button1);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.JailbreakButton_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Uninstall);
            // 
            // App
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Manual);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.statusTextLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "App";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.Manual.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button temporaryButton;
        private System.Windows.Forms.CheckBox developmentCheckbox;
        private System.Windows.Forms.CheckBox signedCheckbox;
        private System.Windows.Forms.CheckBox allUsersCheckbox;
        private System.Windows.Forms.Label statusTextLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox LOBCheckBox;
        private System.Windows.Forms.CheckBox SPPCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl Manual;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button2;
    }
}

