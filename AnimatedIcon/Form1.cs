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
        private bool isDragging = false;

        private static string openedChest = @"C:\Users\Ivan\open.ico";
        private static string closedChest = @"C:\Users\Ivan\closed.ico";

        public Form1()
        {
            InitializeComponent();

            this.desktop.StartDragging += Desktop_StartDrag;

            var events = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            events.MouseUp += Events_MouseUp;
        }

        private void Desktop_StartDrag(object sender, EventArgs e)
        {
            if (this.desktop.IsSelected(this.bin))
                return;

            this.isDragging = true;
            new Task(() => { this.bin.SetIcon(openedChest); }).Start();
        }

        private void Events_MouseUp(object sender, MouseEventArgs e)
        {
            if (!this.isDragging)
                return;

            this.isDragging = false;
            new Task(() => { this.bin.SetIcon(closedChest); }).Start();
        }
    }
}
