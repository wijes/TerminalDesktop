using System.Linq;
using UnityEngine;

namespace TerminalDesktopMod
{
    public class TestIcon : DesktopIconBase
    {
        public DesktopWindowBase WindowBase;
        public override void Click()
        {
            //TerminalDesktopManager.Instance.Reset();
            /*TerminalDesktopManager.Instance.WindowAddedEvent.AddListener(Call);
            TerminalDesktopManager.Instance.AddWindow(WindowBase);*/
        }

        private void Call(DesktopWindowBase win)
        {
            if (win is HackAttackWindow hackAttackWindow)
            {
                hackAttackWindow.Init(Random.Range(2,5));
                TerminalDesktopManager.Instance.WindowAddedEvent.RemoveListener(Call);
            }
        }
    }
}