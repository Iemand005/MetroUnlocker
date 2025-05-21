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
            this.developmentCheckbox = new System.Windows.Forms.CheckBox();
            this.signedCheckbox = new System.Windows.Forms.CheckBox();
            this.allUsersCheckbox = new System.Windows.Forms.CheckBox();
            this.statusTextLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.disableButton = new System.Windows.Forms.Button();
            this.unlockButton = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
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
            // groupBox
            // 
            resources.ApplyResources(this.groupBox, "groupBox");
            this.groupBox.Controls.Add(this.signedCheckbox);
            this.groupBox.Controls.Add(this.allUsersCheckbox);
            this.groupBox.Controls.Add(this.developmentCheckbox);
            this.groupBox.Name = "groupBox";
            this.groupBox.TabStop = false;
            // 
            // disableButton
            // 
            resources.ApplyResources(this.disableButton, "disableButton");
            this.disableButton.Name = "disableButton";
            this.disableButton.UseVisualStyleBackColor = true;
            this.disableButton.Click += new System.EventHandler(this.Uninstall);
            // 
            // unlockButton
            // 
            resources.ApplyResources(this.unlockButton, "unlockButton");
            this.unlockButton.Name = "unlockButton";
            this.unlockButton.UseVisualStyleBackColor = true;
            this.unlockButton.Click += new System.EventHandler(this.Jailbreak);
            // 
            // App
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.disableButton);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.unlockButton);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.statusTextLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "App";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox developmentCheckbox;
        private System.Windows.Forms.CheckBox signedCheckbox;
        private System.Windows.Forms.CheckBox allUsersCheckbox;
        private System.Windows.Forms.Label statusTextLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Button unlockButton;
        private System.Windows.Forms.Button disableButton;
    }
}

