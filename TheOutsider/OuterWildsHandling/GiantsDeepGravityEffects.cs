using System.Collections;
using System.Collections.Generic;
using OWML.Common;
using TheOutsider.MonoBehaviours;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.OuterWildsHandling
{
    public sealed class GiantsDeepGravityEffects
    {
        Transform darkBramble;
        Transform giantsDeep;

        Transform effectParent;
        OWAudioSource[] otherRumbleSources;

        SphereShape dbSectorShape;
        SectorStreaming streaming;

        TextAsset gabbroNewDialogue;

        // Giant's Deeps (default) gravity well has radius of 4000.
        //Closest centers gets is 3543. 30s before closest at 3800.
        const float startCreaking = 5000f;
        const float intenseCreaking = 4250f;
        const float startBreakingAway = 3870f;
        const string soundObjName = "SOUND_CREAK_";
        public const string GravityEffects = "GravityEffects";

        float[] distances = new float[3] { startCreaking, intenseCreaking, startBreakingAway };
        int distIndex;

        public GiantsDeepGravityEffects(GameObject brambleRoot, GameObject dimensionRoot, GameObject dreamRoot, OWObjects objects, AssetBundle bundle)
        {
            darkBramble = Locator._darkBramble.transform;
            giantsDeep = Locator._giantsDeep.transform;

            dbSectorShape = objects.DarkBrambleSector.GetComponent<SphereShape>();
            streaming = objects.DarkBrambleSector.transform.Find("Sector_Streaming").GetComponent<SectorStreaming>();
            gabbroNewDialogue = bundle.LoadAsset<TextAsset>("Gabbro_NEW_LoopEnd.bytes");

            distIndex = 0;
            effectParent = brambleRoot.transform.Find(GravityEffects);

            for (int i = 0; i < 3; i++)
            {
                var obj = effectParent.Find($"{soundObjName}{distIndex}");
                if (Log.ErrorIf(obj == null, $"Sound Obj {distIndex} missing")) continue;
                var owAS = obj.GetComponent<OWAudioSource>();
                if (Log.ErrorIf(owAS == null, $"Audio Source {distIndex} missing")) continue;
            }

            otherRumbleSources = new OWAudioSource[]
            {
                dimensionRoot.transform.Find(GravityEffects).GetComponentInChildren<OWAudioSource>(),
                dreamRoot.transform.Find(GravityEffects).GetComponentInChildren<OWAudioSource>(),
            };

            OWPatches.DreamAudioSource = otherRumbleSources[1];
        }
        public void OnUpdate()
        {
            if (distIndex >= 3) return;

            float dist = (giantsDeep.position - darkBramble.position).magnitude;
            if (dist >= distances[distIndex]) return;   //Wait until dist to GD is less than threshold dist.

            var audioSource = effectParent.Find($"{soundObjName}{distIndex}").GetComponent<OWAudioSource>();
            audioSource.clip = OutsiderAudio.GetCreakSound(distIndex);
            audioSource.Play();

            Log.Print($"Giant's Deep Distance Index: {distIndex}/2");
            PrintStuff();

            if (distIndex == 0) ExpandDarkBrambleSector();
            if (distIndex == 1)
            {
                audioSource.StartCoroutine(StopPlayerFromStickingToGround());
                audioSource.StartCoroutine(DreamGravityinator());
                foreach (var source in otherRumbleSources)
                {
                    source.clip = OutsiderAudio.insideCreaking;
                    source.Play();
                }

                OWObjects.GabbroDialogue.SetTextXml(gabbroNewDialogue);
                OWPatches.DontKillByImpact = true;
            }
            if (distIndex == 2) StartBreakingAway();

            distIndex++;
        }
        void ExpandDarkBrambleSector() //Expand so covers Giant's Deep as well to keep things loaded.
        {
            float largeSize = 10000f;
            dbSectorShape.radius = largeSize;
            streaming._softLoadRadius = largeSize;

            var sectorTriggers = dbSectorShape.GetComponentsInChildren<SphereProximityTrigger>();
            foreach (var sector in sectorTriggers)
            {
                string name = sector.name;
                if (name.Contains("OutsiderDB") || name.Contains("ShuttleCrusher") || name.Contains("StudyTower")) {
                    sector.radius = largeSize;
                }
            }
        }
        void StartBreakingAway()
        {
            foreach (var item in GiantsDeepPull.list)
            {
                if (item is DarkBrambleDetacher surfaceDetachers) surfaceDetachers.BreakAway(); //All but Power Station.
                else item.BeginAttractionToGiantsDeep();
            }
        }
        IEnumerator StopPlayerFromStickingToGround()
        {
            float dist = 0f;
            while (dist < 1200f) //Make not stick to ground when being pulled upwards.
            {
                dist = (Locator.GetPlayerTransform().position - Locator._darkBramble.transform.position).magnitude;
                float gravity = Locator.GetPlayerController().GetNormalAccelerationScalar();

                if (gravity > 0f || PlayerState.IsInsideShip()) OWPatches.AllowPlayerGrounded = true;
                else
                {
                    var volume = Locator.GetPlayerForceDetector()._inheritedVolume;
                    if (volume != null && volume.name.Contains("GravityCrystal"))
                    {
                        OWPatches.AllowPlayerGrounded = true;
                    }
                    else OWPatches.AllowPlayerGrounded = false;
                }

                yield return null;
            }

            OWPatches.AllowPlayerGrounded = true;
        }
        IEnumerator DreamGravityinator()
        {
            var gravity = GameObject.Find("Sector_DreamWorld/Volumes_DreamWorld/DreamWorldVolume").GetComponent<DirectionalForceVolume>();
            float oldValue = gravity._fieldMagnitude;
            float newValue = 0.01f;

            float time = 0f;
            while (time < 20f)  //Gravity Fade Out
            {
                gravity._fieldMagnitude = Mathf.Lerp(oldValue, newValue, time / 20f);
                time += Time.deltaTime;
                yield return null;
            }
            gravity._affectsAlignment = false;
            time = 0f;
            while (time < 30f)  //Zero G
            {
                time += Time.deltaTime;
                yield return null;
            }
            gravity._affectsAlignment = true;
            time = 0f;
            while (time < 30f) //Gravity Fade In
            {
                gravity._fieldMagnitude = Mathf.Lerp(newValue, oldValue, time / 20f);
                time += Time.deltaTime;
                yield return null;
            }
        }

        void PrintStuff()
        {
            if (DebugStuff.DebugControls.SkippedToGiantsDeepPull) return;   //Only print if natural.
            var sun = Locator._sunTransform;

            //Rotation shouldn't matter as Dark Bramble doesn't rotate, and Giant's Deep is mostly water.
            void PrintInfo(string name, OWRigidbody rb)
            {
                var relativePosition = rb.GetPosition() - sun.position;
                var relativeVelocity = rb.GetVelocity();
                Log.Print($"{name}: {relativePosition.ToString("F3")} | {relativeVelocity.ToString("F3")}");
            }
            PrintInfo("Dark Bramble", darkBramble.GetAttachedOWRigidbody());
            PrintInfo("Giant's Deep", giantsDeep.GetAttachedOWRigidbody());
            PrintInfo("Power Station", GameObject.Find("PowerStation").GetComponent<OWRigidbody>());

            Log.Print($"Time left: {(TimeLoop._loopDuration - Time.timeSinceLevelLoad - TimeLoop._timeOffset).ToString("F3")}");
        }
    }
}