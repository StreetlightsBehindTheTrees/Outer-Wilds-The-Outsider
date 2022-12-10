using System.Collections;
using System.Collections.Generic;
using TheOutsider.OuterWildsHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    /// <summary> Base class for all objects to be pulled by Giant's Deep. </summary>
    public class GiantsDeepPull : MonoBehaviour
    {
        public static HashSet<GiantsDeepPull> list { get; set; } = new HashSet<GiantsDeepPull>();

        protected OWRigidbody owRB;
        protected OWRigidbody followingRB;
        bool doFollowing = false;

        void Awake() => list.Add(this);
        void OnDestroy() => list.Remove(this);
        OWRigidbody CreateRB(GameObject obj, bool kinematic)
        {
            obj.SetActive(false);

            var rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            if (kinematic) rb.isKinematic = true;
            rb.mass = GetMass();

            OWRigidbody owrb = obj.AddComponent<OWRigidbody>();
            owrb._isTargetable = false;
            if (kinematic) owrb._kinematicSimulation = true;
            owrb._autoGenerateCenterOfMass = true;
            owrb._maintainOriginalCenterOfMass = true;

            obj.SetActive(true);

            return owrb;
        }
        float GetMass()
        {
            string name = gameObject.name;
            if (name.Contains("Projection_Pool_House_Platform")) return 100f;
            if (name.Contains("UFO_Destroyed")) return 5000f;
            if (name.Contains("Shuttle")) return 5000f;

            return 100000f;
        }
        public void BeginAttractionToGiantsDeep()
        {
            enabled = true;
            transform.parent = null;    //Must be at base level or else Dark Bramble itself gets effected.
            
            //---------------- Find and Set Kinematic colliders ----------------//
            GameObject followingRBObj = new GameObject($"{name}FollowedRB");
            var kinematicColliders = transform.Find("KinematicColliders");
            if (kinematicColliders == null) Log.Error($"Kinematic colliders not found for {name}!");
            else
            {
                kinematicColliders.transform.parent = followingRBObj.transform;
                kinematicColliders.transform.localPosition = Vector3.zero;
                kinematicColliders.transform.localRotation = Quaternion.identity;
            }

            //---------------- Set up Rigidbodies ----------------//
            followingRB = CreateRB(followingRBObj, false);
            if (!gameObject.TryGetComponent(out owRB))  //Get or create rb for object so player physics, etc. still work correctly.
            {
                //---------------- Set Up Physics ----------------//
                owRB = CreateRB(gameObject, true);
                var velocity = OWObjects.DarkBrambleGravity.GetAttachedOWRigidbody().GetVelocity();
                owRB.SetVelocity(velocity); //Make same velocity as DB. (132.6, 0, -49.3)
            }
            CopyRB(followingRB, owRB);  //Initialize followingRB at this rb.
            owRB._isTargetable = true;

            //---------------- Set Up Force Detector ----------------//
            GameObject detectorObj = new GameObject("Detector");
            detectorObj.transform.parent = followingRB.transform;
            detectorObj.transform.localPosition = Vector3.zero;
            detectorObj.transform.localRotation = Quaternion.identity;
            detectorObj.layer = 20; //Basic Detector
            detectorObj.SetActive(false);
            ConstantForceDetector forceDetector = detectorObj.AddComponent<ConstantForceDetector>();
            forceDetector._fieldMultiplier = 1f;
            forceDetector._inheritElement0 = true;
            detectorObj.SetActive(true);

            //As following RB, always add both gravities, despite Power Station already attracted to DB.
            forceDetector.AddConstantVolume(OWObjects.DarkBrambleGravity, false);
            forceDetector.AddConstantVolume(OWObjects.GiantsDeepGravity);

            //---------------- Add Fluid Detector ----------------//
            detectorObj.SetActive(false);
            var fluidDetector = detectorObj.AddComponent<DynamicFluidDetector>();
            var b = fluidDetector._buoyancy;
            b.density = 0.7f;
            b.boundingRadius = 50f;
            b.dragCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            b.submergeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            b.checkAgainstWaves = false;

            fluidDetector._dontApplyForces = false;
            fluidDetector._dragFactor = 1f;
            fluidDetector._angularDragFactor = 1f;
            detectorObj.SetActive(true);
            
            fluidDetector.AddVolume(OWObjects.GiantsDeepOcean);
            fluidDetector.AddVolume(OWObjects.GiantsDeepAtmosphere);
            fluidDetector.AddVolume(OWObjects.GiantsDeepClouds);

            doFollowing = true;
        }

        void FixedUpdate()
        {
            if (doFollowing) CopyRBPhysics(owRB, followingRB);
        }
        void CopyRB(OWRigidbody rb, OWRigidbody copyFrom)
        {
            rb.SetPosition(copyFrom.GetPosition()); //Puts at, but doesn't "Move To"
            rb.SetRotation(copyFrom.GetRotation());
            rb.SetVelocity(copyFrom.GetVelocity());
            rb.SetAngularVelocity(copyFrom.GetAngularVelocity());
        }
        void CopyRBPhysics(OWRigidbody rb, OWRigidbody copyFrom)
        {
            rb.MoveToPosition(copyFrom.GetPosition());  //Move to with physics
            rb.MoveToRotation(copyFrom.GetRotation());
            rb.SetVelocity(copyFrom.GetVelocity());
            rb.SetAngularVelocity(copyFrom.GetAngularVelocity());
        }
    }
}