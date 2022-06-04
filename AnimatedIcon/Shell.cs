using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using Vanara.PInvoke;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimatedIcon
{
    public class FolderViewEvents
    {
        [ComVisible(true)]
        [ClassInterface(ClassInterfaceType.None)]
        [ComDefaultInterface(typeof(Shell32.DShellFolderViewEvents))]
        public class CoClassEvents : Shell32.DShellFolderViewEvents
        {
            FolderViewEvents parent;

            public CoClassEvents(FolderViewEvents parent)
            {
                this.parent = parent;
            }

            public void SelectionChanged()
            {
                this.parent.SelectionChanged(this.parent, EventArgs.Empty);
            }

            public void EnumDone()
            {
                this.parent.EnumDone(this.parent, EventArgs.Empty);
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
                this.parent.StartDrag(this.parent, EventArgs.Empty);
                return true;
            }
        }

        Interop.ConnectionPoint<Shell32.IShellFolderViewDual, Shell32.DShellFolderViewEvents> point;
        CoClassEvents events;

        public EventHandler SelectionChanged = delegate { };
        public EventHandler EnumDone = delegate { };
        public EventHandler StartDrag = delegate { };

        public FolderViewEvents(Shell32.IShellView view)
        {
            var disp = view.GetItemObject(Shell32.SVGIO.SVGIO_BACKGROUND, Interop.Constants.IID_IDispatch);
            var dual = (Shell32.IShellFolderViewDual3)disp;

            this.events = new CoClassEvents(this);
            this.point = new Interop.ConnectionPoint<Shell32.IShellFolderViewDual, Shell32.DShellFolderViewEvents>(dual, this.events);
            this.point.Advise();
        }

        ~FolderViewEvents()
        {
            this.point.Unadvise();
        }
    }

    class Shell
    {
        private static readonly int HRESULT_ERROR_NOT_FOUND = unchecked((int)0x80070490);

        public static void RefreshItem(Shell32.PIDL item)
        {
            Shell32.SHChangeNotify(Shell32.SHCNE.SHCNE_UPDATEITEM, Shell32.SHCNF.SHCNF_FLUSHNOWAIT, item, Shell32.PIDL.Null);
        }

        public static Shell32.IShellView3 GetDesktopView()
        {
            var main = (Shell32.IShellWindows)new Shell32.ShellWindows();

            int hDesktop;

            var wType = Shell32.ShellWindowTypeConstants.SWC_DESKTOP;
            var findOption = Shell32.ShellWindowFindWindowOptions.SWFO_NEEDDISPATCH;
            var pDispatch = main.FindWindowSW(new object(), new object(), wType, out hDesktop, findOption);

            var service = (Shell32.IServiceProvider)pDispatch;

            Guid IID_IShellBrowser = new Guid("000214e2-0000-0000-c000-000000000046");
            Guid IID_IShellFolder = new Guid("000214e6-0000-0000-c000-000000000046");

            IntPtr pBrowserUnknown;
            service.QueryService(Shell32.SID_STopLevelBrowser, IID_IShellBrowser, out pBrowserUnknown);

            var browser = (Shell32.IShellBrowser)Marshal.GetObjectForIUnknown(pBrowserUnknown);

            Shell32.IShellView view;
            browser.QueryActiveShellView(out view);

            return (Shell32.IShellView3)view;
        }

        public static Shell32.IShellFolderViewDual3 GetDesktopAutomationObject()
        {
            var view = (Shell32.IShellView)GetDesktopView();

            var IID_IDispatch = new Guid("00020400-0000-0000-C000-000000000046");

            var obj = view.GetItemObject(Shell32.SVGIO.SVGIO_BACKGROUND, IID_IDispatch);

            return (Shell32.IShellFolderViewDual3)obj;
        }

        public static List<IntPtr> GetSelectedItems(Shell32.IShellView view)
        {
            var result = new List<IntPtr>();

            Shell32.IEnumIDList list = null;
            try
            {
                list = view.GetItemObject<Shell32.IEnumIDList>(Shell32.SVGIO.SVGIO_SELECTION);
            }
            catch (Exception ex)
            {
                Debug.Assert(ex.HResult == HRESULT_ERROR_NOT_FOUND);
            }

            if (list == null)
                return result;

            IntPtr[] pidls = new IntPtr[1];
            uint fetched = 0;

            list.Reset();
            list.Next(1, pidls, out fetched);
            while (fetched > 0)
            {
                result.Add(pidls[0]);
                list.Next(1, pidls, out fetched);
            }

            return result;
        }

        public static bool IsSelected(Shell32.IShellView view, Shell32.PIDL pidl)
        {
            Vanara.PInvoke.Shell32.IShellFolder ishDesk;
            Vanara.PInvoke.Shell32.SHGetDesktopFolder(out ishDesk);
            IntPtr SHCIDS_CANONICALONLY = (IntPtr)268435456;

            foreach (var item in GetSelectedItems(view))
            {
                var res = ishDesk.CompareIDs(SHCIDS_CANONICALONLY, item, pidl);
                if (res == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public HWND FindShellDefView()
        {
            var destop = User32.GetDesktopWindow();
            var hWorkerW = HWND.NULL;
            var hShellViewWin = HWND.NULL;
            do
            {
                hWorkerW = User32.FindWindowEx(destop, hWorkerW, "WorkerW", null);
                hShellViewWin = User32.FindWindowEx(hWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
            }
            while (hShellViewWin == IntPtr.Zero && hWorkerW != IntPtr.Zero);

            return hShellViewWin;
        }

        public HWND FindDesktopListView()
        {
            HWND list = HWND.NULL;
            while (list.IsNull)
            {
                var _SHELLDLL_DefView = FindShellDefView();
                list = User32.FindWindowEx(_SHELLDLL_DefView, IntPtr.Zero, "SysListView32", "FolderView");
            }
            return list;
        }
    }
}
