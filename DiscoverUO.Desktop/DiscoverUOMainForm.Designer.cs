namespace DiscoverUO.Desktop
{
    partial class DiscoverUOMainForm
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
            this.appBar = new MenuStrip();
            this.mainMenuHomeButton = new ToolStripMenuItem();
            this.mainMenuPublicServersButton = new ToolStripMenuItem();
            this.mainMenuAddPublicServerButton = new ToolStripMenuItem();
            this.mainMenuMyServersButton = new ToolStripMenuItem();
            this.mainMenuMyFavoritesButton = new ToolStripMenuItem();
            this.mainMenuAddFavoriteButton = new ToolStripMenuItem();
            this.mainMenuUserButton = new ToolStripMenuItem();
            this.mainMenuAuthenticationButton = new ToolStripMenuItem();
            this.mainMenuDashboardButton = new ToolStripMenuItem();
            this.appBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // appBar
            // 
            this.appBar.Items.AddRange(new ToolStripItem[] { this.mainMenuHomeButton, this.mainMenuPublicServersButton, this.mainMenuMyServersButton, this.mainMenuMyFavoritesButton, this.mainMenuUserButton });
            this.appBar.Location = new Point(0, 0);
            this.appBar.Name = "appBar";
            this.appBar.Size = new Size(800, 24);
            this.appBar.TabIndex = 0;
            this.appBar.Text = "AppBar";
            // 
            // mainMenuHomeButton
            // 
            this.mainMenuHomeButton.Name = "mainMenuHomeButton";
            this.mainMenuHomeButton.Size = new Size(52, 20);
            this.mainMenuHomeButton.Text = "Home";
            // 
            // mainMenuPublicServersButton
            // 
            this.mainMenuPublicServersButton.DropDownItems.AddRange(new ToolStripItem[] { this.mainMenuAddPublicServerButton });
            this.mainMenuPublicServersButton.Name = "mainMenuPublicServersButton";
            this.mainMenuPublicServersButton.Size = new Size(92, 20);
            this.mainMenuPublicServersButton.Text = "Public Servers";
            // 
            // mainMenuAddPublicServerButton
            // 
            this.mainMenuAddPublicServerButton.Name = "mainMenuAddPublicServerButton";
            this.mainMenuAddPublicServerButton.Size = new Size(167, 22);
            this.mainMenuAddPublicServerButton.Text = "Add Public Server";
            // 
            // mainMenuMyServersButton
            // 
            this.mainMenuMyServersButton.Name = "mainMenuMyServersButton";
            this.mainMenuMyServersButton.Size = new Size(76, 20);
            this.mainMenuMyServersButton.Text = "My Servers";
            // 
            // mainMenuMyFavoritesButton
            // 
            this.mainMenuMyFavoritesButton.DropDownItems.AddRange(new ToolStripItem[] { this.mainMenuAddFavoriteButton });
            this.mainMenuMyFavoritesButton.Name = "mainMenuMyFavoritesButton";
            this.mainMenuMyFavoritesButton.Size = new Size(86, 20);
            this.mainMenuMyFavoritesButton.Text = "My Favorites";
            // 
            // mainMenuAddFavoriteButton
            // 
            this.mainMenuAddFavoriteButton.Name = "mainMenuAddFavoriteButton";
            this.mainMenuAddFavoriteButton.Size = new Size(141, 22);
            this.mainMenuAddFavoriteButton.Text = "Add Favorite";
            // 
            // mainMenuUserButton
            // 
            this.mainMenuUserButton.Alignment = ToolStripItemAlignment.Right;
            this.mainMenuUserButton.DropDownItems.AddRange(new ToolStripItem[] { this.mainMenuAuthenticationButton, this.mainMenuDashboardButton });
            this.mainMenuUserButton.Name = "mainMenuUserButton";
            this.mainMenuUserButton.Size = new Size(42, 20);
            this.mainMenuUserButton.Text = "User";
            // 
            // mainMenuAuthenticationButton
            // 
            this.mainMenuAuthenticationButton.Name = "mainMenuAuthenticationButton";
            this.mainMenuAuthenticationButton.Size = new Size(153, 22);
            this.mainMenuAuthenticationButton.Text = "Authentication";
            this.mainMenuAuthenticationButton.Click += this.mainMenuAuthenticationButton_Click;
            // 
            // mainMenuDashboardButton
            // 
            this.mainMenuDashboardButton.Name = "mainMenuDashboardButton";
            this.mainMenuDashboardButton.Size = new Size(153, 22);
            this.mainMenuDashboardButton.Text = "Dashboard";
            // 
            // DiscoverUOMainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.appBar);
            this.Name = "DiscoverUOMainForm";
            this.Text = "DiscoverUO Desktop";
            this.Load += this.DiscoverUOMainForm_Load;
            this.appBar.ResumeLayout(false);
            this.appBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private MenuStrip appBar;
        private ToolStripMenuItem mainMenuHomeButton;
        private ToolStripMenuItem mainMenuPublicServersButton;
        private ToolStripMenuItem mainMenuAddPublicServerButton;
        private ToolStripMenuItem mainMenuMyServersButton;
        private ToolStripMenuItem mainMenuMyFavoritesButton;
        private ToolStripMenuItem mainMenuAddFavoriteButton;
        private ToolStripMenuItem mainMenuUserButton;
        private ToolStripMenuItem mainMenuAuthenticationButton;
        private ToolStripMenuItem mainMenuDashboardButton;
    }
}
