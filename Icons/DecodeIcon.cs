using System.Linq;
using UnityEngine;

namespace TerminalDesktopMod
{
    public class DecodeIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowPrefab;
        public override void Click()
        {
            if (TerminalDesktopManager.Instance.DesktopWindows.Any(win => win is DecodeWindow))
                return;
            base.Click();
            TerminalDesktopManager.Instance.AddWindow(WindowPrefab);
        }
    }
}