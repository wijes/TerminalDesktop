using System.Linq;
using TerminalDesktopMod.Extentions;
using TerminalDesktopMod.Sync;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TerminalDesktopMod
{
    public class MineWindow : DesktopWindowBase
    {
        public Slider Slider;
        public DesktopWindowBase VirusAttackWindow;
        private int CurrentMinePower { get; set; }
        private float TimeToGetMoney { get; set; } = 60;
        private float CurrentTimeToGetMoney { get; set; } = 60;

        public virtual void FixedUpdate()
        {
            if (!TerminalDesktopManager.Instance.IsServer)
                return;
            if (!TimeOfDay.Instance.currentDayTimeStarted)
                return;
            if (CurrentMinePower == 0)
                return;
            if (CurrentTimeToGetMoney > 0)
            {
                CurrentTimeToGetMoney -= Time.fixedDeltaTime;
                return;
            }

            CurrentTimeToGetMoney = TimeToGetMoney;
            ReferencesStorage.Terminal.ChangeCredits(6 * CurrentMinePower);
            if (Random.Range(0, 100) <= 7 * CurrentMinePower)
            {
                if (TerminalDesktopManager.Instance.DesktopWindows.Any(win => win is HackAttackWindow))
                    return;
                TerminalDesktopManager.Instance.WindowAddedEvent.AddListener(SetVirusSettings);
                TerminalDesktopManager.Instance.AddWindow(VirusAttackWindow);
            }
        }
        private void SetVirusSettings(DesktopWindowBase win)
        {
            if (win is HackAttackWindow hackAttackWindow)
            {
                hackAttackWindow.Init(CurrentMinePower);
                TerminalDesktopManager.Instance.WindowAddedEvent.RemoveListener(SetVirusSettings);
            }
        }
        public void ChangeMinePower(BaseEventData baseEventData)
        {
            var newPower = (int)Slider.value;
            if (newPower == CurrentMinePower)
                return;
            var changePower = newPower - CurrentMinePower;
            if (TerminalDesktopManager.Instance.GetFreeEnergy() < changePower)
            {
                Slider.value = CurrentMinePower;
                TerminalDesktopManager.Instance.AddNotificationWindow("not enough energy");
                return;
            }
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomInt = true,
                CustomInt = newPower
            });
        }

        public override void CloseWindow()
        {
            TerminalDesktopManager.Instance.ChangeUseEnergy(-CurrentMinePower);
        }

        public override void WindowSync(WindowSync windowSync)
        {
            base.WindowSync(windowSync);
            if (windowSync.SyncCustomInt)
            {
                TerminalDesktopManager.Instance.ChangeUseEnergy(windowSync.CustomInt - CurrentMinePower);
                CurrentMinePower = windowSync.CustomInt;
                Slider.value = CurrentMinePower;
            }
        }

        public override WindowSync GetFullWindowSync()
        {
            return new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = WindowCanvasGroup.alpha == 0,
                SyncPosition = true,
                Position = transform.localPosition,
                SyncScale = true,
                Scale = WindowContainer.sizeDelta,
                SyncCustomInt = true,
                CustomInt = CurrentMinePower
            };
        }
    }
}