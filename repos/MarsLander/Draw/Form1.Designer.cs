using Algorithms.Space;
using GameSolution;
using GameSolution.Entities;
using GameSolution.Game;
using System.Drawing;
using System.Windows.Forms;

namespace Draw
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

        public Graphics Graphics { get; set; }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics = e.Graphics;
            var state = GameHelper.CreateSimpleGame();
            DrawGame(state);
            //GameHelper.PlayGame(state, e.Graphics);
        }

        private void DrawGame(GameState state)
        {
            Graphics.Clear(Color.White);
            Point2d currentPoint = null;
            foreach (Point2d point in state.Board.Points)
            {
                if(currentPoint == null)
                {
                    currentPoint = point;
                }
                else
                {
                    Graphics.DrawLine(new Pen(Brushes.Red), currentPoint.GetTruncatedX(), currentPoint.GetTruncatedY(), point.GetTruncatedX(), point.GetTruncatedY());
                }
                currentPoint = point;
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(7000, 3000);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);

        }

        

        #endregion
    }
}
