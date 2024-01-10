using GameNetcodeStuff;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TerminalDesktopMod
{
    public class UsbPortSaveModel
    {
        public int FlashInUsbIndex { get; set; }
    }
    public class UsbPort : NetworkBehaviour
    {
        public static UnityEvent<UsbPort> UsbPortChangeEvent = new UnityEvent<UsbPort>();
        public NetworkObject parentTo;
        public Vector3 OffsetFlash;
        public InteractTrigger triggerScript;
        public NetworkVariable<int> FlashInUsbIndex { get; set; } = new NetworkVariable<int>();
        public NetworkVariable<int> PortId { get; set; } = new NetworkVariable<int>();
        public FlashDriveProp FlashInUsb { get; set; }
        private void Awake()
        {
            FlashDriveProp.FlashLoadedEvent.AddListener(LoadFlash);
        }

        public override void OnNetworkSpawn()
        {
        }
        
        private void Start()
        {
            TerminalDesktopManager.Instance.UsbPorts.Add(this);
            var terminal = ReferencesStorage.Terminal;
            transform.parent.parent = terminal.terminalUIScreen.transform.parent;
            transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
            transform.parent.localPosition = Vector3.zero;
            transform.parent.localScale = Vector3.one;
        }
        
        protected virtual void FixedUpdate()
        {
            if (GameNetworkManager.Instance is null || GameNetworkManager.Instance.localPlayerController is null)
                return;
            triggerScript.interactable = IsHoldFlash();
            if (FlashInUsb is null)
                return;
            Vector3 vector3 = transform.localPosition + OffsetFlash;
            FlashInUsb.targetFloorPosition = vector3;
            FlashInUsb.transform.localPosition = vector3;
        }
        
        protected virtual bool IsHoldFlash()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            if (!player.isHoldingObject)
                return false;
            if (player.currentlyHeldObjectServer.itemProperties != DesktopStorage.FlashDriveItem)
                return false;
            if (FlashInUsb is not null)
                return false;
            return true;
        }
        public virtual void InsertIntoUsb(PlayerControllerB playerWhoTriggered)
        {
            if (!playerWhoTriggered.isHoldingObject || playerWhoTriggered.currentlyHeldObjectServer is null)
                return;
            Vector3 vector3 = transform.localPosition + OffsetFlash;
            var flash = playerWhoTriggered.currentlyHeldObjectServer;
            playerWhoTriggered.DiscardHeldObject(true, parentTo, vector3, true);
            InsertIntoUsbServerRpc(flash);
        }
        [ServerRpc(RequireOwnership = false)]
        private void InsertIntoUsbServerRpc(NetworkBehaviourReference flashRef)
        {
            if (!flashRef.TryGet(out FlashDriveProp flash))
                return;
            FlashInUsbIndex.Value = flash.FlashIndex;
            InsertIntoUsbClientRpc(flash);
        }
        [ClientRpc]
        private void InsertIntoUsbClientRpc(NetworkBehaviourReference flashRef)
        {
            if (!flashRef.TryGet(out FlashDriveProp flash))
                return;
            FlashInUsb = flash;
            flash.UsbPort = this;
            flash.transform.localRotation = Quaternion.Euler(0, 0, 180);
            UsbPortChangeEvent.Invoke(this);
        }

        public virtual void PulledFlash()
        {
            PulledFlashServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        private void PulledFlashServerRpc()
        {
            FlashInUsbIndex.Value = 0;
            PulledFlashClientRpc();
        }
        [ClientRpc]
        private void PulledFlashClientRpc()
        {
            FlashInUsb = null;
            UsbPortChangeEvent.Invoke(this);
        }
        public virtual void LoadFlash(FlashDriveProp flashDriveProp)
        {
            if (flashDriveProp.FlashIndex != FlashInUsbIndex.Value)
                return;
            FlashDriveProp.FlashLoadedEvent.RemoveListener(LoadFlash);

            flashDriveProp.reachedFloorTarget = true;
            flashDriveProp.fallTime = 1;
            flashDriveProp.transform.parent = transform.parent;
            flashDriveProp.transform.localRotation = Quaternion.Euler(0, 0, 180);
            flashDriveProp.hasHitGround = true;

            FlashInUsb = flashDriveProp;
            flashDriveProp.UsbPort = this;
            UsbPortChangeEvent.Invoke(this);
        }

        public virtual string GetSaveString()
        {
            var saveModel = new UsbPortSaveModel()
            {
                FlashInUsbIndex = FlashInUsbIndex.Value
            };
            return JsonConvert.SerializeObject(saveModel);
        }
        
        public virtual void LoadPortById(int id)
        {
            LoadPortByIdServerRpc(id);
        }
        [ServerRpc(RequireOwnership = false)]
        private void LoadPortByIdServerRpc(int id)
        {
            PortId.Value = id;
            LoadPort();
        }
        protected virtual void LoadPort()
        {
            if (!IsServer)
                return;
            if (!DesktopStorage.TerminalDesktopSaveModel.UsbPortsSaves.TryGetValue(PortId.Value, out var data))
                return;
            
            var saveModel = JsonConvert.DeserializeObject<UsbPortSaveModel>(data);
            FlashInUsbIndex.Value = saveModel.FlashInUsbIndex;
            Main.Log.LogInfo("load usb port");
        }
    }
}