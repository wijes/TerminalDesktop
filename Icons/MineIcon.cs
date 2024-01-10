using System.Linq;

namespace TerminalDesktopMod
{
    public class MineIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowPrefab;
        public override void Click()
        {
            if (TerminalDesktopManager.Instance.DesktopWindows.Any(win => win is MineWindow))
                return;
            base.Click();
            TerminalDesktopManager.Instance.AddWindow(WindowPrefab);
        }
    }
}