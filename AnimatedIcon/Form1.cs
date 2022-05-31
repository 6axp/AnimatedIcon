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
    [StructLayout(LayoutKind.Sequential)]
    struct LVITEM
    {
        public uint mask;
        public int iItem;
        public int iSubItem;
        public uint state;
        public uint stateMask;
        public IntPtr pszText;
        public int cchTextMax;
        public int iImage;
        public uint lParam;
        public int iIndent;
    };

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(Vanara.PInvoke.Shell32.DShellFolderViewEvents))]
    public partial class Form1 : Form, Vanara.PInvoke.Shell32.DShellFolderViewEvents
    {
        IntPtr listView;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static NativeMethods.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static Form1 form;

        //private string canIconRegKey = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\CurrentVersion\\Explorer\\CLSID\\{645FF040-5081-101B-9F08-00AA002F954E}\\DefaultIcon";

        private string openedChest = "C:\\Users\\Ivan\\open.ico,0";
        private string closedChest = "C:\\Users\\Ivan\\closed.ico,0";

        private RegistryKey canIconRegKey;

        private Vanara.PInvoke.Shell32.IShellView3 desktopView;

        private FolderViewEvents DesktopEvents;

        bool isDragging = false;

        public Form1()
        {
            InitializeComponent();

            canIconRegKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\CLSID\{645FF040-5081-101B-9F08-00AA002F954E}\DefaultIcon", true);

            /*
            while (this.listView == IntPtr.Zero)
            {
                var _SHELLDLL_DefView = FindShellDefView();

                this.listView = User32.FindWindowEx(_SHELLDLL_DefView, IntPtr.Zero, "SysListView32", "FolderView");
            }
            RefreshDebug();

            form = this;
            _hookID = SetHook(_proc);
            */

            

            this.desktopView = Shell.GetDesktopView();


            this.DesktopEvents = new FolderViewEvents(this.desktopView);

            this.DesktopEvents.StartDrag += Desktop_StartDrag;

           

            var events = Gma.System.MouseKeyHook.Hook.GlobalEvents();
            events.MouseUp += Events_MouseUp;

            // timer1.Interval = 100;
            // timer1.Enabled = true;

            //Shell.RefreshTrashCan(desktopView);

        }

        private void Desktop_StartDrag(object sender, EventArgs e)
        {
            var bin = Vanara.Windows.Shell.RecycleBin.ShellFolderInstance.PIDL;

            if (Shell.IsSelected(this.desktopView, bin))
                return;

            this.isDragging = true;

            canIconRegKey.SetValue("full", openedChest, RegistryValueKind.ExpandString);
            canIconRegKey.SetValue("empty", openedChest, RegistryValueKind.ExpandString);
            canIconRegKey.SetValue("", openedChest, RegistryValueKind.ExpandString);
            canIconRegKey.Flush();

            Shell.RefreshTrashCan();

            Action action = () => this.DebugText.Text = "is dragging";
            this.DebugText.Invoke(action);
        }

        private void Events_MouseUp(object sender, MouseEventArgs e)
        {
            if (!this.isDragging)
                return;

            this.isDragging = false;
            this.DebugText.Text = "";

            canIconRegKey.SetValue("full", closedChest, RegistryValueKind.ExpandString);
            canIconRegKey.SetValue("empty", closedChest, RegistryValueKind.ExpandString);
            canIconRegKey.SetValue("", closedChest, RegistryValueKind.ExpandString);
            canIconRegKey.Flush();

            Shell.RefreshTrashCan();
        }

        public void SetDebug(object obj)
        {
            this.DebugText.Text = obj.ToString();
        }

        private static IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Form1.form.SetDebug((Keys)vkCode);
                //Console.WriteLine((Keys)vkCode);
            }
            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //this.DebugText.Text = "Down";
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            this.DebugText.Text = "";
        }

        private IntPtr FindShellDefView()
        {
            IntPtr destop = User32.GetDesktopWindow();
            IntPtr hWorkerW = IntPtr.Zero;
            IntPtr hShellViewWin = IntPtr.Zero;
            do
            {
                hWorkerW = User32.FindWindowEx(destop, hWorkerW, "WorkerW", null);
                hShellViewWin = User32.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
            }
            while (hShellViewWin == IntPtr.Zero && hWorkerW != IntPtr.Zero);

            return hShellViewWin;
        }

        private bool HitTestDesktopItem(Point point)
        {
            var cx = User32.GetSystemMetrics(User32.SystemMetric.SM_CXICON);
            var cy = User32.GetSystemMetrics(User32.SystemMetric.SM_CYICON);

            var view = Shell.GetDesktopView();
            var folderView = (Vanara.PInvoke.Shell32.IFolderView)view;

            Guid IID_IShellFolder = new Guid("000214e6-0000-0000-c000-000000000046");
            var folder = (Vanara.PInvoke.Shell32.IShellFolder)folderView.GetFolder(IID_IShellFolder);

            for (var i = 0; i < folderView.ItemCount(Vanara.PInvoke.Shell32.SVGIO.SVGIO_ALLVIEW); i++)
            {
                var id = folderView.Item(i);
                var pos = folderView.GetItemPosition(id);

                var rect = new Rectangle(pos.X, pos.Y, cx, cy);

                if (rect.Contains(point))
                    return true;
            }

            return false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.DebugText.Clear();

            /*
            var str = string.Empty;

            var dc = Vanara.PInvoke.Gdi32.CreateCompatibleDC();
            var inchX = Vanara.PInvoke.Gdi32.GetDeviceCaps(dc, Vanara.PInvoke.Gdi32.DeviceCap.LOGPIXELSX);
            var inchY = Vanara.PInvoke.Gdi32.GetDeviceCaps(dc, Vanara.PInvoke.Gdi32.DeviceCap.LOGPIXELSX);
            dc.Close();

            var iconCx = User32.GetSystemMetrics(User32.SystemMetric.SM_CXICON);
            var iconCy = User32.GetSystemMetrics(User32.SystemMetric.SM_CYICON);


            var xRatio = (float)inchX / 64f;
            var yRatio = (float)inchY / 64f;

            var pos = Cursor.Position;

            pos.X = (int)(pos.X * xRatio);
            pos.Y = (int)(pos.Y * xRatio);


            var view = Shell.GetDesktopView();
            var folderView = (Vanara.PInvoke.Shell32.IFolderView2)view;

            var focused = folderView.Item(folderView.GetFocusedItem());


            Vanara.PInvoke.Shell32.IShellFolder desktopFolder;
            Vanara.PInvoke.Shell32.SHGetDesktopFolder(out desktopFolder);
            */



            /*
            public:
                 HRESULT Connect(IUnknown *punk)
                 {
                  HRESULT hr = S_OK;
                  CComPtr<IConnectionPointContainer> spcpc;
                  if (SUCCEEDED(hr)) {
                   hr = punk->QueryInterface(IID_PPV_ARGS(&spcpc));
                  }
                  if (SUCCEEDED(hr)) {
                  hr = spcpc->FindConnectionPoint(__uuidof(DispInterface), &m_spcp);
                  }
                  if (SUCCEEDED(hr)) {
                  hr = m_spcp->Advise(this, &m_dwCookie);
                  }
                  return hr;
                 } 

             */

            // Vanara.PInvoke.Shell32.DShellFolderViewEvents;

            /*

            RECT rc = new RECT();

            var fPos = folderView.GetItemPosition(focused);

            var fRect = new Rectangle(fPos, new Size(iconCx, iconCy));

            str += $"Mouse: {pos}\r\n";
            str += $"Focused Pos : {fPos.X} {fPos.Y}\r\n";
            str += $"Focused Rect: {rc.left} {rc.top}\r\n";

            var pointWindow = User32.WindowFromPoint(Cursor.Position);


            str += $"Window under mouse: {pointWindow}\r\n";
            str += $"Desktop window {this.listView}\r\n";


            str += HitTestDesktopItem(pos).ToString();

            if (this.DebugText.Text != str)
                this.DebugText.Text = str;
            */

            timer1.Enabled = false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDebug();
        }

        private void RefreshDebug()
        {
            Shell.RefreshTrashCan();
            return;

            uint LVM_FIRST = 4096;
            uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
            uint LVM_GETITEMRECT = 4110;
            uint LVM_GETITEMW = 4171;
            uint LVM_GETITEMA = 4101;
            uint LVM_GETITEMTEXTW = 4211;
            uint LVM_GETTILEINFO = 4261;
            uint LVM_GETHEADER = 4127;
            uint LVM_GETCOUNTPERPAGE = 4136;
            uint LVM_GETCOLUMNW = 4191;
            uint LVM_GETGROUPINFO = 4245; // user get info by index instead!!!!

            uint LVM_INSERTITEM = LVM_FIRST + 77;

            uint LVM_GETGROUPCOUNT = LVM_FIRST + 152;

            uint HDM_GETITEMCOUNT = 4608;
            uint HDM_GETITEM = 0x1200;

            uint LVM_GETSUBITEMRECT = 4152;

            uint LVIF_TEXT = 0x00000001;
            uint LVIF_IMAGE = 0x00000002;
            uint LVIF_PARAM = 0x00000004;
            uint LVIF_STATE = 0x00000008;
            uint LVIF_INDENT = 0x00000010;
            uint LVIF_NORECOMPUTE = 0x00000800;
            uint LVIF_GROUPID = 0x00000100;
            uint LVIF_COLUMNS = 0x00000200;

            uint LVGF_ITEMS = 0x4000;
            uint LVGF_STATE = 0x00000004;

            uint HDI_TEXT = 0x0002;
            uint HDI_WIDTH = 0x0001;
            /*
#define HDI_HEIGHT              HDI_WIDTH
#define HDI_TEXT                0x0002
#define HDI_FORMAT              0x0004
#define HDI_LPARAM              0x0008
#define HDI_BITMAP              0x0010
#define HDI_IMAGE               0x0020
#define HDI_DI_SETITEM          0x0040
#define HDI_ORDER               0x0080
#define HDI_FILTER              0x0100
            */

            var list = ListView.FromHandle(this.listView);

            var desktopList = ExternalWindow.FromHandle(this.listView);
            var header = ExternalWindow.FromHandle(desktopList.SendMessage(LVM_GETHEADER));

            var items = desktopList.SendMessage(LVM_GETITEMCOUNT);

            var columns = header.SendMessage(HDM_GETITEMCOUNT);

            for (var i = 0; i < (int)columns; i++)
            {
                var column = new HD_ITEMA();
                column.mask = HDI_TEXT | HDI_WIDTH;
                column.pszText = header.AllocateMemory(256);
                column.cchTextMax = 256;

                var mem = header.WriteToMemory(column);
                header.SendMessage(HDM_GETITEM, (IntPtr)i, mem);
                var read = desktopList.ReadMemory<HD_ITEMA>(mem);

                var pszText = desktopList.ReadMemory(read.pszText, 256);

                desktopList.FreeMemory(mem);
                desktopList.FreeMemory(read.pszText);
            }

            var groups = desktopList.SendMessage(LVM_GETGROUPCOUNT);

            for (var i = 0; i < (int)0; i++)
            {
                for (var j = 0; j < (int)0; j++)
                {
                    var item = new tagLVITEMA();
                    item.iItem = i;
                    item.iSubItem = j;
                    item.mask = LVIF_COLUMNS | LVIF_TEXT | LVIF_PARAM | LVIF_STATE | LVIF_GROUPID | LVIF_IMAGE | LVIF_INDENT;
                    item.pszText = desktopList.AllocateMemory(256);
                    item.cchTextMax = 256;

                    var mem = desktopList.WriteToMemory(item);
                    var res = desktopList.SendMessage(LVM_GETITEMA, IntPtr.Zero, mem);
                    var read = desktopList.ReadMemory<tagLVITEMA>(mem);

                    var pszText = desktopList.ReadMemory(read.pszText, 256);

                    desktopList.FreeMemory(mem);
                    desktopList.FreeMemory(read.pszText);
                }



            }

            var browser = new Vanara.Windows.Shell.ShellBrowser();

           

            var w = (Vanara.PInvoke.Shell32.IShellWindows) new Vanara.PInvoke.Shell32.ShellWindows();


            var obj1 = new object();
            var obj2 = new object();

            int hwnd;

            var disp = w.FindWindowSW(obj1, obj2, Vanara.PInvoke.Shell32.ShellWindowTypeConstants.SWC_DESKTOP, out hwnd, Vanara.PInvoke.Shell32.ShellWindowFindWindowOptions.SWFO_NEEDDISPATCH);

            var service = (Vanara.PInvoke.Shell32.IServiceProvider)disp;

            Guid IID_IShellBrowser = new Guid("000214e2-0000-0000-c000-000000000046");
            Guid IID_IShellFolder = new Guid("000214e6-0000-0000-c000-000000000046");

            IntPtr obj3;

            service.QueryService(Vanara.PInvoke.Shell32.SID_STopLevelBrowser, IID_IShellBrowser, out obj3);


            var obj4 = Marshal.GetObjectForIUnknown(obj3);

            var brz = (Vanara.PInvoke.Shell32.IShellBrowser)obj4;

            Vanara.PInvoke.Shell32.IShellView view;

            brz.QueryActiveShellView(out view);

            

            var folderView = (Vanara.PInvoke.Shell32.IFolderView)view;

            var folder = (Vanara.PInvoke.Shell32.IShellFolder)folderView.GetFolder(IID_IShellFolder);

            var brz2 = (Vanara.PInvoke.Shell32.IShellBrowser)new Vanara.Windows.Shell.ShellBrowser();

            Vanara.PInvoke.HWND brzWindow;
            brz2.GetWindow(out brzWindow);

            IntPtr destop = User32.GetDesktopWindow();

            var _SHELLDLL_DefView = FindShellDefView();

            folderView.GetFocusedItem();

            var cnt = folderView.ItemCount(Vanara.PInvoke.Shell32.SVGIO.SVGIO_ALLVIEW);

            this.DebugText.Clear();


            this.DebugText.Text += $"Count: {cnt}\r\n";

            var files = System.IO.Directory.GetFileSystemEntries("C:\\Users\\Ivan\\Desktop");



            var bin = Vanara.Windows.Shell.RecycleBin.ShellFolderInstance;

            var binName = bin.GetDisplayName(Vanara.Windows.Shell.ShellItemDisplayString.NormalDisplay);

            var binPos = folderView.GetItemPosition(bin.PIDL);

            //browser.po

            //service.QueryService()

            for (var i = 0; i < cnt; i++)
            {
                var pid = folderView.Item(i);


                var pos = folderView.GetItemPosition(pid);

                Vanara.PInvoke.Shell32.STRRET strret;

                folder.GetDisplayNameOf(pid, Vanara.PInvoke.Shell32.SHGDNF.SHGDN_NORMAL, out strret);

                string name = strret;

                this.DebugText.Text += $"\r\n";
                this.DebugText.Text += $"{name}: {pos.X}, {pos.Y}";


                //folderView.getdi

                var bp = 0;
            }

           // brsr.QueryActiveShellView(out view);

           

            IntPtr vProcess = desktopList.ProcessHandle;
            IntPtr vPointer = desktopList.AllocateMemory(sizeof(uint));

            for (int j = 0; j < (int)items; j++)
            {
                /*
                byte[] vBuffer = new byte[256];
                LVITEM[] vItem = new LVITEM[1];
                vItem[0].mask = LVIF_TEXT;
                vItem[0].iItem = j;
                vItem[0].iSubItem = 0;
                vItem[0].cchTextMax = vBuffer.Length;
                vItem[0].pszText = (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM)));
                uint vNumberOfBytesRead = 0;
                NativeMethods.WriteProcessMemory(vProcess, vPointer, Marshal.UnsafeAddrOfPinnedArrayElement(vItem, 0), Marshal.SizeOf(typeof(LVITEM)), ref vNumberOfBytesRead);
                desktopList.SendMessage(LVM_GETITEMW, (IntPtr)j, vPointer);
                ReadProcessMemory(vProcess, (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM))), Marshal.UnsafeAddrOfPinnedArrayElement(vBuffer, 0), vBuffer.Length, out vNumberOfBytesRead);

                // Get the name of the Icon
                vText = Encoding.Unicode.GetString(vBuffer, 0, (int)vNumberOfBytesRead);

                // Get  Icon location
                SendMessage(hwndIcon, LVM_GETITEMPOSITION, j, vPointer.ToInt32());
                Point[] vPoint = new Point[1];
                foo = Marshal.UnsafeAddrOfPinnedArrayElement(vPoint, 0);
                ReadProcessMemory(vProcess, vPointer, Marshal.UnsafeAddrOfPinnedArrayElement(vPoint, 0), Marshal.SizeOf(typeof(Point)), out vNumberOfBytesRead);

                //and ultimaely move icon.
                SendMessage(hwndIcon, LVM_SETITEMPOSITION, j, lParam[0]);

              
                  */
            }

            var rectMem = desktopList.WriteToMemory(new RECT());
            var rectRes = desktopList.SendMessage(LVM_GETITEMRECT, items - 1, rectMem);
            var rect    = desktopList.ReadMemory<RECT>(rectMem);

              

                LVITEM item0 = new LVITEM();
            item0.iItem = 0;
            item0.iSubItem = 4;
            item0.mask = LVIF_TEXT;
            item0.pszText = desktopList.AllocateMemory(255);
            item0.cchTextMax = 255;

            var itemMem = desktopList.WriteToMemory(item0);
            var itemRes = desktopList.SendMessage(LVM_GETITEMA, IntPtr.Zero, itemMem);
            var readItem = desktopList.ReadMemory<LVITEM>(itemMem);
            var readText = desktopList.ReadMemory(readItem.pszText, 255);

            string text = new System.Text.ASCIIEncoding().GetString(readText);

            var tile = new LVTILEINFO();
            tile.puColumns = desktopList.AllocateMemory(255);
            tile.piColFmt = desktopList.AllocateMemory(255);

            var tileMem = desktopList.WriteToMemory(tile);
            var tileRes = desktopList.SendMessage(LVM_GETTILEINFO, IntPtr.Zero, tileMem);
            var readTile = desktopList.ReadMemory<LVTILEINFO>(tileMem);
            //var readText = desktopList.ReadMemory(readItem.pszText, 255);

            var newStr = new System.Text.UnicodeEncoding().GetBytes("Inserted via app");

            var insItem = new tagLVITEMA();
            insItem.cchTextMax = newStr.Length;
            insItem.pszText = desktopList.AllocateMemory(newStr.Length);
            desktopList.WriteToMemory(insItem.pszText, newStr);
            insItem.mask = LVIF_TEXT;

            itemMem = desktopList.WriteToMemory(insItem);
            //itemRes = desktopList.SendMessage(LVM_INSERTITEM, IntPtr.Zero, itemMem);

           

          
        }

        struct LVTILEINFO
        {
            public uint cbSize;
            public int iItem;
            public uint cColumns;
            public IntPtr puColumns;
            public IntPtr piColFmt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LVGROUP
        {
            public int cbSize;
            public int mask;
            public IntPtr pszHeader;
            public uint cchHeader;
            public IntPtr pszFooter;
            public uint cchFooter;
            public int iGroupId;
            public int stateMask;
            public int state;
            public uint uAlign;
            public IntPtr pszSubtitle;
            public uint cchSubtitle;
            public IntPtr pszTask;
            public uint cchTask;
            public IntPtr pszDescriptionTop;
            public uint cchDescriptionTop;
            public IntPtr pszDescriptionBottom;
            public uint cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public uint cItems;
            public IntPtr pszSubsetTitle;
            public uint cchSubsetTitle;
        }

        struct tagLVGROUP
        {
            public uint cbSize;
            public uint mask;
            public IntPtr pszHeader;
            public int cchHeader;
            public IntPtr pszFooter;
            public int cchFooter;
            public int iGroupId;
            public uint stateMask;
            public uint state;
            public uint uAlign;
            public IntPtr pszSubtitle;
            public uint cchSubtitle;
            public IntPtr pszTask;
            public uint cchTask;
            public IntPtr pszDescriptionTop;
            public uint cchDescriptionTop;
            public IntPtr pszDescriptionBottom;
            public uint cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public uint cItems;
            public IntPtr pszSubsetTitle;
            public uint cchSubsetTitle;
        }

        struct tagLVITEMA
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public uint cColumns;
            public IntPtr puColumns;
            public IntPtr piColFmt;
            public int iGroup;
        }

        struct HD_ITEMA
        {
            public uint mask;
            public int cxy;
            public IntPtr pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            public uint type;
            public IntPtr pvFilter;
            public uint state;
        }

        public void SelectionChanged()
        {
            
        }

        public void EnumDone()
        {
            
        }

        public bool VerbInvoked()
        {
            return true;
        }

        public bool DefaultVerbInvoked()
        {
            return true;
        }

        public bool BeginDrag()
        {
            Action action = () => DebugText.Text = "is dragging";

            DebugText.Invoke(action);

            return true;
        }
    }
}
