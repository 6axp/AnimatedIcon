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
using System.IO;

namespace AnimatedIcon
{
    public partial class Form1 : Form
    {
        private Desktop desktop = new Desktop();
        private RecycleBin bin = new RecycleBin();

        private ISettings settings = new RegistrySettings();

        private Size screen = NativeMethods.GetPhysicalScreenSize();
        private int maxDistance = 0;
        private Point mousePos = new Point();
        private Point binPos = new Point();
        private int mouseToBin = 0;

        // private int  openedRatio = 10;
        private int  openedDistance = 400;
        private bool isDragging = false;
        private int currentStage = 0; // 0 is closed, 6 is opened

        private List<string> emptyIcons;
        private List<string> fullIcons;

        public Form1()
        {
            this.settings.Load();
            var openedRatio = this.settings.Sensivity;

            this.maxDistance = (int)(new Rectangle(0, 0, screen.Width, screen.Height).GetDiagonal());

            this.openedDistance = this.maxDistance * openedRatio / 100;

            InitializeComponent();
            this.trackBar1.Value = openedRatio;

            this.desktop.StartDragging += Desktop_StartDrag;

            var events = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            events.MouseUpExt += Events_MouseUp;
            events.MouseMoveExt += Events_MouseMove;

            UpdatePreviewBar();
            UpdatePreviewList();
            UpdatePreviewPicture();

            CreateFiles();
        }

        private void CreateFiles()
        {
            if (this.settings.Files == null || this.settings.Files.Length == 0)
                return;

            if (string.IsNullOrEmpty(settings.TempDir))
                settings.TempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            if (Directory.Exists(settings.TempDir))
                Directory.Delete(settings.TempDir, true);

            Directory.CreateDirectory(settings.TempDir);

            this.emptyIcons = new List<string>(this.settings.Files.Length);
            this.fullIcons = new List<string>(this.settings.Files.Length);

            foreach (var fileName in this.settings.Files)
            {
                var fi = new FileInfo(fileName);
                var empty = Path.Combine(this.settings.TempDir, "empty_" + fi.Name);
                var full = Path.Combine(this.settings.TempDir, "full_" + fi.Name);

                File.Copy(fileName, empty);
                File.Copy(fileName, full);

                this.emptyIcons.Add(empty);
                this.fullIcons.Add(full);
            }
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

        private int MapRange(int value, int input_start, int input_end, int output_start, int output_end)
        {
            int input_range = input_end - input_start;
            int output_range = output_end - output_start;

            return (value - input_start) * output_range / input_range + output_start;
        }

        private void HandleIcon(bool force = false)
        {
            var currentStage = 0;
            if (this.isDragging && this.desktop.HitTest(Cursor.Position) && this.mouseToBin < this.openedDistance)
            {
                // todo: fix MapRange()
                currentStage = MapRange(this.mouseToBin, this.openedDistance, 100, 0, this.emptyIcons.Count - 1);
            }

            if ((this.currentStage != currentStage || force) && InRange(emptyIcons, currentStage))
            {
                this.bin.SetIcon(emptyIcons[currentStage], fullIcons[currentStage]);

                this.currentStage = currentStage;
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
            str += $"Sensivity: {this.settings.Sensivity}\r\n";
            str += $"Stage: {this.currentStage}\r\n";
            str += $"Temp dir: { this.settings.TempDir }\r\n";
            this.DebugText.Text = str;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.settings.Sensivity = this.trackBar1.Value;
            this.openedDistance = this.maxDistance * this.settings.Sensivity / 100;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.bin.RestoreDefaultIcon();
            this.settings.Save();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "*.ico|*.ico||";
            dlg.Multiselect = true;
            if (this.settings.Files != null && this.settings.Files.Length > 0)
            {
                var fi = new FileInfo(this.settings.Files[0]);
                dlg.InitialDirectory = fi.Directory.FullName;
            }

            var result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.settings.Files = dlg.FileNames;
                UpdatePreviewList();
                UpdatePreviewBar();
                UpdatePreviewPicture();

                CreateFiles();
                HandleIcon(true);
            }
            
        }

        private void UpdatePreviewList()
        {
            this.lstPreviewImages.Items.Clear();
            this.lstPreviewImages.Items.AddRange(this.settings.Files);
        }

        private void UpdatePreviewBar()
        {
            if (this.settings.Files == null || this.settings.Files.Length == 0)
            {
                this.barPreview.Minimum = 0;
                this.barPreview.Maximum = 0;
                this.barPreview.Enabled = false;
                return;
            }

            this.barPreview.Minimum = 1;
            this.barPreview.Maximum = this.settings.Files.Length;
            this.barPreview.Enabled = true;
        }

        private void UpdatePreviewPicture()
        {
            if (this.settings.Files == null || this.settings.Files.Length == 0)
            {
                this.imgPreview.Image = null;
                return;
            }
            var index = this.barPreview.Value - 1;
            if (index < 0 || index >= this.settings.Files.Length)
            {
                Debug.Assert(false);
                return;
            }

            var file = this.settings.Files[index];
            var icon = new Icon(file);

            var w = this.imgPreview.Size.Width;
            var h = this.imgPreview.Size.Height;
            this.imgPreview.Image = new Bitmap(w, h);
            using (var gfx = Graphics.FromImage(this.imgPreview.Image))
            {
                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                
                gfx.DrawIcon(icon, new Rectangle(0, 0, w, h));
            }

            this.lstPreviewImages.SelectedIndex = index;
        }

        private void barPreview_Scroll(object sender, EventArgs e)
        {
            UpdatePreviewPicture();
        }

        bool IsNullOrEmpty<T>(T[] arr)
        {
            return arr == null || arr.Length == 0;
        }

        bool IsNullOrEmpty<T>(List<T> arr)
        {
            return arr == null || arr.Count == 0;
        }

        bool InRange<T>(T[] arr, int index)
        {
            if (IsNullOrEmpty(arr))
                return false;

            return index >= 0 && index < arr.Length;
        }

        bool InRange<T>(List<T> arr, int index)
        {
            if (IsNullOrEmpty(arr))
                return false;

            return index >= 0 && index < arr.Count;
        }
    }
}
