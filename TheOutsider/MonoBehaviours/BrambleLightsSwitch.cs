using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheOutsider.OutsiderHandling;

namespace TheOutsider.MonoBehaviours
{
    public sealed class BrambleLightsSwitch : MonoBehaviour
    {
        NomaiInterfaceSlot powerSlot;
        SectorLightsCullGroup cullGroup;

        TractorBeamController[] tractorBeams;
        SectorVolumeOcclusionGroup[] occlusionGroups;
        List<MeshRenderer> lightRenderers = new List<MeshRenderer>();
        Dictionary<Light, Float> originalIntensities = new Dictionary<Light, Float>();
        class Float { public float Value; }

        void Awake()
        {
            powerSlot = GetComponent<NomaiPowerSwitch>()._slot;
            powerSlot.OnSlotActivated += OnActivate;
            powerSlot.OnSlotDeactivated += OnDeactivate;

            offset = Random.Range(-0.08f, 0.08f);
        }
        void OnDestroy()
        {
            powerSlot.OnSlotActivated -= OnActivate;
            powerSlot.OnSlotDeactivated -= OnDeactivate;
        }

        void Start()
        {
            GetCullGroup();
            SetOcclusionStrength();   //Ensure initialized properly
        }
        void GetCullGroup()
        {
            var targetName = GetTargetName();

            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj != null)
            {
                if (targetObj.TryGetComponent(out cullGroup))
                {
                    foreach (var item in cullGroup._staticLights) {
                        originalIntensities.Add(item.light, new Float() { Value = item.originalIntensity });
                    }
                }
                else Log.Error($"Cull group not found for {targetObj.name} for lights.");
            }
            else Log.Error($"Obj not found: {name} -> {targetName}");

            var renderers = targetObj.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers) {
                if (r.name.Contains("Wall_Lantern")) lightRenderers.Add(r); //Not all lights are under wall lantern. (Doubles)
            }

            occlusionGroups = targetObj.GetComponentsInChildren<SectorVolumeOcclusionGroup>();
            tractorBeams = targetObj.GetComponentsInChildren<TractorBeamController>();

            enabled = false;
        }
        string GetTargetName()
        {
            switch (name)
            {
                case OutsiderLightSwitch.Observatory: return OutsiderSector.Observatory;
                case OutsiderLightSwitch.BrambleDimension: return OutsiderSector.BrambleDimension;

                case OutsiderLightSwitch.SouthPole:
                    secretDoor = GameObject.Find(OutsiderPath.SecretRoomDoor).transform;
                    secretDoorLocalStart = secretDoor.localPosition;
                    return OutsiderSector.SouthPole;

                case OutsiderLightSwitch.HuntingBlind:
                    return OutsiderSector.HuntingBlindCloser;

                case OutsiderLightSwitch.ShuttleCrusher:
                    shuttleCrusher = FindObjectOfType<ShuttleCrusher>();
                    return OutsiderSector.ShuttleCrusher;

                case OutsiderLightSwitch.PowerStation: return OutsiderSector.PowerStation;
            }

            return "ERROR";
        }

        //--------------------------------------------- Set Cull Group Active ---------------------------------------------//
        float t = 1f;
        float offset;
        bool fadeIn;
        void OnActivate(NomaiInterfaceSlot slot)
        {
            SetActive(false);   //OnSlot is actually off slot.
            Locator.GetShipLogManager().RevealFact("PS_POWER_STATION_X3");
        }
        void OnDeactivate(NomaiInterfaceSlot slot)
        {
            SetActive(true);
        }
        void SetActive(bool active)
        {
            fadeIn = active;
            enabled = true;
        }

        //--------------------------------------------- Fade Cull Group ---------------------------------------------//
        Transform secretDoor;
        ShuttleCrusher shuttleCrusher;
        Vector3 secretDoorLocalStart;
        bool ContinueLoop(bool fadeIn, float target) //Fade in and not at 1, fade out and above 0
        {
            return (fadeIn && t < target) || (!fadeIn && t >= target);
        }
        void Update()
        {
            float target = fadeIn ? 1f : 0f;

            if (!ContinueLoop(fadeIn, target))
            {
                t = target;
                SetIntensities();
                enabled = false;
                return;
            }

            float duration = 1f;
            float dt = Time.deltaTime / duration;
            t += fadeIn ? dt : -dt;
            SetIntensities();
        }
        void SetIntensities()
        {
            for (int i = 0; i < cullGroup._staticLights.Count; i++)
            {
                var light = cullGroup._staticLights[i].light;
                if (light == null) continue;

                //Only turn off structure lights.
                if (light.name != "Light_NOM_GravityCrystal" || light.name.Contains("Campfire") || light.name.Contains("NONPOWER"))
                {
                    float newIntensity = Mathf.Lerp(0f, originalIntensities[light].Value, t);
                    light.intensity = newIntensity;

                    var v = cullGroup._staticLights[i];
                    v.originalIntensity = newIntensity;
                    cullGroup._staticLights[i] = v;
                }
            }

            SetOcclusionStrength();

            if (t < 0.5f + offset) SetLightMats(OutsiderMaterials.LightsOff);
            else if (t < 0.6f + offset) SetLightMats(OutsiderMaterials.LightsOn);
            else if (t < 0.7f + offset) SetLightMats(OutsiderMaterials.LightsOff);
            else if (t < 0.8f + offset) SetLightMats(OutsiderMaterials.LightsOn);
            else if (t < 0.9f + offset) SetLightMats(OutsiderMaterials.LightsOff);
            else SetLightMats(OutsiderMaterials.LightsOn);

            if (secretDoor != null)
            {
                Vector3 localTarget = secretDoorLocalStart + Vector3.left * 2f;
                secretDoor.localPosition = Vector3.Lerp(localTarget, secretDoorLocalStart, t);
            }
            if (shuttleCrusher != null)
            {
                if (t < 0.1f) shuttleCrusher.HasPower = false;
                else if (t > 0.9f) shuttleCrusher.HasPower = true;
            }

            foreach (var beam in tractorBeams)
            {
                beam.SetActivation(t > 0.98f, true);
            }
        }

        void SetOcclusionStrength()
        {
            float strength = Mathf.Lerp(1f, 0.5f, t); //On: t = 1, so reduce occlusion strength.

            foreach (var group in occlusionGroups)
            {
                for (int i = 0; i < group._occlusionVolumes.Count; i++)
                {
                    var o = group._occlusionVolumes[i];
                    o.originalStrength = strength;
                    o.occlusionVolume.occlusionStrength = strength;
                    group._occlusionVolumes[i] = o;
                }
            }
        }

        void SetLightMats(Material mat)
        {
            foreach (var r in lightRenderers)
            {
                var mats = r.sharedMaterials;
                mats[1] = mat;
                r.sharedMaterials = mats;
            }
        }
    }
}