// Copyright(c) 2018 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Assets.MRSharingWithUnetSamples.Scripts;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Assets.UNETTestSamples.Scripts
{
    [RequireComponent(typeof(SamplePlayerController))]
    public class HoloLensController : DeviceControllerBase, IInputClickHandler
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
            get { return MixedRealityCameraManager.DisplayType.Transparent; }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (PlayerNetworkBehaviour.isLocalPlayer) PlayerNetworkBehaviour.CmdFire();
        }
    }
}