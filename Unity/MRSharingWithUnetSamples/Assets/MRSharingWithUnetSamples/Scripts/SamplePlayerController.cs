// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Assets.MRSharingWithUnetSamples.Scripts;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SharingWithUNET;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.UNETTestSamples.Scripts
{
    /// <summary>
    ///     Controls player behavior (local and remote).
    /// </summary>
    [NetworkSettings(sendInterval = 0.033f)]
    public class SamplePlayerController : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        ///     The transform of the shared world anchor.
        /// </summary>
        private Transform _sharedWorldAnchorTransform;

        /// <summary>
        ///     The anchor manager.
        /// </summary>
        private UNetAnchorManager _anchorManager;

        /// <summary>
        ///     Script that handles finding, creating, and joining sessions.
        /// </summary>
        private NetworkDiscoveryWithAnchors _networkDiscovery;

        private static SamplePlayerController _instance;

        /// <summary>
        ///     Instance of the PlayerController that represents the local player.
        /// </summary>
        public static SamplePlayerController Instance
        {
            get { return _instance; }
        }

        #endregion

        #region SharedProperties

        /// <summary>
        ///     The position relative to the shared world anchor.
        /// </summary>
        [SyncVar] private Vector3 _localPosition;

        /// <summary>
        ///     The rotation relative to the shared world anchor.
        /// </summary>
        [SyncVar] private Quaternion _localRotation;

        /// <summary>
        ///     Tracks if the player associated with the script has found the shared anchor
        /// </summary>
        [SyncVar(hook = "AnchorEstablishedChanged")]
        private bool _anchorEstablished;

        /// <summary>
        ///     Called when the anchor is either lost or found
        /// </summary>
        /// <param name="update">true if the anchor is found</param>
        private void AnchorEstablishedChanged(bool update)
        {
            Debug.LogFormat("AnchorEstablished for {0} was {1} is now {2}", _playerName, _anchorEstablished, update);
            _anchorEstablished = update;
            // only draw the mesh for the player if the anchor is found.
            //GetComponentInChildren<MeshRenderer>().enabled = update;
        }

        /// <summary>
        ///     Tracks the player name.
        /// </summary>
        [SyncVar(hook = "PlayerNameChanged")] private string _playerName;

        /// <summary>
        ///     Called when the player name changes.
        /// </summary>
        /// <param name="update">the updated name</param>
        private void PlayerNameChanged(string update)
        {
            Debug.LogFormat("Player name changing from {0} to {1}", _playerName, update);
            _playerName = update;
            // Special case for spectator view
            if (_playerName.ToLower() == "spectatorviewpc") gameObject.SetActive(false);
        }

        [SyncVar(hook = "CurrentDisplayTypeChanged")]
        private MixedRealityCameraManager.DisplayType _currentDisplayType;

        private void CurrentDisplayTypeChanged(MixedRealityCameraManager.DisplayType update)
        {
            Debug.LogFormat("CurrentDisplayType changing from {0} to {1}", _currentDisplayType, update);
            _currentDisplayType = update;
            _refreshDeviceController = true;
        }

        private bool _refreshDeviceController = false;

#pragma warning disable 0414
        /// <summary>
        ///     Keeps track of the player's IP address
        /// </summary>
        [SyncVar(hook = "PlayerIpChanged")] private string _playerIp;

        /// <summary>
        ///     Called when the player IP address changes
        /// </summary>
        /// <param name="update">The updated IP address</param>
        private void PlayerIpChanged(string update)
        {
            _playerIp = update;
        }

        /// <summary>
        ///     Tracks if the player can share spatial anchors
        /// </summary>
        [SyncVar(hook = "SharesAnchorsChanged")]
        public bool SharesSpatialAnchors;

        /// <summary>
        ///     Called when the ability to share spatial anchors changes
        /// </summary>
        /// <param name="update">True if the device can share spatial anchors.</param>
        private void SharesAnchorsChanged(bool update)
        {
            SharesSpatialAnchors = update;
            Debug.LogFormat("{0} {1} share", _playerName, SharesSpatialAnchors ? "does" : "does not");
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The game object that represents the 'bullet' for
        ///     this player. Must exist in the spawnable prefabs on the
        ///     NetworkManager.
        /// </summary>
        public GameObject Bullet;

        public bool CanShareAnchors;
        private DeviceControllerBase _deviceController;
        private DeviceControllerBase[] _deviceControllerBases;

        #endregion

        #region UnityMethods

        private void Awake()
        {
            _networkDiscovery = NetworkDiscoveryWithAnchors.Instance;
            _anchorManager = UNetAnchorManager.Instance;
        }

        private void Start()
        {
            if (SharedCollection.Instance == null)
            {
                Debug.LogError("This script required a SharedCollection script attached to a GameObject in the scene");
                Destroy(this);
                return;
            }

            _deviceControllerBases = GetComponents<DeviceControllerBase>();

            SetDeviceController();

            if (isLocalPlayer)
            {
                // If we are the local player then we want to have airtaps 
                // sent to this object so that projectiles can be spawned.
                InputManager.Instance.AddGlobalListener(gameObject);
                CmdSetCurrentDisplayType(MixedRealityCameraManager.Instance.CurrentDisplayType);
                InitializeLocalPlayer();
            }
            else
            {
                Debug.Log("remote player");
                InitializeRemotePlayer();
            }
            
            _sharedWorldAnchorTransform = SharedCollection.Instance.gameObject.transform;
            transform.SetParent(_sharedWorldAnchorTransform);
        }

        private void SetDeviceController()
        {
            if (_deviceControllerBases.Length == 0)
            {
                Debug.LogError(
                    "This script required one or more DeviceController(inheritate DeviceControllerBase) script attached to this GameObject.");
                Destroy(this);
                return;
            }

            foreach (var controller in _deviceControllerBases)
                if (isLocalPlayer && MixedRealityCameraManager.Instance.CurrentDisplayType ==
                    controller.SupportDisplayType
                    || !isLocalPlayer && _currentDisplayType == controller.SupportDisplayType)
                {
                    _deviceController = controller;
                    controller.enabled = true;
                }
                else
                {
                    controller.enabled = false;
                    controller.RemovePlayerObject();
                }

            _deviceController.SetNetworkBehaviour(this);
            _deviceController.PlayerInstance = gameObject;

            if (isLocalPlayer)
            {
                _deviceController.InitializeLocalPlayer();
            }
            else
            {
                _deviceController.InitializeRemotePlayer();
            }
        }

        private void Update()
        {
            if (_refreshDeviceController)
            {
                SetDeviceController();
                _refreshDeviceController = false;
            }

            // If we aren't the local player, we just need to make sure that the position of this object is set properly
            // so that we properly render their avatar in our world.
            if (!isLocalPlayer && string.IsNullOrEmpty(_playerName) == false)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _localPosition, 0.3f);
                transform.localRotation = _localRotation;
                return;
            }

            if (!isLocalPlayer) return;

            // if our anchor established state has changed, update everyone
            if (_anchorEstablished != _anchorManager.AnchorEstablished)
                CmdSendAnchorEstablished(_anchorManager.AnchorEstablished);

            // if our anchor isn't established, we shouldn't bother sending transforms.
            if (_anchorEstablished == false)
            {
                //return;
            }

            _deviceController.LocalPlayerUpdate();

            // For UNET we use a command to signal the host to update our local position
            // and rotation
            CmdTransform(transform.localPosition, transform.localRotation);
        }

        private void OnDestroy()
        {
            if (isLocalPlayer) InputManager.Instance.RemoveGlobalListener(gameObject);
        }

        #endregion

        #region NetworkCommands

        /// <summary>
        ///     Called on the host when a bullet needs to be added.
        ///     This will 'spawn' the bullet on all clients, including the
        ///     client on the host.
        /// </summary>
        [Command]
        public void CmdFire()
        {
            var bulletDir = transform.forward;
            var bulletPos = transform.position + bulletDir * 1.5f;

            // The bullet needs to be transformed relative to the shared anchor.
            var nextBullet = Instantiate(Bullet, _sharedWorldAnchorTransform.InverseTransformPoint(bulletPos),
                Quaternion.Euler(bulletDir));
            nextBullet.GetComponentInChildren<Rigidbody>().velocity = bulletDir * 1.0f;
            NetworkServer.Spawn(nextBullet);

            // Clean up the bullet in 8 seconds.
            Destroy(nextBullet, 8.0f);
        }

        /// <summary>
        ///     Called to set the IP address
        /// </summary>
        /// <param name="playerIp"></param>
        [Command]
        private void CmdSetPlayerIp(string playerIp)
        {
            _playerIp = playerIp;
        }

        /// <summary>
        ///     Called to update if the player can share spatial anchors.
        /// </summary>
        /// <param name="canShareAnchors">True if the device can share spatial anchors.</param>
        [Command]
        private void CmdSetCanShareAnchors(bool canShareAnchors)
        {
            Debug.Log("CMDSetCanShare " + canShareAnchors);
            SharesSpatialAnchors = canShareAnchors;
        }

        [Command]
        private void CmdSendSharedTransform(GameObject target, Vector3 pos, Quaternion rot)
        {
            var ush = target.GetComponent<UNetSharedHologram>();
            ush.CmdTransform(pos, rot);
        }

        /// <summary>
        ///     Sets the localPosition and localRotation on clients.
        /// </summary>
        /// <param name="postion">the localPosition to set</param>
        /// <param name="rotation">the localRotation to set</param>
        [Command(channel = 1)]
        public void CmdTransform(Vector3 postion, Quaternion rotation)
        {
            _localPosition = postion;
            _localRotation = rotation;
        }

        /// <summary>
        ///     Sent from a local client to the host to update if the shared
        ///     anchor has been found.
        /// </summary>
        /// <param name="established">true if the shared anchor is found</param>
        [Command]
        private void CmdSendAnchorEstablished(bool established)
        {
            _anchorEstablished = established;
            if (established && SharesSpatialAnchors && !isLocalPlayer)
            {
                Debug.Log("remote device likes the anchor");
#if UNITY_WSA
                _anchorManager.AnchorFoundRemotely();
#endif
            }
        }

        /// <summary>
        ///     Called to set the player name
        /// </summary>
        /// <param name="playerName">The name to update to</param>
        [Command]
        private void CmdSetPlayerName(string playerName)
        {
            _playerName = playerName;
        }

        [Command]
        private void CmdSetCurrentDisplayType(MixedRealityCameraManager.DisplayType displayType)
        {
            _currentDisplayType = displayType;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets up all of the local player information
        /// </summary>
        private void InitializeLocalPlayer()
        {
            if (isLocalPlayer)
            {
                Debug.Log("Setting instance for local player ");
                _instance = this;
                Debug.LogFormat("Set local player name {0} IP {1}", _networkDiscovery.broadcastData,
                    _networkDiscovery.LocalIp);
                CmdSetPlayerName(_networkDiscovery.broadcastData);
                CmdSetPlayerIp(_networkDiscovery.LocalIp);
#if UNITY_WSA
#if UNITY_2017_2_OR_NEWER
                CanShareAnchors = !UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque;
#else
                CanShareAnchors = !Application.isEditor && UnityEngine.VR.VRDevice.isPresent;
#endif
#endif
                Debug.LogFormat("local player {0} share anchors ", CanShareAnchors ? "does not" : "does");
                CmdSetCanShareAnchors(CanShareAnchors);
             }
        }

        private void InitializeRemotePlayer()
        {
            AnchorEstablishedChanged(_anchorEstablished);
            SharesAnchorsChanged(SharesSpatialAnchors);
        }

        /// <summary>
        ///     For sending transforms for holograms which do not frequently change.
        /// </summary>
        /// <param name="target">The shared hologram</param>
        /// <param name="pos">position relative to the shared anchor</param>
        /// <param name="rot">rotation relative to the shared anchor</param>
        public void SendSharedTransform(GameObject target, Vector3 pos, Quaternion rot)
        {
            if (isLocalPlayer) CmdSendSharedTransform(target, pos, rot);
        }

        #endregion

#pragma warning restore 0414
    }
}