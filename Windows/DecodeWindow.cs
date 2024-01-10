using System.Linq;
using TerminalDesktopMod.Sync;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TerminalDesktopMod
{
    public class DecodeWindow : DesktopWindowBase
    {
        public Button StartButton;
        public TextMeshProUGUI DecodeText;
        public TextMeshProUGUI DecodePercentText;

        private int DecodeLevel { get; set; } = 0;
        private float TimeToDecode { get; set; } = 60;
        private float CurrentTimeToDecode { get; set; } = 60;
        private UsbPort UseUsbPort { get; set; }
        private int MaxDecodeLevel { get; set; } = 3;
        private int DefaultDecodeTime { get; set; } = 220;

        protected override void Start()
        {
            base.Start();
            UsbPort.UsbPortChangeEvent.AddListener(UsbPortChanged);
        }

        public virtual void FixedUpdate()
        {
            if (!TimeOfDay.Instance.currentDayTimeStarted)
                return;
            if (DecodeLevel == 0)
                return;
            if (CurrentTimeToDecode > 0)
            {
                CurrentTimeToDecode -= Time.fixedDeltaTime;
                UpdatePercent();
                UpdateDecodeText();
                return;
            }
            
            if (!TerminalDesktopManager.Instance.IsServer)
                return;
            CurrentTimeToDecode = TimeToDecode;
            EndSuccessDecode();
        }

        protected virtual void UpdatePercent()
        {
            var percent = CurrentTimeToDecode / TimeToDecode;
            percent *= 100;
            percent = 100 - percent;
            percent = Mathf.Round(percent);
            DecodePercentText.text = $"{percent}%";
        }
        protected virtual void UpdateDecodeText()
        {
            var text = DecodeText.text;
            var rndTextIndex = Random.Range(0, text.Length - 1);
            var decodeChar = text[rndTextIndex];
            if (decodeChar == ' ') return;
            text = text.Remove(rndTextIndex, 1)
                .Insert(rndTextIndex, decodeChar == '1' ? "0" : "1");
            DecodeText.text = text;
        }

        protected virtual void EndSuccessDecode()
        {
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomInt = true
            });
        }
        public virtual void ClickStart()
        {
            var port = TerminalDesktopManager.Instance.UsbPorts.First();
            if (port.FlashInUsb is null)
            {
                TerminalDesktopManager.Instance.AddNotificationWindow("flash drive not found");
                return;
            }
            if (port.FlashInUsb.DecodeLevel.Value >= MaxDecodeLevel)
            {
                TerminalDesktopManager.Instance.AddNotificationWindow("the flash drive is completely decrypted");
                return;
            }
            if (TerminalDesktopManager.Instance.GetFreeEnergy() < port.FlashInUsb.DecodeLevel.Value + 1)
            {
                TerminalDesktopManager.Instance.AddNotificationWindow("not enough energy to decode");
                return;
            }
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomBool = true,
                CustomBool = true
            });
        }
        private void UsbPortChanged(UsbPort port)
        {
            if (port != UseUsbPort)
                return;
            if (port.FlashInUsb is null)
                EndDecode();

        }
        public virtual void EndDecode()
        {
            TerminalDesktopManager.Instance.ChangeUseEnergy(-DecodeLevel);
            DecodeLevel = 0;
            StartButton.gameObject.SetActive(true);
            DecodeText.enabled = false;
            DecodePercentText.transform.parent.gameObject.SetActive(false);
            UseUsbPort = null;
        }
        public override void CloseWindow()
        {
            TerminalDesktopManager.Instance.ChangeUseEnergy(-DecodeLevel);
        }

        public override void WindowSync(WindowSync windowSync)
        {
            base.WindowSync(windowSync);
            if (windowSync.SyncCustomBool)
            {
                UseUsbPort = TerminalDesktopManager.Instance.UsbPorts.First();
                DecodeLevel = UseUsbPort.FlashInUsb.DecodeLevel.Value + 1;
                TerminalDesktopManager.Instance.ChangeUseEnergy(DecodeLevel);
                TimeToDecode = DefaultDecodeTime * DecodeLevel;
                CurrentTimeToDecode = TimeToDecode;
                StartButton.gameObject.SetActive(false);
                DecodeText.enabled = true;
                DecodePercentText.transform.parent.gameObject.SetActive(true);
                UpdatePercent();
                UpdateDecodeText();
            }

            if (windowSync.SyncCustomFloat)
                CurrentTimeToDecode = windowSync.CustomFloat;
            if (windowSync.SyncCustomInt)
            {
                UseUsbPort.FlashInUsb.UpdateScrapValue(50);
                UseUsbPort.FlashInUsb.UpdateDecodeLevel(DecodeLevel);
                EndDecode();
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
                SyncCustomFloat = true,
                CustomFloat = CurrentTimeToDecode,
                SyncCustomBool = DecodeLevel != 0 
            };
        }
    }
}