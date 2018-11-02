using System.Drawing;
using System.Windows.Forms;
namespace GraphicsApp
{
    partial class GameWindow
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
            this.ActionPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ActionPanel
            // 
            this.ActionPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.ActionPanel.Cursor = System.Windows.Forms.Cursors.Cross;
            this.ActionPanel.Location = new System.Drawing.Point(0, 0);
            this.ActionPanel.Name = "ActionPanel";
            this.ActionPanel.Size = new System.Drawing.Size(1280, 800);
            this.ActionPanel.TabIndex = 5;
            // 
            // GameWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Linen;
            this.ClientSize = new System.Drawing.Size(1272, 766);
            this.ControlBox = false;
            this.Controls.Add(this.ActionPanel);
            this.Name = "GameWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Blaster";
            this.Shown += new System.EventHandler(this.GameWindow_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.GameWindow_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameWindow_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameWindow_KeyDown);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Panel ActionPanel;
    }
}

