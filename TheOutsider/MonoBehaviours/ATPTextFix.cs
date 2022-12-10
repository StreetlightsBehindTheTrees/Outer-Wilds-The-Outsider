using System;
using System.Collections;
using System.Collections.Generic;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class ATPTextFix : MonoBehaviour  //UNUSED
    {
        void Awake()
        {
            GlobalMessenger<OWRigidbody>.AddListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
            GlobalMessenger<OWRigidbody>.AddListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
        }
        void OnDestroy()
        {
            GlobalMessenger<OWRigidbody>.RemoveListener("EnterTimeLoopCentral", OnEnterTimeLoopCentral);
            GlobalMessenger<OWRigidbody>.RemoveListener("ExitTimeLoopCentral", OnExitTimeLoopCentral);
        }

        void OnEnterTimeLoopCentral(OWRigidbody arg1)
        {
            var arcs = GetComponentsInChildren<OWRenderer>(true);
            for (int i = 0; i < arcs.Length; i++)
            {
            }
        }
        void OnExitTimeLoopCentral(OWRigidbody arg1)
        {
            var arcs = GetComponentsInChildren<NomaiTextLine>();

        }
    }
}
