using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FoxScreen
{
    public partial class frmDropArea : Form
    {
        UploadOrganizer uploadOrganizer;
        public frmDropArea(UploadOrganizer uploadOrganizer)
        {
            InitializeComponent();
            targetOpacity = this.Opacity;
            this.uploadOrganizer = uploadOrganizer;
        }

        private void frmDropArea_DragLeave(object sender, EventArgs e)
        {
            DragEnd();
        }

        private void frmDropArea_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
            DragStart();
        }

        private void frmDropArea_DragDrop(object sender, DragEventArgs e)
        {
            DragEnd();

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                foreach (string file in files)
                {
                    if ((!File.Exists(file)) || Directory.Exists(file)) continue;

                    int fPathPos = file.LastIndexOf('/');
                    int fPathPos2 = file.LastIndexOf('\\');
                    if(fPathPos2 > fPathPos) fPathPos = fPathPos2;
                    string filename = file.Substring(fPathPos + 1);

                    int fPosDot = filename.LastIndexOf('.');
                    string extension;
                    if (fPosDot < 0)
                    {
                        extension = "bin";
                    }
                    else if (fPosDot == 0)
                    {
                        extension = filename.Substring(1);
                        filename = "file";
                    }
                    else
                    {
                        extension = filename.Substring(fPosDot + 1);
                        filename = filename.Substring(0, fPosDot);
                    }

                    MemoryStream mstr = new MemoryStream(File.ReadAllBytes(file));
                    uploadOrganizer.AddUpload("FS_" + filename, extension, mstr);
                }
            }

            string text = e.Data.GetData(DataFormats.Text) as string;
            if (text != null)
            {
                uploadOrganizer.AddUpload("FS_paste", "txt", new MemoryStream(System.Text.Encoding.ASCII.GetBytes(text)));
            }
        }

        private void DragStart()
        {
            targetOpacity = 1.0;
            lbDragDrop.Text = "Drop";
        }

        private void DragEnd()
        {
            targetOpacity = 0.5;
            lbDragDrop.Text = "Drag";
        }

        public void ToggleVisibility()
        {
            if (this.targetOpacity <= 0 || !this.Visible)
            {
                this.targetOpacity = 0.5;
                if (!this.Visible)
                    this.Show();
            }
            else
            {
                this.targetOpacity = 0.0;
            }
        }

        private double targetOpacity;
        private void tmOpacity_Tick(object sender, EventArgs e)
        {
            if (this.Opacity > targetOpacity)
            {
                this.Opacity -= 0.01;
            }
            else if (this.Opacity < targetOpacity)
            {
                this.Opacity += 0.01;
            }
            else
            {
                if (this.Opacity <= 0)
                    this.Hide();

                return;
            }
        }

        private void frmDropArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.DrawRectangle(Pens.Red, 0, 0, this.Width - 1, this.Height - 1);
            g.DrawRectangle(Pens.Red, 1, 1, this.Width - 3, this.Height - 3);
        }

        private void frmDropArea_Click(object sender, EventArgs e)
        {
            this.ToggleVisibility();
        }
    }
}
