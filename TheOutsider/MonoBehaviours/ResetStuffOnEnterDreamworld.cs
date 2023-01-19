using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class ResetStuffOnEnterDreamworld : MonoBehaviour
    {
        bool initialized = false;

        void Awake()
        {
            GlobalMessenger.AddListener(OWConstants.EnterDreamWorld, DreamworldEnter);
            GlobalMessenger.AddListener(OWConstants.ExitDreamWorld, DreamworldExit);
            enabled = false;
        }
        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(OWConstants.EnterDreamWorld, DreamworldEnter);
            GlobalMessenger.RemoveListener(OWConstants.ExitDreamWorld, DreamworldExit);
        }
        void DreamworldEnter()
        {
            enabled = true;

            var audio = GetComponent<OWAudioSource>();
            audio.time = 0f;
            audio.Stop();

            Locator.GetGlobalMusicController()._travelSource.Stop();

            //---------------- Initialize Friend Text properly ----------------//
            if (!initialized)
            {
                initialized = true;
                FriendConversationFixes.Instance.InitializeOnEnter();

                var conversations = FindObjectsOfType<NomaiConversationManager>();
                foreach (var c in conversations)
                {
                    if (c.gameObject.name == "FriendConversation") c.InitializeNomaiText();
                }
            }
        }
        void DreamworldExit()
        {
            enabled = false;
        }

        //Quick Fix: right arm not held awkwardly.
        public static Transform Shoulder { get; set; }
        public static Transform Elbow { get; set; }
        public static Transform Neck { get; set; }
        public static Transform Jaw { get; set; }
        Quaternion previousRot;
        void LateUpdate()
        {
            if (Shoulder == null) return;

            Shoulder.localRotation = Quaternion.Euler(0f, 0f, 33f) * Shoulder.localRotation;
            //Elbow.localRotation = Quaternion.Euler(0f, 0f, 0f) * Elbow.localRotation;
            Jaw.localRotation = Quaternion.Euler(0f, 0f, -230f);

            //Also: add thing on ground so can't see leg weirdness?

            if (FriendConversationFixes.Instance == null || FriendConversationFixes.Instance.ConversationManager == null) return;

            var convo = FriendConversationFixes.Instance.ConversationManager;
            var state = convo._state;
            //var controller = convo._solanumAnimController;

            Quaternion targetRot = Neck.rotation; //Default target to animation.

            if (state == NomaiConversationManager.State.WatchingPlayer)
            {
                var playerPos = Locator.GetPlayerBody().GetPosition();
                Vector3 dir = (playerPos - Neck.position).normalized;

                var tf = FriendConversationFixes.Instance.transform;
                var dot = Vector3.Dot(tf.forward, dir);
                dot = Mathf.Clamp01(dot);

                targetRot = Quaternion.LookRotation(dir, Vector3.up);
                var e = targetRot.eulerAngles;
                //(x = 0) = forward | + = down
                //(y = 90) = forward | 0 = left
                targetRot = Quaternion.Euler(0f, 180f - 90f + e.y, 280f + e.x);

                targetRot = Quaternion.Slerp(Neck.rotation, targetRot, dot); //Look at if in front.
            }

            Neck.rotation = Quaternion.Slerp(previousRot, targetRot, Time.deltaTime * 7f);

            previousRot = Neck.rotation;
        }
    }
}