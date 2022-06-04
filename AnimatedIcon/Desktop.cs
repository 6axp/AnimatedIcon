using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace AnimatedIcon
{
    class Desktop
    {
        private Shell32.IShellFolder folder;
        private Shell32.IShellView3 view;
        private Shell32.IShellFolderViewDual3 dualView;
        private FolderViewEvents events;

        public EventHandler StartDragging = delegate { };

        public Desktop()
        {
            Shell32.SHGetDesktopFolder(out this.folder);
            this.view = Shell.GetDesktopView();
            this.dualView = Shell.GetDesktopAutomationObject();
            this.events = new FolderViewEvents(this.view);
            this.events.StartDrag += this.OnStartDragging;
        }

        public bool IsSelected(Shell32.PIDL item)
        {
            return Shell.IsSelected(this.view, item);
        }

        public void OnStartDragging(object sender, EventArgs e)
        {
            this.StartDragging(sender, e);
        }
    }
}
