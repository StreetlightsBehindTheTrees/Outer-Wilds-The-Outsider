using System.Collections;
using System.Collections.Generic;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    /// <summary> Class for all objects to be pulled of the surface of Dark Bramble. </summary>
    public sealed class DarkBrambleDetacher : GiantsDeepPull
    {
        bool breakAwayCheck = false;    //Might not be needed, but just in case.
        float breakAwayTime;
        float RandomBetween(float a, float b) => Mathf.Lerp(a, b, Random.value);
        void Update()
        {
            if (breakAwayCheck && Time.time > breakAwayTime)
            {
                breakAwayCheck = false;
                BeginAttractionToGiantsDeep();
                CreateAndPlayAudioSource();
                FadeOutSurfaceGravityFields();
                //owRB.AddVelocityChange(transform.up * 3f);
            }
        }
        //public void BreakAway() => StartCoroutine(BreakAwayinator());
        public void BreakAway()
        {
            breakAwayCheck = true;
            float waitTime = RandomBetween(0f, 3f);
            breakAwayTime = Time.time + waitTime;

            //yield return new WaitForSecondsRealtime(waitTime);  //Not working????
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
        void FadeOutSurfaceGravityFields() //Turn off gravity crystals, etc.
        {
            var dirFields = GetComponentsInChildren<DirectionalForceVolume>();
            float t = 0f;
            float fadeOutTime = 2f;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime * 0.1f;
                foreach (var field in dirFields) field._fieldMagnitude = Mathf.Lerp(field._fieldMagnitude, 0f, t / fadeOutTime);
            }

            foreach (var field in dirFields) Destroy(field.gameObject);
        }
    }
}