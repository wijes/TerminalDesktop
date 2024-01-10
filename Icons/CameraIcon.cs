using System.Linq;
using UnityEngine;

namespace TerminalDesktopMod
{
    public class CameraIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowPrefab;
        public override void Click()
        {
            base.Click();
            if (TerminalDesktopManager.Instance.GetFreeEnergy() <= 0)
            {
                TerminalDesktopManager.Instance.AddNotificationWindow("not enough energy to open the window");
                return;
            }
            TerminalDesktopManager.Instance.AddWindow(WindowPrefab);
        }
    }
}