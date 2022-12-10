using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class ShuttleCrusher : MonoBehaviour  //Perhaps make Scout emergency recall.
    {
        NomaiInterfaceSlot powerSlot;

        Transform crusher;
        Transform extender;
        OWAudioSource audio;

        ShipReactorComponent reactor;
        ShipComponent[] shipComponents;

        public enum CrushState { CrushDown, Wait, ReturnUp }
        CrushState state;
        bool hitShip;
        Vector3 localStartPos;
        Vector3 localHitPoint;
        float t = 0f;

        public bool HasPower { get; set; } = true;

        void Awake()
        {
            crusher = transform.Find("Crusher");
            extender = crusher.Find("Crusher_Extender");
            audio = GetComponentInChildren<OWAudioSource>();

            reactor = FindObjectOfType<ShipReactorComponent>();
            shipComponents = FindObjectsOfType<ShipComponent>();
            
            localStartPos = crusher.localPosition;

            powerSlot = GetComponentInParent<Sector>().GetComponentsInChildren<NomaiInterfaceSlot>().First(x => x.name == "OnSlot");
            powerSlot.OnSlotActivated += ActivateCrusher;
        }
        void OnDestroy()
        {
            powerSlot.OnSlotActivated -= ActivateCrusher;
        }

        void ActivateCrusher(NomaiInterfaceSlot slot)
        {
            if (enabled) return;
            if (!HasPower) return;

            enabled = true;
            InitializeDoRaycast();
            state = CrushState.CrushDown;
        }
        void InitializeDoRaycast()
        {
            hitShip = false;
            float extendDist = 36f;
            localHitPoint = crusher.localPosition - new Vector3(0f, extendDist);

            Ray r = new Ray(transform.position - transform.up, -transform.up);
            int mask = OWLayerMask.groundMask;
            if (Physics.SphereCast(r, 8f, out RaycastHit hitInfo, extendDist, mask))
            {
                if (hitInfo.rigidbody == null) hitShip = false;
                else hitShip = hitInfo.rigidbody.GetComponent<OWRigidbody>() == Locator.GetShipBody(); //Damage if hit ship.
                
                localHitPoint = transform.InverseTransformPoint(hitInfo.point);
                localHitPoint.y += 0.3f;    //Give some room.
            }

            audio.PlayOneShot(OutsiderAudio.CrushStart);
        }
        void FixedUpdate()
        {
            if (state == CrushState.CrushDown)
            {
                if (crusher.localPosition.y >= localHitPoint.y) //Crush down to point
                {
                    crusher.localPosition -= new Vector3(0f, 45f) * Time.deltaTime;

                    UpdateExtenderLength();
                    return;
                }

                Vector3 pos = crusher.localPosition;    //Make sure don't overshoot.
                pos.y = localHitPoint.y;
                crusher.localPosition = pos;
                audio.PlayOneShot(OutsiderAudio.CrushHit);

                if (hitShip) CrushShip();
                
                state = CrushState.Wait;
                t = 0f;
            }
            if (state == CrushState.Wait)
            {
                t += Time.deltaTime;
                if (t > 1.5f)
                {
                    t = 0f;
                    state = CrushState.ReturnUp;
                    audio.PlayOneShot(OutsiderAudio.CrushReturn);
                    Locator.GetShipLogManager().RevealFact("DB_SHUTTLE_CRUSHER_X3");
                }
            }
            if (state == CrushState.ReturnUp)
            {
                Vector3 currentPos = crusher.localPosition;
                t += Time.deltaTime;
                crusher.localPosition = Vector3.Lerp(currentPos, localStartPos, t);

                UpdateExtenderLength();
                float extendDist = Mathf.Abs(crusher.localPosition.y - localStartPos.y);

                if (t > 3.5f || extendDist < 0.1f)
                {
                    crusher.localPosition = localStartPos;
                    audio.PlayOneShot(OutsiderAudio.CrushReturnHit);
                    enabled = false;
                }
            }
        }
        void UpdateExtenderLength()
        {
            float extendDist = Mathf.Abs(crusher.localPosition.y - localStartPos.y);
            extender.localScale = new Vector3(1f, extendDist + 1f, 1f);
        }

        void CrushShip()
        {
            //---------------- Apply force to ship ----------------//
            Vector3 force = new Vector3(Random.value, -10f, Random.value);
            force = transform.TransformVector(force);
            Locator.GetShipBody().AddForce(force);

            //---------------- Damage ----------------//
            if (reactor.isDamaged)
            {
                //DamageRandomShipComponents(5);
                reactor._criticalTimer = 0f;
            }
            else
            {
                float timeToExplosion = 40f;
                reactor._minCountdown = timeToExplosion;
                reactor._maxCountdown = timeToExplosion;
                reactor.SetDamaged(true);
                DamageRandomShipComponents(3);
            }
        }
        void DamageRandomShipComponents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, shipComponents.Length - 1);
                var c = shipComponents[randomIndex];
                
                if (c is ShipThrusterComponent) continue; //Don't damage thrusters.

                c.SetDamaged(true);
            }
        }
    }
}