// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Assets.MRSharingWithUnetSamples.Scripts;
using Assets.UNETTestSamples.Scripts;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Assets.MRSharingWithUnetSamples.Scripts
{
    [RequireComponent(typeof(SamplePlayerController))]
    public class ImmersiveHMDController : DeviceControllerBase, IInputClickHandler
    {
        private MixedRealityCameraManager.DisplayType _supportDisplayType;

        private SamplePlayerController PlayerNetworkBehaviour
        {
            get { return (SamplePlayerController) GetPlayerNetworkBehaviour(); }
        }

        public override void LocalPlayerUpdate()
        {
            transform.position = CameraCache.Main.transform.position;
            transform.rotation = CameraCache.Main.transform.rotation;
        }

        public override void OnStartLocalPlayer()
        {
            PlayerInstance.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        }

        public override void InitializeLocalPlayer()
        {
            CreatePlayerObject(Color.blue);
        }

        public override void InitializeRemotePlayer()
        {
            CreatePlayerObject(Color.red);
        }

        public override MixedRealityCameraManager.DisplayType SupportDisplayType
        {
            get { return MixedRealityCameraManager.DisplayType.Opaque; }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (PlayerNetworkBehaviour.isLocalPlayer) PlayerNetworkBehaviour.CmdFire();
        }
    }
}