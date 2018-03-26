// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.MRSharingWithUnetSamples.Scripts
{
    public abstract class DeviceControllerBase : MonoBehaviour
    {
        public GameObject PlayerObject;

        [HideInInspector] public GameObject PlayerInstance;

        private GameObject _playerObject;

        private NetworkBehaviour _playerNetworkBehaviour;

        protected NetworkBehaviour GetPlayerNetworkBehaviour()
        {
            return _playerNetworkBehaviour;
        }

        public void SetNetworkBehaviour(NetworkBehaviour behaviour)
        {
            _playerNetworkBehaviour = behaviour;
        }

        public void RemovePlayerObject()
        {
            if (_playerObject != null) DestroyImmediate(_playerObject);
        }


        public abstract void LocalPlayerUpdate();

        public abstract void OnStartLocalPlayer();

        public abstract void InitializeLocalPlayer();

        public abstract void InitializeRemotePlayer();

        public abstract MixedRealityCameraManager.DisplayType SupportDisplayType { get; }

        protected void CreatePlayerObject(Color playerColor)
        {
            _playerObject = Instantiate(PlayerObject);
            _playerObject.transform.position = Vector3.zero;
            _playerObject.transform.SetParent(PlayerInstance.transform);
            _playerObject.transform.localPosition = Vector3.zero;
            var material = new Material(Shader.Find("Diffuse"));
            material.color = playerColor;
            var componentInChildren = _playerObject.GetComponent<MeshRenderer>();
            componentInChildren.material = material;
        }
    }
}