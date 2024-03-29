﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace AnimatedIcon
{
    class Desktop
    {
        private Shell32.IShellFolder folder;
        private Shell32.IShellView3 view;
        private Shell32.IFolderView2 folderView;
        private Shell32.IShellFolderViewDual3 dualView;
        private FolderViewEvents events;

        private HWND hwnd = HWND.NULL;

        public EventHandler StartDragging = delegate { };

        public Desktop()
        {
            Shell32.SHGetDesktopFolder(out this.folder);
            this.view = Shell.GetDesktopView();
            this.folderView = (Shell32.IFolderView2)this.view;
            this.dualView = Shell.GetDesktopAutomationObject();
            this.events = new FolderViewEvents(this.view);
            this.events.StartDrag += this.OnStartDragging;
        }

        public bool HitTest(Point point)
        {
            return User32.WindowFromPoint(point) == this.hwnd;
        }

        public bool IsSelected(Shell32.PIDL item)
        {
            return Shell.IsSelected(this.view, item);
        }

        public Point GetPosition(Shell32.PIDL item)
        {
            return this.folderView.GetItemPosition(item);
        }

        public void OnStartDragging(object sender, EventArgs e)
        {
            this.hwnd = User32.WindowFromPoint(Cursor.Position);
            this.StartDragging(sender, e);
        }
    }
}
