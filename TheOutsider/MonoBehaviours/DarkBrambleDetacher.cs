using System.Collections;
using System.Collections.Generic;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    /// <summary> Class for all objects to be pulled of the surface of Dark Bramble. </summary>
    public sealed class DarkBrambleDetacher : GiantsDeepPull
    {
        DirectionalForceVolume[] fieldsToFadeOut;
        float breakAwayTime;
        
        bool brokenAway = false;
        float t = 0f;

        protected override void Awake()
        {
            base.Awake();
            fieldsToFadeOut = GetComponentsInChildren<DirectionalForceVolume>();
        }
        float RandomBetween(float a, float b) => Mathf.Lerp(a, b, Random.value);
        public void BreakAway()
        {
            enabled = true;
            float waitTime = RandomBetween(0f, 3f);
            breakAwayTime = Time.time + waitTime;
        }
        void Update()
        {
            if (Time.time > breakAwayTime)
            {
                if (!brokenAway)
                {
                    brokenAway = true;
                    BeginAttractionToGiantsDeep();
                    CreateAndPlayAudioSource();
                    
                    ManageVolumes();
                }

                FadeOutSurfaceGravityFields();
            }
        }
        void ManageVolumes()
        {
            if (gameObject.name == OutsiderSector.StudyTowerRoot) //Only do for Study Tower when breaks away.
            {
                var hack = Vector3.Distance(transform.position, Locator.GetPlayerBody().GetPosition()); //Gets it done.
                if (hack < 60f) Locator.GetPlayerForceDetector()._activeInheritedDetector = forceDetector;
            }
        }
        void CreateAndPlayAudioSource()
        {
            OWAudioSource audioSource = gameObject.AddComponent<OWAudioSource>();
            audioSource.SetTrack(OWAudioMixer.TrackName.Environment);
            audioSource.SetLocalVolume(0.7f);
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 70f;
            audioSource.maxDistance = 450f;
            audioSource.pitch = RandomBetween(0.9f, 1.1f);
            var clip = OutsiderAudio.GetRandomDetatchSound();
            audioSource.clip = clip;
            audioSource._audioSource.Play();
        }
        void FadeOutSurfaceGravityFields() //Turn down gravity crystals
        {
            float fadeOutTime = 4f;
            float targetLowGravity = 3f;

            t += Time.deltaTime;
            foreach (var field in fieldsToFadeOut)
            {
                field._fieldMagnitude = Mathf.Lerp(field._fieldMagnitude, targetLowGravity, t / fadeOutTime);
            }
        }
    }
}