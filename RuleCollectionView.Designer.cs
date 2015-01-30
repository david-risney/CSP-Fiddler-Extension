namespace FiddlerCSP
{
    partial class RuleCollectionView
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
            this.EnableRuleCollectionCheckBox = new System.Windows.Forms.CheckBox();
            this.VerboseLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.RuleCollectionListView = new System.Windows.Forms.ListView();
            this.URI = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ContentSecurityPolicy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectedRuleText = new System.Windows.Forms.TextBox();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // EnableRuleCollectionCheckBox
            // 
            this.EnableRuleCollectionCheckBox.AutoSize = true;
            this.EnableRuleCollectionCheckBox.Location = new System.Drawing.Point(3, 3);
            this.EnableRuleCollectionCheckBox.Name = "EnableRuleCollectionCheckBox";
            this.EnableRuleCollectionCheckBox.Size = new System.Drawing.Size(133, 17);
            this.EnableRuleCollectionCheckBox.TabIndex = 0;
            this.EnableRuleCollectionCheckBox.Text = "Enable Rule Collection";
            this.EnableRuleCollectionCheckBox.UseVisualStyleBackColor = true;
            this.EnableRuleCollectionCheckBox.CheckedChanged += new System.EventHandler(this.EnableRuleCollectionCheckBox_CheckedChanged);
            // 
            // VerboseLoggingCheckBox
            // 
            this.VerboseLoggingCheckBox.AutoSize = true;
            this.VerboseLoggingCheckBox.Location = new System.Drawing.Point(4, 31);
            this.VerboseLoggingCheckBox.Name = "VerboseLoggingCheckBox";
            this.VerboseLoggingCheckBox.Size = new System.Drawing.Size(106, 17);
            this.VerboseLoggingCheckBox.TabIndex = 1;
            this.VerboseLoggingCheckBox.Text = "Verbose Logging";
            this.VerboseLoggingCheckBox.UseVisualStyleBackColor = true;
            this.VerboseLoggingCheckBox.CheckedChanged += new System.EventHandler(this.VerboseLoggingCheckBox_CheckedChanged);
            // 
            // RuleCollectionListView
            // 
            this.RuleCollectionListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RuleCollectionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.URI,
            this.ContentSecurityPolicy});
            this.RuleCollectionListView.FullRowSelect = true;
            this.RuleCollectionListView.GridLines = true;
            this.RuleCollectionListView.HideSelection = false;
            this.RuleCollectionListView.Location = new System.Drawing.Point(4, 59);
            this.RuleCollectionListView.Name = "RuleCollectionListView";
            this.RuleCollectionListView.Size = new System.Drawing.Size(517, 354);
            this.RuleCollectionListView.TabIndex = 2;
            this.RuleCollectionListView.UseCompatibleStateImageBehavior = false;
            this.RuleCollectionListView.View = System.Windows.Forms.View.Details;
            // 
            // URI
            // 
            this.URI.Text = "URI";
            // 
            // ContentSecurityPolicy
            // 
            this.ContentSecurityPolicy.Text = "Content Security Policy";
            this.ContentSecurityPolicy.Width = 420;
            // 
            // SelectedRuleText
            // 
            this.SelectedRuleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectedRuleText.Location = new System.Drawing.Point(4, 420);
            this.SelectedRuleText.Multiline = true;
            this.SelectedRuleText.Name = "SelectedRuleText";
            this.SelectedRuleText.ReadOnly = true;
            this.SelectedRuleText.Size = new System.Drawing.Size(517, 204);
            this.SelectedRuleText.TabIndex = 3;
            // 
            // lnkHelp
            // 
            this.lnkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.Location = new System.Drawing.Point(466, 7);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(38, 13);
            this.lnkHelp.TabIndex = 4;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Help...";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // RuleCollectionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.SelectedRuleText);
            this.Controls.Add(this.RuleCollectionListView);
            this.Controls.Add(this.VerboseLoggingCheckBox);
            this.Controls.Add(this.EnableRuleCollectionCheckBox);
            this.Name = "RuleCollectionView";
            this.Size = new System.Drawing.Size(524, 627);
            this.Load += new System.EventHandler(this.RuleCollectionView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox EnableRuleCollectionCheckBox;
        private System.Windows.Forms.CheckBox VerboseLoggingCheckBox;
        private System.Windows.Forms.ListView RuleCollectionListView;
        private System.Windows.Forms.ColumnHeader URI;
        private System.Windows.Forms.ColumnHeader ContentSecurityPolicy;
        private System.Windows.Forms.TextBox SelectedRuleText;
        private System.Windows.Forms.LinkLabel lnkHelp;
    }
}
