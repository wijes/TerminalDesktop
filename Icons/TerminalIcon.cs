using System.Linq;
using UnityEngine;

namespace TerminalDesktopMod
{
    public class TerminalIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowPrefab;
        public override void Click()
        {
            if (TerminalDesktopManager.Instance.DesktopWindows.Any(win => win is TerminalWindow))
                return;
            base.Click();
            TerminalDesktopManager.Instance.AddWindow(WindowPrefab);
        }
    }
}