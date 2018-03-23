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

        private NetworkBehaviour _playerNetworkBehaviour;

        protected NetworkBehaviour GetPlayerNetworkBehaviour()
        {
            return _playerNetworkBehaviour;
        }

        public void SetNetworkBehaviour(NetworkBehaviour behaviour)
        {
            _playerNetworkBehaviour = behaviour;
        }

        public abstract void LocalPlayerUpdate();

        public abstract void OnStartLocalPlayer();

        public abstract void InitializeLocalPlayer();

        public abstract void InitializeRemotePlayer();

        public abstract MixedRealityCameraManager.DisplayType SupportDisplayType { get; }

        protected void CreatePlayerObject(Color playerColor)
        {
            var instantiate = Instantiate(PlayerObject);
            instantiate.transform.position = Vector3.zero;
            instantiate.transform.SetParent(PlayerInstance.transform);
            instantiate.transform.localPosition = Vector3.zero;
            var material = new Material(Shader.Find("Diffuse"));
            material.color = playerColor;
            var componentInChildren = instantiate.GetComponent<MeshRenderer>();
            componentInChildren.material = material;
        }
    }
}