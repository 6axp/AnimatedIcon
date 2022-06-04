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

        private Point mousePos = new Point();
        private Point binPos = new Point();
        private int mouseToBin = 0;

        private int  openedDistance = 400;
        private bool isDragging = false;
        private bool isOpened = false;

        private static string openedChest = @"C:\Users\Ivan\open.ico";
        private static string closedChest = @"C:\Users\Ivan\closed.ico";

        public Form1()
        {
            InitializeComponent();

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
            var shouldBeOpened = this.isDragging && this.mouseToBin < this.openedDistance;
            if (this.isOpened != shouldBeOpened)
            {
                this.bin.SetIcon(shouldBeOpened ? openedChest : closedChest);
                this.isOpened = shouldBeOpened;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var str = string.Empty;
            str += $"{mousePos.X} {mousePos.Y}\r\n";
            str += $"{binPos.X} {binPos.Y}\r\n";
            str += $"{this.mouseToBin}\r\n";
            this.DebugText.Text = str;
        }
    }
}
