using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace _3D_models
{
    public partial class Form1 : Form
    {
        private BufferedGraphics graphic;
        private BufferedGraphicsContext context;
        int camZ = 2, camDepth = 200, centerX, centerY;                                        //Важные переменные
        bool painting_completed = true, is_Loading=false;
        int timeNow = 0, frapsPerSec = 0;
        Figure Fig = new Figure();
        private char rotationAxis = 'x';
        private double rotationSpeed = 0;

        /////////////////////////////////////////
        public Form1()
        {
            InitializeComponent();
            context = BufferedGraphicsManager.Current;
            graphic = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
        }
       // TODO:  Добавить массив нормалей для поверхностей
        public class Figure
        {
            public List<List<int>> surface;
            public List<int> normal;//соответствие нормалей и поверхностей
            public List<Point3d> coords,normals;
            public Brush upbrush;
            public Brush downbrush;


            public Figure()
            {
                surface = new List<List<int>>();
                normal = new List<int>();
                coords = new List<Point3d>();
                normals = new List<Point3d>();
                upbrush = Brushes.Bisque;
                downbrush = Brushes.Gray;
            }
        }

        public class Point3d
        {
            public double x, y, z;
            public Point3d(double X, double Y, double Z)
            {
                x = X;
                y = Y;
                z = Z;
            }
            public Point3d()
            {
                x = 0;
                y = 0;
                z = 0;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            centerX = Convert.ToInt32(Width / 2);
            centerY = Convert.ToInt32(Height / 2);
            Pen upPen = new Pen(Color.BlanchedAlmond, 1);
            Brush upBrush = Brushes.Chocolate;
            //***********************************
            Fig.coords.Add(new Point3d(0, 0, 0));
            Fig.coords.Add(new Point3d(1, 0, 0));
            Fig.coords.Add(new Point3d(1, 1, 0));
            Fig.coords.Add(new Point3d(0, 1, 0));
            Fig.surface.Add(new List<int>());
            Fig.surface[0].Add(new int());
            Fig.surface[0][0] = 0;
            Fig.surface[0].Add(new int());
            Fig.surface[0][1] = 1;
            Fig.surface[0].Add(new int());
            Fig.surface[0][2] = 2;
            Fig.surface[0].Add(new int());
            Fig.surface[0][3] = 3;
            Painting(Fig);
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (painting_completed && !is_Loading)
            {
                painting_completed = false;
                Painting(Fig);
            }
            else if (is_Loading)
            {
                timer1.Enabled = false;
                LoadFig(Fig);
            }
        }
        
        private void Form1_Resize(object sender, EventArgs e)
        {
            centerX = Convert.ToInt32(Width / 2);
            centerY = Convert.ToInt32(Height / 2);
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            if (graphic != null)
            {
                graphic.Dispose();
                graphic = null;
            }
            graphic = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationAxis = 'x';
            yToolStripMenuItem.Checked = false;
            zToolStripMenuItem.Checked = false;
        }

        private void yToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationAxis = 'y';
            xToolStripMenuItem.Checked = false;
            zToolStripMenuItem.Checked = false;
        }

        private void zToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationAxis = 'z';
            xToolStripMenuItem.Checked = false;
            yToolStripMenuItem.Checked = false;
        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            is_Loading = true;
        }

        private void upToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationSpeed += 0.01;
        }

        private void downToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationSpeed -= 0.01;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            camZ = Convert.ToInt32(numericUpDown1.Value);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label1.Text = Convert.ToString(frapsPerSec);
            frapsPerSec = 0;
        }

        private bool LoadFig(Figure fig)
        {
            openFileDialog1.Title = "Выберите файл";
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fig.coords.Clear();
                fig.normals.Clear();
                for(int iii = 0; iii < fig.surface.Count; iii++)
                {
                    fig.surface[iii].Clear();
                }
                fig.surface.Clear();
                string first, s;
                int i;// ,ncoord = 0, nsurf = 0, nnsurf;
                StreamReader inStream = new StreamReader(openFileDialog1.FileName);
                while (!inStream.EndOfStream)
                {
                    s = inStream.ReadLine();
                    first = "";
                    i = 0;// nnsurf = 0;
                    while (i < s.Length && s[i] != ' ')
                    {
                        first += s[i];
                        i++;
                    }
                    if(first != "#"){
                        switch (first)
                        {
                            case "v":{
                                    //x
                                    first = "";
                                    i = 2;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.coords.Add(new Point3d());
                                    fig.coords[fig.coords.Count-1].x = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);//[ncoord]
                                    //y
                                    first = ""; i++;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.coords[fig.coords.Count - 1].y = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                    //z
                                    first = ""; i++;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.coords[fig.coords.Count - 1].z = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                    //ncoord++;
                                    break;
                                }
                            //TODO:запись нормали (DONE)
                            case "vn":
                                {
                                    //x
                                    first = "";
                                    i = 3;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.normals.Add(new Point3d());
                                    fig.normals[fig.normals.Count - 1].x = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                    //y
                                    first = ""; i++;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.normals[fig.normals.Count - 1].y = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                    //z
                                    first = ""; i++;
                                    while (i < s.Length && s[i] != ' ')
                                    {
                                        first += s[i];
                                        i++;
                                    }
                                    fig.normals[fig.normals.Count - 1].z = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                    //ncoord++;
                                    break;
                                }
                            case "f":{
                                    i = 0; int j;
                                    fig.surface.Add(new List<int>());
                                    while (i < s.Length)
                                    {
                                        if (s[i] == ' ' || i==s.Length-1)
                                        {
                                            if (first.Contains("//"))
                                            {
                                                j = -1; 
                                                while (first[++j] != '/');
                                                fig.surface[fig.surface.Count-1].Add(new int());
                                                fig.surface[fig.surface.Count - 1][fig.surface[fig.surface.Count - 1].Count - 1] = -1 + Convert.ToInt32(first.Substring(0, j), System.Globalization.CultureInfo.InvariantCulture);
                                                //нормаль[nsurf].Add(new int() = Convert.ToInt32(first.Substring(j+2)));

                                                first = "";
                                                i++;
                                            }
                                            // для случая 1/1/1
                                            else if (first.Contains('/'))
                                            {
                                                j = -1;
                                                while (first[++j] != '/' && j < first.Length) ;
                                                fig.surface[fig.surface.Count - 1].Add(new int());
                                                fig.surface[fig.surface.Count - 1][fig.surface[fig.surface.Count - 1].Count - 1] =-1+ Convert.ToInt32(first.Substring(0, j), System.Globalization.CultureInfo.InvariantCulture);
                                                j = -1;
                                                while (first[++j] != '/') ;
                                                //нормаль[nsurf].Add(new int() = Convert.ToInt32(first.Substring(j+2)));

                                                first = "";
                                                i++;
                                            }
                                            else
                                            {
                                                first = "";
                                                i++;
                                            }
                                        }
                                        else
                                        {
                                            first += s[i];
                                            i++;
                                        }
                                    }
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                }
                inStream.Close();
            }
            is_Loading = false;
            timer1.Enabled = true; 
            return true;
        }

        private void Rotation(char vector, double angle, Figure fig)
        {
            if (angle != 0)
            {
                switch (vector)
                {
                    case 'x':
                        {
                            for (int i = 0; i < fig.coords.Count; i++)
                            {
                                fig.coords[i].y = fig.coords[i].y * Math.Cos(angle) + fig.coords[i].z * Math.Sin(angle);
                                fig.coords[i].z = -fig.coords[i].y * Math.Sin(angle) + fig.coords[i].z * Math.Cos(angle);
                            }
                            for (int i = 0; i < fig.normals.Count; i++)
                            {
                                fig.normals[i].y = fig.normals[i].y * Math.Cos(angle) + fig.normals[i].z * Math.Sin(angle);
                                fig.normals[i].z = -fig.normals[i].y * Math.Sin(angle) + fig.normals[i].z * Math.Cos(angle);
                            }
                            break;
                        }
                    case 'y':
                        {
                            for (int i = 0; i < fig.coords.Count; i++)
                            {
                                fig.coords[i].x = fig.coords[i].x * Math.Cos(angle) + fig.coords[i].z * Math.Sin(angle);
                                fig.coords[i].z = -fig.coords[i].x * Math.Sin(angle) + fig.coords[i].z * Math.Cos(angle);
                            }
                            for (int i = 0; i < fig.normals.Count; i++)
                            {
                                fig.normals[i].y = fig.normals[i].x * Math.Cos(angle) + fig.normals[i].z * Math.Sin(angle);
                                fig.normals[i].z = -fig.normals[i].x * Math.Sin(angle) + fig.normals[i].z * Math.Cos(angle);
                            }
                            break;
                        }
                    case 'z':
                        {
                            for (int i = 0; i < fig.coords.Count; i++)
                            {
                                fig.coords[i].x = fig.coords[i].x * Math.Cos(angle) - fig.coords[i].y * Math.Sin(angle);
                                fig.coords[i].y = fig.coords[i].y * Math.Cos(angle) + fig.coords[i].x * Math.Sin(angle);
                            }
                            for (int i = 0; i < fig.normals.Count; i++)
                            {
                                fig.normals[i].x = fig.normals[i].x * Math.Cos(angle) + fig.normals[i].y * Math.Sin(angle);
                                fig.normals[i].y = fig.normals[i].y * Math.Cos(angle) + fig.normals[i].x * Math.Sin(angle);
                            }
                            break;
                        }
                }
            }
        }

        private bool Painting(Figure fig)
        {
            //for (int i = 0; fig.count; i++) {
            Rotation(rotationAxis, rotationSpeed,Fig);
            int i = 0;
            Point[] pict = new Point[fig.coords.Count];//, poli = new Point[fig.coords.Count];
            for (i = 0; i < fig.coords.Count; i++)
            {
                pict[i].X = Convert.ToInt32(fig.coords[i].x / (fig.coords[i].z + camZ) * camDepth)+centerX;
                pict[i].Y = Convert.ToInt32(fig.coords[i].y / (fig.coords[i].z + camZ) * camDepth)+centerY;
            }
            graphic.Graphics.FillRectangle(Brushes.White, 0, 0, Width, Height);
            //сделать нахождение дистанций после определения видимости
            double[] distances = new double[fig.surface.Count];
            for(i = 0; i< fig.surface.Count; i++)
            {
                distances[i] = 0;
                for (int j = 0; j < fig.surface[i].Count; j++) distances[i] += DistanceTo(new Point3d(0,0,-camZ),fig.coords[fig.surface[i][j]]);
            }


            int max=-1,lastnMax= -1;
            List<int> been = new List<int>();
            for (int n=0; n < fig.surface.Count; n++)
            {
                /*
                max = -1;
                for (i = 0; i < fig.surface.Count; i++)
                {
                    if((max == -1 || distances[i]>distances[max]) && (lastnMax == -1 || distances[i]<=distances[lastnMax]) && i != lastnMax )
                    {
                        max = i;
                    }
                }
                lastnMax = max;*/
                max = -1;
                for (i = 0; i < fig.surface.Count; i++)
                {
                    if ((max == -1 || distances[max] < distances[i]) && (!been.Contains(i)))
                    {
                        max = i;
                    }
                }
                been.Add(new int());
                been[been.Count - 1] = max;
                Point[] poli = new Point[fig.surface[max].Count];
                for(int j = 0; j < fig.surface[max].Count; j++)
                {
                    poli[j] = pict[fig.surface[max][j]];
                }
                graphic.Graphics.FillPolygon(fig.upbrush, poli);
                graphic.Graphics.DrawPolygon(Pens.Brown, poli);
                Array.Clear(poli, 0, fig.surface[max].Count);
            }
            
            graphic.Render();
            frapsPerSec += 1;
            painting_completed = true;
            return true;
        }

        private double DistanceTo(Point3d point3d1, Point3d point3d2)
        {
            return Math.Sqrt((point3d1.x - point3d2.x) * (point3d1.x - point3d2.x) + (point3d1.y - point3d2.y) * (point3d1.y - point3d2.y) + (point3d1.z - point3d2.z) * (point3d1.z - point3d2.z));
            //throw new NotImplementedException();
        }

        private double CosViaVectors(Point3d p1, Point3d p2)
        {
            return ((p1.x * p2.x + p1.y * p2.y + p1.z + p2.z) /(Math.Sqrt(p1.x * p1.x + p1.y * p1.y + p1.z + p1.z)*Math.Sqrt(p2.x * p2.x + p2.y * p2.y + p2.z + p2.z)));
        }
    }
}
