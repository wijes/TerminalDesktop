using System.Linq;

namespace TerminalDesktopMod
{
    public class WalkieIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowPrefab;
        public override void Click()
        {
            if (TerminalDesktopManager.Instance.DesktopWindows.Any(win => win is WalkieWindow))
                return;
            base.Click();
            TerminalDesktopManager.Instance.AddWindow(WindowPrefab);
        }
    }
}