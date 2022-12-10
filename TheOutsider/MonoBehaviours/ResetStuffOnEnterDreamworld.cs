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
        }
        void OnDestroy()
        {
            GlobalMessenger.RemoveListener(OWConstants.EnterDreamWorld, DreamworldEnter);
        }
        void DreamworldEnter()
        {
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
    }
}