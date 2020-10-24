namespace AivyDofus.DofusMap.Visualizer
{
    partial class DofusMapVisualizer
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
            this.mapControl1 = new AivyDofus.DofusMap.Visualizer.MapControl();
            this.SuspendLayout();
            // 
            // mapControl1
            // 
            this.mapControl1.ActiveCellColor = System.Drawing.Color.Transparent;
            this.mapControl1.AutoSize = true;
            this.mapControl1.BorderColorOnOver = System.Drawing.Color.Empty;
            this.mapControl1.CommonCellHeight = 43D;
            this.mapControl1.CommonCellWidth = 86D;
            this.mapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl1.DrawMode = ((AivyDofus.DofusMap.Visualizer.DrawMode)((((AivyDofus.DofusMap.Visualizer.DrawMode.Movements | AivyDofus.DofusMap.Visualizer.DrawMode.Fights) 
            | AivyDofus.DofusMap.Visualizer.DrawMode.Triggers) 
            | AivyDofus.DofusMap.Visualizer.DrawMode.Others)));
            this.mapControl1.InactiveCellColor = System.Drawing.Color.DarkGray;
            this.mapControl1.LesserQuality = false;
            this.mapControl1.Location = new System.Drawing.Point(0, 0);
            this.mapControl1.MapHeight = 20;
            this.mapControl1.MapWidth = 14;
            this.mapControl1.Name = "mapControl1";
            this.mapControl1.Size = new System.Drawing.Size(800, 450);
            this.mapControl1.TabIndex = 0;
            this.mapControl1.TraceOnOver = false;
            this.mapControl1.ViewGrid = true;
            // 
            // DofusMapVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.mapControl1);
            this.Name = "DofusMapVisualizer";
            this.Text = "DofusMapVisualizer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MapControl mapControl1;
    }
}