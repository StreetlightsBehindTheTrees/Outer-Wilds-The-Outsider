using OWML.Common;
using System.Collections.Generic;
using TheOutsider.MonoBehaviours;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheOutsider.DebugStuff
{
    public sealed class DebugControls
    {
        private IModHelper modHelper;
        public DebugControls(IModHelper modHelper)
        {
            this.modHelper = modHelper;
        }

        public static bool SkippedToGiantsDeepPull { get; private set; }

        bool GetKeyDown(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        bool GetKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }
        public void OnUpdate()
        {
            if (GetKeyDown(Key.T)) TeleportToDarkBramble();
            if (GetKeyDown(Key.Y)) TeleportToVesselWithAdvancedWarpCore();
            if (GetKeyDown(Key.B)) SkipToGiantsDeepGravityPull();
            if (GetKeyDown(Key.L)) GivePlayerLantern();
            if (GetKeyDown(Key.K)) ExplodeShip();
            //if (GetKeyDown(Key.P)) Crush();

            if (GetKeyDown(Key.O)) ToggleSecretDoor();

            if (GetKeyDown(Key.Comma)) Rewind60Seconds();
            if (GetKeyDown(Key.Period)) SkipForward60Seconds();

            if (Time.timeScale > 0.5f)
            {
                if (GetKeyDown(Key.Equals)) Time.timeScale = 20f;
                if (GetKeyUp(Key.Equals)) Time.timeScale = 1f;
            }
            if (GetKeyDown(Key.F1))
            {
                GUIMode.SetRenderMode(GUIMode.IsHiddenMode() ? GUIMode.RenderMode.FPS : GUIMode.RenderMode.Hidden);
            }
            if (GetKeyDown(Key.F2))
            {
                var suit = Locator.GetPlayerSuit();
                if (suit.IsWearingSuit()) suit.RemoveSuit();
                else suit.SuitUp();
            }
            if (GetKeyDown(Key.F4))
            {
                var resources = GameObject.FindObjectOfType<PlayerResources>();
                resources._currentFuel = PlayerResources._maxFuel;
                resources._currentOxygen = PlayerResources._maxOxygen;
                resources._currentHealth = PlayerResources._maxHealth;
            }

            if (GetKeyDown(Key.I)) RunDebugEndCutscene();
        }
        void Rewind60Seconds()
        {
            TimeLoop.SetSecondsRemaining(TimeLoop.GetSecondsRemaining() + 60f);
            Log.Print($"Rewind 60s to {TimeLoop.GetMinutesElapsed()} minutes in.");
        }
        void SkipForward60Seconds()
        {
            TimeLoop.SetSecondsRemaining(TimeLoop.GetSecondsRemaining() - 60f);
            Log.Print($"Skipped 60s to {TimeLoop.GetMinutesElapsed()} minutes in.");
        }

        void ToggleSecretDoor()
        {
            var obj = GameObject.Find($"{OutsiderSector.FriendsHouse}/SecretRoomDoor");
            obj.SetActive(!obj.activeSelf);
        }

        //--------------------------------------------- Helper Functions ---------------------------------------------//
        void TeleportTo(OWRigidbody rb, Vector3 relativePosition = default, bool teleportShip = true)
        {
            var player = Locator._playerBody;
            var ship = Locator._shipBody;
            Vector3 pos = rb.GetPosition();
            Vector3 targetPos = pos + relativePosition;

            Quaternion rot = Quaternion.LookRotation(pos - targetPos);
            player.WarpToPositionRotation(targetPos, rot);
            if (teleportShip) ship.WarpToPositionRotation(targetPos, rot);

            var targetVelocity = rb.GetPointVelocity(targetPos);
            player.SetVelocity(targetVelocity);
            if (teleportShip) ship.SetVelocity(targetVelocity);
        }
        delegate bool ItemType<T>(T t);
        void GivePlayerItem<T>(ItemType<T> itemType) where T : OWItem
        {
            var items = Object.FindObjectsOfType<T>();
            foreach (T item in items)
            {
                if (itemType(item)) { GivePlayerItem(item); return; }
            }
            Log.Warning($"Could not find OWItem: {typeof(T)}.");
        }
        void GivePlayerItem(OWItem item) => Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(item);

        //--------------------------------------------- Debug Functions ---------------------------------------------//
        void TeleportToDarkBramble()
        {
            TeleportTo(Locator._darkBramble.GetOWRigidbody(), new Vector3(0f, 0f, -1100f));
        }
        void TeleportToVesselWithAdvancedWarpCore()
        {
            var vesselRB = GameObject.Find("DB_VesselDimension_Body").GetComponent<OWRigidbody>();
            TeleportTo(vesselRB, new Vector3(330f, -100f, 170f));
            
            GivePlayerItem<WarpCoreItem>(x => x.GetWarpCoreType() == WarpCoreType.Vessel);
        }
        void GivePlayerLantern()
        {
            GivePlayerItem<DreamLanternItem>(x => x.GetLanternType() == DreamLanternType.Functioning);
            //TeleportTo(Locator._darkBramble.GetOWRigidbody(), new Vector3(93.3f, -596.4f, -11.5f), false);
        }

        void SkipToGiantsDeepGravityPull()
        {
            SkippedToGiantsDeepPull = true;

            var sun = Locator._sunTransform;
            var dbRB = Locator._darkBramble.GetOWRigidbody();
            var gdRB = Locator._giantsDeep.GetOWRigidbody();
            var psRB = GameObject.Find("PowerStation").GetComponent<OWRigidbody>(); //Also will need to find out power station relative info.
            
            var dbRot = dbRB.transform.rotation;
            var gdRot = gdRB.transform.rotation;
            var psRot = psRB.GetRotation();
            
            var ship = Locator._shipBody;
            var relativePos = ship.GetPosition() - dbRB.GetPosition();
            var relativeVel = ship.GetVelocity() - dbRB.GetVelocity();
            var shipRot = ship.GetRotation();

            //---------------- Keep Islands ----------------//
            var gdIslands = new GameObject[]
            {
                GameObject.Find("QuantumIsland_Body"),
                GameObject.Find("ConstructionYardIsland_Body"),
                GameObject.Find("StatueIsland_Body"),
                GameObject.Find("GabbroIsland_Body"),
                GameObject.Find("BrambleIsland_Body"),
                
                GameObject.Find("GabbroShip_Body")
            };
            List<IslandInfo> islandInfos = new List<IslandInfo>();
            foreach (var island in gdIslands) islandInfos.Add(new IslandInfo(island, gdRB));

            //---------------- Move Objects ----------------//
            void MoveObject(OWRigidbody rb, Vector3 offset, Quaternion rot, Vector3 velocity)
            {
                rb.WarpToPositionRotation(sun.position + offset, rot); //Relative to sun
                rb.SetVelocity(velocity);
            }

            TimeLoop.SetSecondsRemaining(107.723f);
            MoveObject(dbRB, new Vector3(-12551.33f, 0f, -15572.36f), dbRot, new Vector3(110.092f, 0f, -88.759f));
            MoveObject(gdRB, new Vector3(-12610.29f, 0f, -10572.88f), gdRot, new Vector3(100.14f, 0f, -119.501f));
            MoveObject(psRB, new Vector3(-13468.17f, 0f, -16346.44f), psRot, new Vector3(73.244f, 0f, -45.255f));

            foreach (var island in islandInfos) island.ReapplyInfo(gdRB);   //Move islands with GD.

            //---------------- Teleport Ship ----------------//
            ship.WarpToPositionRotation(dbRB.GetPosition() + relativePos, shipRot);
            ship.SetVelocity(dbRB.GetVelocity() + relativeVel);

            //---------------- Telport Player to ship ----------------//
            var player = Locator._playerBody;
            player.WarpToPositionRotation(ship.GetPosition(), shipRot);
            player.SetVelocity(ship.GetVelocity());
        }
        public class IslandInfo
        {
            public OWRigidbody rb;
            public Vector3 relativePos;
            public Quaternion relativeRot;
            public Vector3 relativeVel;

            public IslandInfo(GameObject island, OWRigidbody gdRB)
            {
                rb = island.GetComponent<OWRigidbody>();
                relativePos = rb.GetPosition() - gdRB.GetPosition();
                relativeVel = rb.GetVelocity() - gdRB.GetVelocity();
                relativeRot = InverseTransformRotation(gdRB.transform, rb.GetRotation());
            }
            public void ReapplyInfo(OWRigidbody gdRB)
            {
                rb.SetPosition(relativePos + gdRB.GetPosition());
                rb.SetVelocity(relativeVel + gdRB.GetVelocity());
                rb.SetRotation(TransformRotation(gdRB.transform, relativeRot));
            }

            /// <summary> Transforms a rotation from local space to world space. </summary>
            Quaternion TransformRotation(Transform t, Quaternion localRot) => t.rotation * localRot;

            /// <summary> Transforms a rotation from world space to local space. </summary>
            Quaternion InverseTransformRotation(Transform t, Quaternion rot) => Quaternion.Inverse(t.rotation) * rot;
        }

        void ExplodeShip()
        {
            GameObject.FindObjectOfType<ShipDamageController>().Explode();
        }
        void Crush()
        {
            //GameObject.FindObjectOfType<ShuttleCrusher>().ActivateCrusher();
        }
        void RunDebugEndCutscene()
        {
            //Load ending bundle.
            var endingBundle = modHelper.Assets.LoadBundle("endingbundle");

            NewEndingHandler.Initialize(endingBundle);
            NewEndingHandler.Static_PlayAnimation();
        }
    }
}