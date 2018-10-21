using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace PlanetaryGear
{
    public partial class Form1 : Form
    {
        private Device d3D;

        private Mesh wheel1, wheel2, wheel3;
        private Mesh bearing1, bearing2;
        private Mesh crank1, crank2, crank3;
        private Mesh cube1;
        private Mesh torus1, torus2, torus3;

        private Material wheelMaterial;
        private Material crankMaterial;
        private Material cubeMaterial;

        private const int Coeff = 70;
        private double deltaT = 0.1;
        private double t;

        public static int R1 = 50, R2 = 20, R3 = 30;
        public static double Omega = 0.05;
        public static Point Center;
        public static Point Pos;

        private bool flag;
        private bool move;
        private bool stop = true;
        private bool carcass = true;
        
        public Form1()
        {
            InitializeComponent();

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lr1.Text = Convert.ToString(R1) + " см";
            lr2.Text = Convert.ToString(R2) + " см";
            lr3.Text = Convert.ToString(R2) + " см";
            LOmega.Text = Convert.ToString(Omega) + " рад/с";


            try
            {
                // Устанавливаем режим отображения трехмерной графики
                PresentParameters d3Dpp = new PresentParameters
                {
                    BackBufferCount = 1,
                    SwapEffect = SwapEffect.Discard,
                    Windowed = true, // Выводим графику в окно
                    MultiSample = MultiSampleType.None, // Выключаем антиалиасинг
                    EnableAutoDepthStencil = true, // Разрешаем создание z-буфера
                    AutoDepthStencilFormat = DepthFormat.D16 // Z-буфер в 16 бит
                };
                d3D = new Device(0, // D3D_ADAPTER_DEFAULT - видеоадаптер по 
                                    // умолчанию
                      DeviceType.Hardware, // Тип устройства - аппаратный ускоритель
                      this, // Окно для вывода графики
                      CreateFlags.SoftwareVertexProcessing, // Геометрию обрабатывает CPU
                      d3Dpp);

            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Ошибка инsициализации");
                Close(); // Закрываем окно
            }

            wheel1 = Mesh.Cylinder(d3D, (float)R1 / Coeff, (float)R1 / Coeff, 0.1f, 50, 10);
            wheel2 = Mesh.Cylinder(d3D, (float)R2 / Coeff, (float)R2 / Coeff, 0.15f, 20, 10);
            wheel3 = Mesh.Cylinder(d3D, (float)R3 / Coeff, (float)R3 / Coeff, 0.1f, 30, 10);

            torus1 = Mesh.Torus(d3D, 0.006f, (R1 + 0.006f) / Coeff, 36, 80);
            torus2 = Mesh.Torus(d3D, 0.006f, (R2 + 0.006f) / Coeff, 36, 80);
            torus3 = Mesh.Torus(d3D, 0.006f, (R3 + 0.006f) / Coeff, 36, 80);

            bearing1 = Mesh.Sphere(d3D, 0.05f, 10, 10);
            bearing2 = Mesh.Sphere(d3D, 0.05f, 10, 10);

            crank1 = Mesh.Cylinder(d3D, 0.006f, 0.006f, (float)(2 * R1) / Coeff, 10, 10);
            crank2 = Mesh.Cylinder(d3D, 0.006f, 0.006f, (float)(R1 + R3) / Coeff, 10, 10);
            crank3 = Mesh.Cylinder(d3D, 0.006f, 0.006f, (float)(R1 + R3) / Coeff, 10, 10);

            cube1 = Mesh.Box(d3D, 0.1f, 0.1f, 0.1f);

            wheelMaterial = new Material
            {
                Diffuse = Color.Blue,
                Specular = Color.White
            };

            crankMaterial = new Material
            {
                Diffuse = Color.Silver,
                Specular = Color.White
            };

            cubeMaterial = new Material
            {
                Diffuse = Color.Red,
                Specular = Color.White
            };



        }

        private void PictureMove()
        {
            d3D.RenderState.Lighting = true;

            double f2 = Omega;
            double f1 = (f2 * R3) / R1;
            double v1 = f2 * R3;

            d3D.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Wheat, 1.0f, 0);
            d3D.BeginScene();
            if (flag)
            {
                SetupProjection();
                d3D.RenderState.FillMode = carcass ? FillMode.WireFrame : FillMode.Solid;
                if (move)
                {
                    d3D.Material = wheelMaterial;
                    
                    d3D.Transform.World = Matrix.RotationZ((float)(-f1 * t)) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                     Matrix.Translation(0, (float)2 * R1 / Coeff, 6f);
                    wheel1.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationZ((float)(-f1 * t)) * Matrix.Translation(0, (float)2 * R1 / Coeff, -0.1f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                    Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                    wheel2.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationZ((float)(f2 * t)) * Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, 0f) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                    wheel3.DrawSubset(0);

                    crank1 = Mesh.Cylinder(d3D, 0.006f, 0.006f, (float)(2 * R1 + v1 * t) / Coeff, 10, 10);
                    d3D.Material = crankMaterial;
                    d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R2) / Coeff, (float)(R1 - (v1 * t) / 2) / Coeff, -0.1f) *
                        Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0f, 0, 6f);
                    crank1.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationZ((float)(-f1 * t)) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                      Matrix.Translation(0, (float)2 * R1 / Coeff, 6f);
                    torus1.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationZ((float)(-f1 * t)) * Matrix.Translation(0, (float)2 * R1 / Coeff, -0.1f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                   Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                    torus2.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationZ((float)(f2 * t)) * Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, 0f) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                    torus3.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R1) / Coeff, (float)(2 * R1 - (R1 + R3) / 2) / Coeff, 0) *
                    Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                    crank2.DrawSubset(0);

                    d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R1 + 2 * R3) / Coeff, (float)(2 * R1 - (R1 + R3) / 2) / Coeff, 0) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                    crank3.DrawSubset(0);

                    d3D.Transform.World = Matrix.Translation(0, (float)2 * R1 / Coeff, -0.15f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                   Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                    bearing1.DrawSubset(0);

                    d3D.Transform.World = Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, -0.05f) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                    bearing2.DrawSubset(0);

                    d3D.Material = cubeMaterial;
                    d3D.Transform.World = Matrix.Translation((float)(R2) / Coeff, (float)(R1 - R1 - v1 * t) / Coeff, -0.1f) *
                        Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0f, 0, 6f);
                    cube1.DrawSubset(0);

                    if (stop)
                        t = t + deltaT;

                }

            }
            d3D.EndScene();
            //Показываем содержимое дублирующего буфера
            d3D.Present();
        }
        private void PictureStart()
        {
            d3D.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Wheat, 1.0f, 0);
            d3D.BeginScene();
            if (flag)
            {
                SetupProjection();
                d3D.RenderState.FillMode = carcass ? FillMode.WireFrame : FillMode.Solid;

                d3D.Material = wheelMaterial;

                d3D.Transform.World = Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                    Matrix.Translation(0, 0, 0) * Matrix.Translation(0, (float)2 * R1 / Coeff, 6f);
                wheel1.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation(0, (float)2 * R1 / Coeff, -0.1f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                    Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                wheel2.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, 0f) *
                    Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                wheel3.DrawSubset(0);

                d3D.Material = crankMaterial;
                d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R2) / Coeff, (float)R1 / Coeff, -0.1f) *
                    Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0f, 0, 6f);
                crank1.DrawSubset(0);

                d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R1) / Coeff, (float)(2 * R1 - (R1 + R3) / 2) / Coeff, 0) *
                    Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                crank2.DrawSubset(0);

                d3D.Transform.World = Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation((float)(R1 + 2 * R3) / Coeff, (float)(2 * R1 - (R1 + R3) / 2) / Coeff, 0) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                crank3.DrawSubset(0);

                d3D.Transform.World = Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                   Matrix.Translation(0, 0, 0) * Matrix.Translation(0, (float)2 * R1 / Coeff, 6f);
                torus1.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation(0, (float)2 * R1 / Coeff, -0.1f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                    Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                torus2.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, 0f) *
                     Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                torus3.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation(0, (float)2 * R1 / Coeff, -0.15f) * Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) *
                    Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0, 0, 6f);
                bearing1.DrawSubset(0);

                d3D.Transform.World = Matrix.Translation((float)(R1 + R3) / Coeff, (float)(2 * R1 - R1 - R3) / Coeff, -0.05f) *
                   Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0f, 0, 6f);
                bearing2.DrawSubset(0);

                d3D.Material = cubeMaterial;
                d3D.Transform.World = Matrix.Translation((float)(R2) / Coeff, (float)(R1 - R1) / Coeff, -0.1f) *
                    Matrix.RotationY((float)(vScrollBar1.Value * Math.PI / 180)) * Matrix.Translation(0, 0, -0.1f) * Matrix.Translation(0f, 0, 6f);
                cube1.DrawSubset(0);
            }
            d3D.EndScene();
            //Показываем содержимое дублирующего буфера
            d3D.Present();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (move)
                PictureMove();
            else
                PictureStart();
        }

        private void drawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            t = 0;
            clearToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.Enabled = true;
            //нельзя новый рисунок, пока его не стерли
            drawToolStripMenuItem.Enabled = false;
            flag = true;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //новый рисунок нельзя пока движение не будет остановлено
            drawToolStripMenuItem.Enabled = false;
            //задавать новые геометрические и кинематические параметры нельзя
            // пока движение не остановлено
            geometricToolStripMenuItem.Enabled = false;
            kinematicToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            //можно останавливать
            остановитьToolStripMenuItem.Enabled = true;
            //стерать нельзя пока движение не будет остановлено
            clearToolStripMenuItem.Enabled = false;

            flag = true;
            move = true;
            PictureMove();
            stop = true;

            double f2 = Omega * (R1 + R2) * t / R2;
            Point a = new Point
            {
                X = Convert.ToInt32(Center.X + (R1 - R2) * Math.Cos(Omega * t) - R2 * Math.Cos(f2)),
                Y = Convert.ToInt32(Center.Y - (R1 - R2) * Math.Sin(Omega * t) + R2 * Math.Sin(f2))
            };
            Point b = new Point
            {
                X = Convert.ToInt32(Center.X + (R1 - R2) * Math.Cos(Omega * t) + R2 * Math.Cos(f2)),
                Y = Convert.ToInt32(Center.Y - (R1 - R2) * Math.Sin(Omega * t) - R2 * Math.Sin(f2))
            };
            CustomVertex.PositionColored one = new CustomVertex.PositionColored
            {
                Position = new Vector3((float)a.X / Coeff, (float)a.Y / Coeff, 0f),
                Color = Color.Red.ToArgb()
            };

            one.Position = new Vector3((float)b.X / Coeff, (float)b.Y / Coeff, 0f);
            one.Color = Color.Green.ToArgb();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // стереть можно
            clearToolStripMenuItem.Enabled = true;
            //новые параметры задавать можно
            geometricToolStripMenuItem.Enabled = true;
            kinematicToolStripMenuItem.Enabled = true;
            //нечего останавливать
            остановитьToolStripMenuItem.Enabled = false;
            //можно запускать
            startToolStripMenuItem.Enabled = true;

            stop = false;
        }

       private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag = false;
            //можно рисовать
            drawToolStripMenuItem.Enabled = true;
            //нельзя стирать
            clearToolStripMenuItem.Enabled = false;
            //нечего запускать
            startToolStripMenuItem.Enabled = false;
        }

        private void geometricToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGeometricCharacteristics myF2 = new FormGeometricCharacteristics();
            Pos = new Point(Location.X, Location.Y + Height - panel1.Height);
            myF2.StartPosition = FormStartPosition.CenterParent;
            myF2.Location = new Point(Pos.X, Pos.Y);

            if (myF2.ShowDialog() == DialogResult.OK)
            {

                lr1.Text = Convert.ToString(R1) + " см";
                lr2.Text = Convert.ToString(R2) + " см";
                lr3.Text = Convert.ToString(R2) + " см";
            }
            clearToolStripMenuItem_Click(sender, e);
            drawToolStripMenuItem_Click(sender, e);
            Creation();
        }

        private void kinematicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAngularVelocity myF3 = new FormAngularVelocity();
            Pos = new Point(Location.X, Location.Y + Height - panel1.Height);
            myF3.StartPosition = FormStartPosition.CenterParent;
            myF3.Location = new Point(Pos.X, Pos.Y);
            if (myF3.ShowDialog() == DialogResult.OK)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Omega == 0)
                    MessageBox.Show("Вы задали угловую скорость равной 0");
                else
                    deltaT = Math.Abs(Omega);
            }
            // ReSharper disable once SpecifyACultureInStringConversionExplicitly
            LOmega.Text = Convert.ToString(Omega) + " рад/с";
            clearToolStripMenuItem_Click(sender, e);
            drawToolStripMenuItem_Click(sender, e);
        }

        private void SetupProjection()
        {
            // Устанавливаем параметры источника освещения
            // Устанавливаем параметры источника освещения
            d3D.Lights[0].Enabled = true;   // Включаем нулевой источник освещения
            d3D.Lights[0].Diffuse = Color.White;         // Цвет источника освещения
            d3D.Lights[0].Position = new Vector3(0, 0, 0); // Задаем координаты
            // ReSharper disable once PossibleLossOfFraction
            d3D.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, Width / Height, 1.0f, 50.0f);
        }

        public void OnIdle(object sender, EventArgs e)
        {
            Invalidate(); // Помечаем главное окно (this) как требующее перерисовки
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (move)
                PictureMove();
            else
                PictureStart();
        }

        private void Creation()
        {
            wheel1.Dispose();
            wheel2.Dispose();
            wheel3.Dispose();
            crank1.Dispose();
            crank2.Dispose();

            wheel1 = Mesh.Cylinder(d3D, (float)R1 / Coeff, (float)R1 / Coeff, 0.1f, 50, 10);
            wheel2 = Mesh.Cylinder(d3D, (float)R2 / Coeff, (float)R2 / Coeff, 0.15f, 20, 10);
            wheel3 = Mesh.Cylinder(d3D, (float)R3 / Coeff, (float)R3 / Coeff, 0.1f, 30, 10);

            crank1 = Mesh.Cylinder(d3D, 0.005f, 0.005f, (float)(2 * R1) / Coeff, 10, 10);
            crank2 = Mesh.Cylinder(d3D, 0.006f, 0.006f, (float)(R1 + R3) / Coeff, 10, 10);

            torus1 = Mesh.Torus(d3D, 0.006f, (R1 + 0.006f) / Coeff, 36, 80);
            torus2 = Mesh.Torus(d3D, 0.006f, (R2 + 0.006f) / Coeff, 36, 80);
            torus3 = Mesh.Torus(d3D, 0.006f, (R3 + 0.006f) / Coeff, 36, 80);
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            carcass = true;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            carcass = false;
        }
    }
}
