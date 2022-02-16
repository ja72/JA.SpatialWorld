using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static System.Math;

namespace JA.UI
{
    using JA.Mathematics.Spatial;
    using JA.Mathematics.Spatial.Dynamics;

    public partial class RunningForm1 : Form
    {
        readonly FpsCounter clock;
        readonly Scene scene;

        #region Windows API - User32.dll
        [StructLayout(LayoutKind.Sequential)]
        public struct WinMessage
        {
            public IntPtr hWnd;
            public Message msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern bool PeekMessage(out WinMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        #endregion


        public RunningForm1()
        {
            InitializeComponent();

            //Initialize the machine
            this.clock=new FpsCounter();
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            var obj1 = new Object(
                new ObjState(
                    2.8*Vector3.UnitY,
                    Quaternion.Identity,
                    Vector3.Zero,
                    2*PI*Vector3.UnitY),
                Gdi2.Pallette[0],
                Mesh3.TriangularPrism(5, 4));

            var obj2 = new Object(
                new ObjState(                
                    -2*Vector3.UnitY,
                    Quaternion.RotateZ(PI/6) * Quaternion.RotateX(PI/2),
                    Vector3.Zero,
                    (PI/4)*Vector3.UnitY),
                Gdi2.Pallette[1],
                Mesh3.RectangularPrism(5, 3, 1)
                );

            this.scene = new Scene(
                new Camera(pic)
                {
                    ModelSize = 20,    
                    Fov = 45,
                },
                obj1, obj2);

            scene.GetRate = (f) =>
                f.States.Select((state, i) =>
               {
                   var obj = scene.Objects[i];
                   var vee = state.Velocity;
                   var omg = state.Omega;
                   var acc = -0.05f*vee;
                   var alp = -0.12f*omg;

                   return new ObjRate(state, acc, alp);
               }).ToArray();
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            MainLoop();
        }

        void UpdateMachine(float lastFps)
        {
            const double timeFactor = 1;
            var timeStep = lastFps==0 ? 1e-3*timeFactor : timeFactor/lastFps;
            scene.Step(timeStep);
            pic.Refresh();
        }

        #region Main Loop
        public void MainLoop()
        {
            // Hook the application's idle event
            Application.Idle += new EventHandler(OnApplicationIdle);            
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                // Render a frame during idle time (no messages are waiting)
                UpdateMachine(clock.LastFps);
            }
        }

        private bool AppStillIdle
        {
            get
            {
                return !PeekMessage(out WinMessage msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        #endregion

        private void pic_SizeChanged(object sender, EventArgs e)
        {
            pic.Refresh();
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            // Show FPS counter
            var fps = clock.Measure();
            var text = $"{fps:F2} fps";
            var sz = e.Graphics.MeasureString(text, SystemFonts.DialogFont);
            var pt = new PointF(pic.Width-1 - sz.Width - 4, 4);
            e.Graphics.DrawString(text, SystemFonts.DialogFont, Brushes.Black, pt);

            // Draw the machine
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            e.Graphics.TranslateTransform(
                pic.ClientRectangle.Left + pic.ClientRectangle.Width/2,
                pic.ClientRectangle.Top + pic.ClientRectangle.Height/2);

            e.Graphics.ScaleTransform(1, -1);
            scene.Render(e.Graphics);
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            // TODO: Handle it
        }
    }
}
