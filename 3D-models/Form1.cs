using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace _3D_models
{
    public partial class Form1 : Form
    {
        private BufferedGraphics graphic;
        private BufferedGraphicsContext context;
        int camZ = 2, camDepth = 200, centerX, centerY;                                        //Важные переменные
        bool painting_completed = true, is_Loading=false, Res_to_full = false; 
        int frapsPerSec = 0;
        Figure Fig = new Figure(), Fig_Or;
        private char rotationAxis = 'x';
        private double rotationSpeed = 0;
        Point3d Cam = new Point3d(0, 0, -2), SunVetor = new Point3d(0, -1, 0);
        public Thread PaintTHR;
        public Thread RotationTHR;
        public Thread LoadFigTHR;
        public Thread Resizing = null;
        /////////////////////////////////////////
        public Form1()
        {
            InitializeComponent();
            context = BufferedGraphicsManager.Current;
            graphic = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
        }
        public class Figure
        {
            public List<List<int>> surface;
            public List<int> normal;//соответствие нормалей и поверхностей
            public List<Point3d> coords,normals;
            public Brush upbrush;
            public Brush downbrush;
            public Color color;

            public Figure()
            {
                surface = new List<List<int>>();
                normal = new List<int>();
                coords = new List<Point3d>();
                normals = new List<Point3d>();
                color = Color.Bisque;
            }
            public Figure(Figure f)
            {
                int i, j;
                surface = new List<List<int>>();
                for(i =0; i<f.surface.Count(); i++)
                {
                    surface.Add(new List<int>());
                    for (j = 0; j < f.surface[i].Count(); j++)
                    {
                        surface[i].Add(new int());
                        surface[i][j] = f.surface[i][j];
                    }
                }
                normal = new List<int>();
                for (i = 0; i < f.normal.Count(); i++)
                {
                    normal.Add(new int());
                    normal[i] = f.normal[i];
                }
                coords = new List<Point3d>();
                for (i = 0; i < f.coords.Count(); i++)
                {
                    coords.Add(new Point3d(f.coords[i]));
                }
                normals = new List<Point3d>();
                for(i=0; i < f.normals.Count(); i++)
                {
                    normals.Add(new Point3d(f.normals[i]));
                }
                color = Color.Bisque;
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
            public Point3d(Point3d P)
            {
                x = P.x;
                y = P.y;
                z = P.z;
            }

            public static Point3d operator +(Point3d a, Point3d b)
            {
                return new Point3d(a.x + b.x, a.y + b.y, a.z + b.z);
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
            Fig.surface.Add(new List<int>());
            Fig.surface[1].Add(new int());
            Fig.surface[1][0] = 0;
            Fig.surface[1].Add(new int());
            Fig.surface[1][1] = 1;
            Fig.surface[1].Add(new int());
            Fig.surface[1][2] = 2;
            Fig.surface[1].Add(new int());
            Fig.surface[1][3] = 3;
            Fig.normals.Add(new Point3d(0, 0, -1));
            Fig.normals.Add(new Point3d(0, 0, 1));
            Fig.normal.Add(new int());
            Fig.normal[0] = 0;
            Fig.normal.Add(new int());
            Fig.normal[1] = 1;
            //**************************************
            Fig_Or = new Figure(Fig);
            PaintTHR = new Thread(Painting);
            RotationTHR = new Thread(Rotation);
            LoadFigTHR = new Thread(LoadFig);
            Resizing = new Thread(Resize_graphics);
            LoadFigTHR.SetApartmentState(ApartmentState.STA);
            PaintTHR.Start();
            RotationTHR.Start();
            LoadFigTHR.Start();
           // Resizing.Start();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //
        }
        
        private void Form1_Resize(object sender, EventArgs e)
        {
        }

        private void Resize_graphics()
        {
            while (true)
            {
                while (PaintTHR.ThreadState == ThreadState.Running && !Res_to_full) Thread.Sleep(0);
                if (Res_to_full)
                {
                    Form1.ActiveForm.WindowState = FormWindowState.Maximized;
                    /* 
                    Form1.ActiveForm.Height = Screen.PrimaryScreen.Bounds.Height-20;
                    Form1.ActiveForm.Width = Screen.PrimaryScreen.Bounds.Width-5;
                    */
                }
                else Form1.ActiveForm.WindowState = FormWindowState.Normal;
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
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void fromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            is_Loading = true;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            camZ = Convert.ToInt32(numericUpDown1.Value);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                rotationAxis = 'x';
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                radioButton3.Checked = false;
                rotationAxis = 'y';
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                radioButton2.Checked = false;
                radioButton1.Checked = false;
                rotationAxis = 'z';                
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            rotationSpeed = Convert.ToDouble(numericUpDown2.Value) / 200;
        }

        private void timer2_Tick(object sender, EventArgs e)
        { 
            label1.Text = Convert.ToString(frapsPerSec)+" FPS";
            frapsPerSec = 0;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotationSpeed = 0;
        }

<<<<<<< HEAD
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "FULL SCREEN")
            {
                Res_to_full = true;
                button1.Text = "REDUCE SCREEN";
            }
            else
            {
                Res_to_full = false;
                button1.Text = "FULL SCREEN";
            }
            while (PaintTHR.ThreadState == ThreadState.Running) ;
            PaintTHR.Suspend();
            if (Res_to_full)
            {
                Form1.ActiveForm.WindowState = FormWindowState.Maximized;
                /* 
                Form1.ActiveForm.Height = Screen.PrimaryScreen.Bounds.Height-20;
                Form1.ActiveForm.Width = Screen.PrimaryScreen.Bounds.Width-5;
                */
            }
            else Form1.ActiveForm.WindowState = FormWindowState.Normal;
            centerX = Convert.ToInt32(Width / 2);
            centerY = Convert.ToInt32(Height / 2);
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            if (graphic != null)
            {
                graphic.Dispose();
                graphic = null;
            }
            graphic = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            PaintTHR.Resume();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Autor: Shevchenko I.D. (Zeruk) 2017 \nMailTo: id.shev@yandex.ru");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            PaintTHR.Abort();
            RotationTHR.Abort();
            LoadFigTHR.Abort();
            Resizing.Abort();
        }

        private void LoadFig()
=======
        private bool LoadFig(Figure fig)
>>>>>>> parent of c079642... Авторство
        {
            while (true)
            {
                if (is_Loading)
                {
                    while (PaintTHR.ThreadState == ThreadState.Running || RotationTHR.ThreadState == ThreadState.Running) { Thread.Sleep(0); }
                    PaintTHR.Suspend();
                    RotationTHR.Suspend();
                    rotationAxis = 'l';
                    openFileDialog1.Title = "Выберите файл";
                    openFileDialog1.Multiselect = false;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        Fig.coords.Clear();
                        Fig.normals.Clear();
                        for (int iii = 0; iii < Fig.surface.Count; iii++)
                        {
                            Fig.surface[iii].Clear();
                        }
                        Fig.surface.Clear();
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
                            if (first != "#")
                            {
                                switch (first)
                                {
                                    case "v":
                                        {
                                            //x
                                            first = "";
                                            i = 2;
                                            while (i < s.Length && s[i] != ' ')
                                            {
                                                first += s[i];
                                                i++;
                                            }
                                            Fig.coords.Add(new Point3d());
                                            Fig.coords[Fig.coords.Count - 1].x = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);//[ncoord]
                                                                                                                                                            //y
                                            first = ""; i++;
                                            while (i < s.Length && s[i] != ' ')
                                            {
                                                first += s[i];
                                                i++;
                                            }
                                            Fig.coords[Fig.coords.Count - 1].y = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                            //z
                                            first = ""; i++;
                                            while (i < s.Length && s[i] != ' ')
                                            {
                                                first += s[i];
                                                i++;
                                            }
                                            Fig.coords[Fig.coords.Count - 1].z = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
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
                                            Fig.normals.Add(new Point3d());
                                            Fig.normals[Fig.normals.Count - 1].x = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                            //y
                                            first = ""; i++;
                                            while (i < s.Length && s[i] != ' ')
                                            {
                                                first += s[i];
                                                i++;
                                            }
                                            Fig.normals[Fig.normals.Count - 1].y = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                            //z
                                            first = ""; i++;
                                            while (i < s.Length && s[i] != ' ')
                                            {
                                                first += s[i];
                                                i++;
                                            }
                                            Fig.normals[Fig.normals.Count - 1].z = Convert.ToDouble(first, System.Globalization.CultureInfo.InvariantCulture);
                                            //ncoord++;
                                            break;
                                        }
                                    case "f":
                                        {
                                            i = 0; int j;
                                            Fig.surface.Add(new List<int>());
                                            Fig.normal.Add(new int());
                                            while (i < s.Length)
                                            {
                                                if (s[i] == ' ' || i == s.Length - 1)
                                                {
                                                    if (i == s.Length - 1) first += s[i];
                                                    if (first.Contains("//"))
                                                    {
                                                        j = -1;
                                                        while (first[++j] != '/') ;
                                                        Fig.surface[Fig.surface.Count - 1].Add(new int());
                                                        Fig.surface[Fig.surface.Count - 1][Fig.surface[Fig.surface.Count - 1].Count - 1] = -1 + Convert.ToInt32(first.Substring(0, j), System.Globalization.CultureInfo.InvariantCulture);
                                                        Fig.normal[Fig.surface.Count - 1] = Convert.ToInt32(first.Substring(j + 2)) - 1;
                                                        first = "";
                                                        i++;
                                                    }
                                                    // для случая 1/1/1
                                                    else if (first.Contains('/'))
                                                    {
                                                        j = -1;
                                                        while (first[++j] != '/' && j < first.Length) ;
                                                        Fig.surface[Fig.surface.Count - 1].Add(new int());
                                                        Fig.surface[Fig.surface.Count - 1][Fig.surface[Fig.surface.Count - 1].Count - 1] = -1 + Convert.ToInt32(first.Substring(0, j), System.Globalization.CultureInfo.InvariantCulture);
                                                        while (first[++j] != '/') ;
                                                        Fig.normal[Fig.surface.Count - 1] = Convert.ToInt32(first.Substring(j + 1)) - 1;
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
                        Fig_Or = new Figure(Fig);
                    }
                    PaintTHR.Resume();
                    RotationTHR.Resume();
                    is_Loading = false;
                    timer1.Enabled = true;
                    radioButton1.Checked = false;
                    radioButton2.Checked = false;
                    radioButton3.Checked = false;
                }
                Thread.Sleep(0);
            }
        }

        private void Rotation()
        {
            double angle = 0; char lastDir = 'x';
            while (true)
            {
                if (rotationSpeed != 0)
                {
                    if (rotationAxis != lastDir)
                    {
                        Fig_Or = new Figure(Fig);
                        
                        lastDir = rotationAxis;
                        angle = 0;
                    }
                    else
                    {
                        angle += rotationSpeed;
                        switch (rotationAxis)
                        {
                            case 'x':
                                {
                                    for (int i = 0; i < Fig_Or.coords.Count; i++)
                                    {
                                        Fig.coords[i].y = Fig_Or.coords[i].y * Math.Cos(angle) + Fig_Or.coords[i].z * Math.Sin(angle);
                                        Fig.coords[i].z = -Fig_Or.coords[i].y * Math.Sin(angle) + Fig_Or.coords[i].z * Math.Cos(angle);
                                    }
                                    for (int i = 0; i < Fig_Or.normals.Count; i++)
                                    {
                                        Fig.normals[i].y = Fig_Or.normals[i].y * Math.Cos(angle) + Fig_Or.normals[i].z * Math.Sin(angle);
                                        Fig.normals[i].z = -Fig_Or.normals[i].y * Math.Sin(angle) + Fig_Or.normals[i].z * Math.Cos(angle);
                                    }
                                    break;
                                }
                            case 'y':
                                {
                                    for (int i = 0; i < Fig_Or.coords.Count; i++)
                                    {
                                        Fig.coords[i].x = Fig_Or.coords[i].x * Math.Cos(angle) + Fig_Or.coords[i].z * Math.Sin(angle);
                                        Fig.coords[i].z = -Fig_Or.coords[i].x * Math.Sin(angle) + Fig_Or.coords[i].z * Math.Cos(angle);
                                    }
                                    for (int i = 0; i < Fig_Or.normals.Count; i++)
                                    {
                                        Fig.normals[i].x = (Fig_Or.normals[i].x * Math.Cos(angle)) + (Fig_Or.normals[i].z * Math.Sin(angle));
                                        Fig.normals[i].z = (-Fig_Or.normals[i].x * Math.Sin(angle)) + (Fig_Or.normals[i].z * Math.Cos(angle));
                                    }
                                    break;
                                }
                            case 'z':
                                {
                                    for (int i = 0; i < Fig_Or.coords.Count; i++)
                                    {
                                        Fig.coords[i].x = Fig_Or.coords[i].x * Math.Cos(angle) - Fig_Or.coords[i].y * Math.Sin(angle);
                                        Fig.coords[i].y = Fig_Or.coords[i].y * Math.Cos(angle) + Fig_Or.coords[i].x * Math.Sin(angle);
                                    }
                                    for (int i = 0; i < Fig_Or.normals.Count; i++)
                                    {
                                        Fig.normals[i].x = Fig_Or.normals[i].x * Math.Cos(angle) - Fig_Or.normals[i].y * Math.Sin(angle);
                                        Fig.normals[i].y = Fig_Or.normals[i].y * Math.Cos(angle) + Fig_Or.normals[i].x * Math.Sin(angle);
                                    }
                                    break;
                                }
                        }
                    }
                    
                }
                Thread.Sleep(5);
            }
        }

        private void Painting()
        {
            while (true)
            {
                while (LoadFigTHR.ThreadState == ThreadState.Running || Resizing.ThreadState == ThreadState.Running) ;
                    Figure Figu = new Figure(Fig);
                    int i = 0;
                    double cosVal = 0;
                    Brush brushForColor;
                    Point[] pict = new Point[Figu.coords.Count];//, poli = new Point[fig.coords.Count];
                    for (i = 0; i < Figu.coords.Count; i++)
                    {
                        pict[i].X = Convert.ToInt32(Figu.coords[i].x / (Figu.coords[i].z + camZ) * camDepth) + centerX;
                        pict[i].Y = Convert.ToInt32(Figu.coords[i].y / (Figu.coords[i].z + camZ) * camDepth) + centerY;
                    }
                    graphic.Graphics.FillRectangle(Brushes.Azure, 0, 0, Width, Height);
                    List<double> distances = new List<double>();
                    for (i = 0; i < Figu.surface.Count; i++)
                    {
                        distances.Add(new double());
                        for (int j = 0; j < Figu.surface[i].Count; j++)
                        {
                            distances[i] += DistanceTo(Cam, Figu.coords[Figu.surface[i][j]]);
                        }
                    }
                    int max = -1;
                    List<int> been = new List<int>();
                    for (int n = 0; n < Figu.surface.Count; n++)
                    {
                        max = -1;
                        for (i = 0; i < Figu.surface.Count; i++)
                        {
                            if ((max == -1 || distances[max] < distances[i]) && (!been.Contains(i)))
                            {
                                max = i;
                            }
                        }
                        been.Add(new int());
                        been[been.Count - 1] = max;
                        double d = CosViaVectors(Figu.normals[Figu.normal[max]], Cam);
                        if (CosViaVectors(Figu.normals[Figu.normal[max]], Cam) > 0)
                        {
                            Point[] poli = new Point[Figu.surface[max].Count];
                            for (int j = 0; j < Figu.surface[max].Count; j++)
                            {
                                poli[j] = pict[Figu.surface[max][j]];
                            }
                            cosVal = (CosViaVectors(SunVetor, Figu.normals[Figu.normal[max]]) + 1) / 2;
                            brushForColor = new SolidBrush(Color.FromArgb(Convert.ToInt16(Figu.color.R * cosVal), Convert.ToInt16(Figu.color.G * cosVal), Convert.ToInt16(Figu.color.B * cosVal)));
                            graphic.Graphics.FillPolygon(brushForColor, poli);
                            //graphic.Graphics.DrawPolygon(Pens.DarkGray, poli);
                            //graphic.Render(); //for debugging
                            Array.Clear(poli, 0, Figu.surface[max].Count);
                            distances.Clear();
                        }
                    }
                    graphic.Render();
                    frapsPerSec += 1;
                    painting_completed = true;
                Thread.Sleep(0);
            }
        }

        private double DistanceTo(Point3d point3d1, Point3d point3d2)
        {
            return /*Math.Sqrt(*/
                    (point3d1.x - point3d2.x) * (point3d1.x - point3d2.x) + (point3d1.y - point3d2.y) * (point3d1.y - point3d2.y) + (point3d1.z - point3d2.z) * (point3d1.z - point3d2.z);
            //throw new NotImplementedException();
        }

        private double CosViaVectors(Point3d p1, Point3d p2)
        {
            return ((p1.x * p2.x + p1.y * p2.y + p1.z * p2.z)/(Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z))*Math.Sqrt((p2.x * p2.x) + (p2.y * p2.y) + (p2.z * p2.z))));
        }
    }
}
