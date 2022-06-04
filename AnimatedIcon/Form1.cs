using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PInvoke;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Diagnostics;
using Gma;
using Microsoft.Win32;

namespace AnimatedIcon
{
    public partial class Form1 : Form
    {
        private Desktop desktop = new Desktop();
        private RecycleBin bin = new RecycleBin();

        private Size screen = NativeMethods.GetPhysicalScreenSize();
        private int maxDistance = 0;
        private Point mousePos = new Point();
        private Point binPos = new Point();
        private int mouseToBin = 0;

        private int  openedRatio = 50;
        private int  openedDistance = 400;
        private bool isDragging = false;
        private bool isOpened = false;

        private static string openedChest = @"C:\Users\Ivan\open.ico";
        private static string closedChest = @"C:\Users\Ivan\closed.ico";

        public Form1()
        {
            this.maxDistance = (int)(new Rectangle(0, 0, screen.Width, screen.Height).GetDiagonal());

            this.openedDistance = this.maxDistance * this.openedRatio / 100;

            InitializeComponent();
            this.trackBar1.Value = openedRatio;

            this.desktop.StartDragging += Desktop_StartDrag;

            var events = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            events.MouseUpExt += Events_MouseUp;
            events.MouseMoveExt += Events_MouseMove;
        }

        private void Desktop_StartDrag(object sender, EventArgs e)
        {
            if (this.desktop.IsSelected(this.bin))
                return;

            this.isDragging = true;
            this.HandleIcon();
        }

        private void Events_MouseUp(object sender, MouseEventArgs e)
        {
            if (!this.isDragging)
                return;

            this.isDragging = false;
            this.HandleIcon();
        }

        private void Events_MouseMove(object sender, MouseEventArgs e)
        {
            new Task(() =>
            {
                this.mousePos = e.Location;
                this.binPos = this.desktop.GetPosition(this.bin);
                this.mouseToBin = (int)this.binPos.GetDistance(this.mousePos);
                this.HandleIcon();
            }
            ).Start();
        }

        private void HandleIcon()
        {
            var shouldBeOpened = this.isDragging && (this.mouseToBin < this.openedDistance) && this.desktop.HitTest(Cursor.Position);
            if (this.isOpened != shouldBeOpened)
            {
                this.bin.SetIcon(shouldBeOpened ? openedChest : closedChest);
                this.isOpened = shouldBeOpened;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var str = string.Empty;
            str += $"Screen: {screen.Width}x{screen.Height}\r\n";
            str += $"Mouse: {mousePos.X} {mousePos.Y}\r\n";
            str += $"Bin: {binPos.X} {binPos.Y}\r\n";
            str += $"Max distance: {this.maxDistance}\r\n";
            str += $"Distance: {this.mouseToBin}\r\n";
            str += $"Open distance: {this.openedDistance}\r\n";
            str += $"Sensivity: {this.openedRatio}\r\n";
            this.DebugText.Text = str;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.openedRatio = this.trackBar1.Value;
            this.openedDistance = this.maxDistance * this.openedRatio / 100;
        }
    }
}
