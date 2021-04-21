using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        struct Pixel
        {
            public float colorGray;
            public int x;
            public int y;
        }

        private List<Bitmap> bitmaps = new List<Bitmap>(101);
        private Bitmap processingBitmap;
        private bool isBlocked = false;
        private CheckBox lastChecked;

        public Form1()
        {
            InitializeComponent();
            Text = "Image Processing";

            checkBox1.Checked = true;
        }

        private void SplitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Text = "Processing... 0%";
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                for(int i = 0; i < bitmaps.Count; i++)
                {
                    bitmaps[i].Dispose();
                }
                bitmaps.Clear();

                var bitmap = new Bitmap(openFileDialog1.FileName);
                await Task.Run(() => { RunProcessing(bitmap); });
                pictureBox2.Image = bitmaps[trackBar1.Value];
            }
        }

        private void Expansion(Bitmap bitmap)
        {
            this.Invoke(new Action(() =>
            {
                Text = "Processing...";
            }));

            int step;
            if (checkBox1.Checked)
                step = 1;
            else if (checkBox2.Checked)
                step = 2;
            else
                step = 3;

            if (processingBitmap != null)
                processingBitmap.Dispose();

            processingBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = step; y < bitmap.Height - step; y++)
            {
                this.Invoke(new Action(() =>
                {
                    Text = "Processing... " + (100 * y) / bitmap.Height + "%";
                }));

                for (int x = step; x < bitmap.Width - step; x++)
                {
                    Color color = bitmap.GetPixel(x, y);       
                    if (color.R == 0 && color.G == 0 && color.B == 0)
                    {
                        
                        for (int i = y-step; i <= y+step; i++)
                        {
                            for(int j = x-step; j <= x+step; j++)
                            {
                                processingBitmap.SetPixel(j, i, Color.Black);
                            }
                        }
                    }
                    else
                    {
                        processingBitmap.SetPixel(x, y, Color.White);
                    }
                }
            }

            this.Invoke(new Action(() =>
            {
                Text = "Processing complite!";
            }));
            pictureBox2.Image = processingBitmap;
        }

        private void Eroisa(Bitmap bitmap)
        {
            this.Invoke(new Action(() =>
            {
                Text = "Processing...";
            }));

            int step;
            if (checkBox1.Checked)
                step = 1;
            else if (checkBox2.Checked)
                step = 2;
            else
                step = 3;

            if (processingBitmap != null)
                processingBitmap.Dispose();

            processingBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = step; y < bitmap.Height - step; y++)
            {
                this.Invoke(new Action(() =>
                {
                    Text = "Processing... " + (100*y)/bitmap.Height + "%";
                }));
                for (int x = step; x < bitmap.Width - step; x++)
                {
                    processingBitmap.SetPixel(x, y, Color.White);
                    bool setToBlack = true;
                    for (int i = y - step; i <= y + step; i++)
                    {
                        setToBlack = true;
                        for (int j = x - step; j <= x + step; j++)
                        {
                            Color color = bitmap.GetPixel(j, i);
                            if (color.R == 255 && color.G == 255 && color.B == 255)
                            {
                                setToBlack = false;
                                break;
                            }
                        }

                        if (!setToBlack)
                        {
                            break;
                        }
                    }
                    if (setToBlack)
                    {
                        processingBitmap.SetPixel(x, y, Color.Black);
                    }
                }
            }

            this.Invoke(new Action(() =>
            {
                Text = "Processing complite!";
            }));
            pictureBox2.Image = processingBitmap;
        }

        private void RunProcessing(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for(int y = 0; y < bitmap.Height; y++)
            {
                for(int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    float gray = (color.R * 0.3f + color.G * 0.59f + color.B * 0.11f);
                    pixels.Add(new Pixel
                    {
                        colorGray = gray,
                        x = x,
                        y = y
                    });
                }
            }

            pixels.Sort((x, y) => x.colorGray.CompareTo(y.colorGray));
            int pixelsPerIter = pixels.Count / 99;
            Color black = Color.Black;
            Color white = Color.White;

            var curBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for(int i = 0; i < pixels.Count; i++)
            {
                curBitmap.SetPixel(pixels[i].x, pixels[i].y, white);
            }
            bitmaps.Add(new Bitmap(curBitmap));

            int globalCounter = 1;
            for(int i = 1; i < 100; i++)
            {
                this.Invoke(new Action(() =>
                {
                    Text = "Processing... " + i + "%";
                }));

                for (int j = 0; j < pixelsPerIter; j++)
                {
                    curBitmap.SetPixel(pixels[globalCounter].x, pixels[globalCounter].y, black);
                    globalCounter++;
                }
                bitmaps.Add(new Bitmap(curBitmap));
            }

            for (int i = globalCounter; i < pixels.Count; i++)
            {
                curBitmap.SetPixel(pixels[i].x, pixels[i].y, black);
            }
            bitmaps.Add(new Bitmap(curBitmap));
            curBitmap.Dispose();

            this.Invoke(new Action(() =>
            {
                Text = "Processing complite!";
            }));      
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            if(bitmaps.Count > 0)
            {
                pictureBox2.Image = bitmaps[trackBar1.Value];
            }
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
                return;

            if (!isBlocked)
            {
                isBlocked = true;
                await Task.Run(() =>
                {
                    Expansion(new Bitmap(pictureBox2.Image));
                });
                isBlocked = false;
            }
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
                return;

            if (!isBlocked)
            {
                isBlocked = true;
                await Task.Run(() =>
                {
                    Eroisa(new Bitmap(pictureBox2.Image));
                });
                isBlocked = false;
            }
        }

        private async void Button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
                return;

            if (!isBlocked)
            {
                isBlocked = true;
                await Task.Run(() =>
                {
                    Eroisa(new Bitmap((Image)pictureBox2.Image.Clone()));
                });

                await Task.Run(() =>
                {
                    Expansion(new Bitmap((Image)pictureBox2.Image.Clone()));
                });
                isBlocked = false;
            }
        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
                return;

            if (!isBlocked)
            {
                isBlocked = true;
                await Task.Run(() =>
                {
                    Expansion(new Bitmap((Image)pictureBox2.Image.Clone()));
                });

                await Task.Run(() =>
                {
                    Eroisa(new Bitmap((Image)pictureBox2.Image.Clone()));
                });
                isBlocked = false;
            }
        }

        private void CheckBox1_Click(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                checkBox1.Checked = true;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
            }
        }

        private void CheckBox2_Click(object sender, EventArgs e)
        {
            if(checkBox2.Checked == true)
            {
                checkBox2.Checked = true;
                checkBox1.Checked = false;
                checkBox3.Checked = false;
            }
        }

        private void CheckBox3_Click(object sender, EventArgs e)
        {
            if(checkBox3.Checked == true)
            {
                checkBox3.Checked = true;
                checkBox2.Checked = false;
                checkBox1.Checked = false;
            }
        }
    }
}
