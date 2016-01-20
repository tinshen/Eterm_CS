namespace Eterm_CS
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.AppContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Exititem = new System.Windows.Forms.ToolStripMenuItem();
            this.Tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.AppContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // AppContextMenu
            // 
            this.AppContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Exititem});
            this.AppContextMenu.Name = "AppContextMenu";
            this.AppContextMenu.ShowImageMargin = false;
            this.AppContextMenu.Size = new System.Drawing.Size(72, 26);
            // 
            // Exititem
            // 
            this.Exititem.Name = "Exititem";
            this.Exititem.Size = new System.Drawing.Size(71, 22);
            this.Exititem.Text = "Exit";
            this.Exititem.Click += new System.EventHandler(this.Exititem_Click);
            // 
            // Tray
            // 
            this.Tray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.Tray.BalloonTipText = "后台程序已运行，如需打开\r\n请双击此图标或右键打开";
            this.Tray.BalloonTipTitle = "订座系统";
            this.Tray.ContextMenuStrip = this.AppContextMenu;
            this.Tray.Icon = ((System.Drawing.Icon)(resources.GetObject("Tray.Icon")));
            this.Tray.Text = "双击打开 订座系统窗体";
            this.Tray.Visible = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(190, 146);
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.AppContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip AppContextMenu;
        private System.Windows.Forms.ToolStripMenuItem Exititem;
        private System.Windows.Forms.NotifyIcon Tray;
    }
}

