using System.Linq;
using TerminalDesktopMod.Sync;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static TerminalApi.Events.Events;

namespace TerminalDesktopMod
{
    public class WalkieWindow : DesktopWindowBase
    {
        public static WalkieTalkie TerminalWalkieTalkie { get; set; }
        private bool IsActiveTalk { get; set; } = false;
        [SerializeField] private TextMeshProUGUI TalkText;

        protected override void Start()
        {
            base.Start();
            TerminalBeganUsing += OnTerminalBeganUsing;
            TerminalDesktopManager.Instance.TerminalExitEvent.AddListener(OnTerminalExitedUsing);
            if (TerminalWalkieTalkie is null || TerminalWalkieTalkie.Equals(null))
                CreateWalkie();
            else
            {
                if (!TerminalDesktopManager.Instance.IsServer)
                    return;
                TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
                {
                    SyncCustomRef = true,
                    CustomRef = TerminalWalkieTalkie
                });
            }
        }
        
        public void OnTerminalBeganUsing(object sender, TerminalEventArgs e)
        {
            if (TerminalWalkieTalkie is null || TerminalWalkieTalkie.Equals(null))
                return;
            TerminalDesktopManager.Instance.ChangeObjectOwnerServerRpc(
                TerminalWalkieTalkie.GetComponent<NetworkObject>(),
                NetworkManager.Singleton.LocalClientId);
        }
        private void OnTerminalExitedUsing()
        {
            if (IsActiveTalk)
                SwitchTalk();
        }

        private void FixedUpdate()
        {
            if (TerminalWalkieTalkie is null || TerminalWalkieTalkie.Equals(null))
                return;
            TerminalWalkieTalkie.targetFloorPosition = Vector3.zero;
            TerminalWalkieTalkie.insertedBattery.charge = 1;
            TerminalWalkieTalkie.insertedBattery.empty = false;
            if (IsActiveTalk && TerminalWalkieTalkie.IsOwner && !TerminalWalkieTalkie.clientIsHoldingAndSpeakingIntoThis)
            {
                TerminalWalkieTalkie.SetLocalClientSpeaking(true);
                TerminalWalkieTalkie.speakingIntoWalkieTalkie = true;
            }
        }

        public void SwitchTalk()
        {
            IsActiveTalk = !IsActiveTalk;
            if (IsActiveTalk)
            {
                TerminalWalkieTalkie.playerHeldBy = GameNetworkManager.Instance.localPlayerController;
                TerminalWalkieTalkie.SwitchWalkieTalkieOn(true);
                TerminalWalkieTalkie.ItemActivate(true);
                TalkText.text = "switch off";
            }
            else
            {
                TerminalWalkieTalkie.SetLocalClientSpeaking(false);
                TerminalWalkieTalkie.SwitchWalkieTalkieOn(false);
                TerminalWalkieTalkie.ItemActivate(false);
                TalkText.text = "switch on";
            }
        }
        
        public void CreateWalkie()
        {
            if (!TerminalDesktopManager.Instance.IsServer)
                return;
            var walkieItem =
                StartOfRound.Instance.allItemsList.itemsList.FirstOrDefault(x => x.itemName == "Walkie-talkie");
            var terminalWalkie = Instantiate(walkieItem.spawnPrefab);
            var netObj = terminalWalkie.GetComponent<NetworkObject>();
            netObj.Spawn();
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomRef = true,
                CustomRef = terminalWalkie.GetComponent<WalkieTalkie>()
            });
        }

        public override void CloseWindow()
        {
            base.CloseWindow();
            if (IsActiveTalk)
                SwitchTalk();
        }

        private void OnDestroy()
        {
            TerminalBeganUsing -= OnTerminalBeganUsing;
        }
        /// <summary>
        /// cleaning only when before leaving
        /// </summary>
        public static void ManagerDestroyed()
        {
            if (TerminalWalkieTalkie is null || TerminalWalkieTalkie.Equals(null))
                return;
            TerminalWalkieTalkie = null;
        }

        public override void WindowSync(WindowSync windowSync)
        {
            base.WindowSync(windowSync);
            if (windowSync.SyncCustomRef)
            {
                if (!windowSync.CustomRef.TryGet(out WalkieTalkie walkie))
                    return;
                TerminalWalkieTalkie = walkie;
                walkie.transform.parent = TerminalDesktopManager.Instance.transform;
                walkie.transform.localPosition = Vector3.zero;
                walkie.transform.localScale = Vector3.one * 0.01f;
                walkie.GetComponentInChildren<Renderer>().enabled = false;
                walkie.GetComponentInChildren<Collider>().enabled = false;
                TerminalWalkieTalkie.isInElevator = true;
                TerminalWalkieTalkie.isInShipRoom = true;
                if (!TerminalDesktopManager.Instance.IsUsingTerminal)
                    return;
                TerminalDesktopManager.Instance.ChangeObjectOwnerServerRpc(
                    TerminalWalkieTalkie.GetComponent<NetworkObject>(),
                    NetworkManager.Singleton.LocalClientId);
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
                SyncCustomRef = true,
                CustomRef = TerminalWalkieTalkie,
            };
        }
    }
}