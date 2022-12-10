using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class ObservatoryFrontDoor : MonoBehaviour
    {
        OWTriggerVolume _trigger;

        GameObject door;    //Can't be this object as this runs stuff.
        MeshRenderer doorRenderer;
        Transform tf;

        const float doorThickness = 0.2f;
        const float shipRadius = 1120f;

        bool keepOffUntilNotVisible;

        //--------------------------------------------- Initialize ---------------------------------------------//
        void Awake()
        {
            door = transform.GetChild(0).gameObject;
            doorRenderer = door.GetComponent<MeshRenderer>();

            _trigger = GetComponent<OWTriggerVolume>();
            _trigger.OnEntry += OnEntry;
            _trigger.OnExit += OnExit;

            tf = transform;

            enabled = false;
        }
        void OnDestroy() {
            _trigger.OnEntry -= OnEntry;
            _trigger.OnExit -= OnExit;
        }
        void OnEntry(GameObject hitObj) {
            if (hitObj.CompareTag(OWConstants.PlayerDetector))
            {
                door.SetActive(true);
                keepOffUntilNotVisible = false;
                enabled = true;
            }
        }
        void OnExit(GameObject hitObj) {
            if (hitObj.CompareTag(OWConstants.PlayerDetector))
            {
                door.SetActive(true);
                keepOffUntilNotVisible = false;
                enabled = false;
            }
        }

        //--------------------------------------------- Check ---------------------------------------------//
        void Update()
        {
            Vector3 dbPos = Locator._darkBramble._owRigidbody.GetPosition();

            bool shipOnPlanet = Vector3.Distance(Locator.GetShipBody().GetPosition(), dbPos) < shipRadius;
            //shipOnPlanet |= ; //Or if ship destroyed?
            if (shipOnPlanet)
            {
                door.SetActive(true);
                return;
            }

            bool playerOnPlanet = Vector3.Distance(Locator.GetPlayerBody().GetPosition(), dbPos) < shipRadius;
            if (!playerOnPlanet)
            {
                door.SetActive(true); //Stop door disappearing when send probe through seed at Power Station.
                return;
            }

            //---------------- Player not looking at ----------------//
            Vector3 vec = Locator.GetPlayerBody().GetPosition() - tf.position;
            float signedDist = Vector3.Dot(vec, tf.forward);
            
            var planes = GeometryUtility.CalculateFrustumPlanes(Locator.GetPlayerCamera().mainCamera);
            bool rendererVisible = GeometryUtility.TestPlanesAABB(planes, doorRenderer.bounds);

            //Stop door from enabling when passing backwards through it.
            if (Mathf.Abs(signedDist) < doorThickness)  //If back into door, by it being not visible, ensure door is off.
            {
                if (vec.magnitude < 6f) //Only turn off if pass through door.
                {
                    door.SetActive(false);
                    keepOffUntilNotVisible = true;
                    return;
                }
            }
            if (keepOffUntilNotVisible)     //After being inside door, it should remain off until it is not visible again.
            {
                if (rendererVisible) return;
                else keepOffUntilNotVisible = false;
            }

            door.SetActive(rendererVisible && ExtraDotCheck(signedDist));
        }
        bool ExtraDotCheck(float signedDist)  //Simpler method to deactivate to ensure renderer visibility doesn't fail.
        {
            float d = Vector3.Dot(tf.forward, Locator.GetPlayerCamera().transform.forward);

            float threshold = 0.8f;
            if (signedDist < 0f)
                return d > -threshold;
            else return d < threshold;
        }

        #region Old
        //Newer plan:
        //- Points of corners to screen space (create quad mesh)
        //- If any door corner is inside screen corners, visible.
        //- If any screen corner is within door corners, visible.
        /*
        Mesh mesh;
        Transform meshTF;
        Vector2[] vertsViewPos = new Vector2[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            vertsViewPos[i] = camera.WorldToViewportPoint(tf.TransformPoint(verts[i]));
        }
        */

        /*
        Vector3 min = camera.WorldToViewportPoint(tf.TransformPoint(-extents));
        Vector3 max = camera.WorldToViewportPoint(tf.TransformPoint(extents));
        Vector3 pos = camera.WorldToViewportPoint(tf.position);
        if (pos.z < 0) return false; //Behind, not visible.

        Vector3 size = max - min;

        Rect doorRect = new Rect(pos, size);
        Rect screenRect = new Rect(0.5f, 0.5f, 1f, 1f);
        if (RectContainsRect(doorRect, screenRect) || RectContainsRect(screenRect, doorRect)) return true;

        //- else, not visible.
        return false;
        */

        //New plan: Get just one point in world space. Tries to go to camera center but limited by local mesh bounds.
        //Then convert this point to viewport space get if on screen.

        //Almost works. Can slip through if don't see point, and door appears if don't see point when going backwards.

        /*
        Vector3 planePos = tf.position;
        Vector3 planeNormal = cameraTf.forward;
        //Vector3 pos = cameraTf.position;
        //float dist = Vector3.Dot(pos - planePos, planeNormal);
        //Vector3 target = pos - planeNormal * dist;

        Ray ray = new Ray(cameraTf.position, cameraTf.forward);
        Vector3 target = PointIntersectsPlane(ray, planePos, planeNormal);  //Need better way of getting target.

        Vector3 localTarget = tf.InverseTransformPoint(target);
        localTarget = Clamp3(localTarget, -extents, extents);
        target = tf.TransformPoint(localTarget);

        Vector3 viewPortPos = camera.WorldToViewportPoint(target);

        if (viewPortPos.z > 0f && Between01(viewPortPos.x) && Between01(viewPortPos.y)) return true;
        return false;
        */

        /*
        Vector3 XYZ(float f) => new Vector3(f, f, f);
        bool RectContainsRect(Rect r1, Rect r2)
        {
            if (r1.Contains(r2.min)) return true;
            if (r1.Contains(r2.max)) return true;
            if (r1.Contains(new Vector2(r2.xMin, r2.yMax))) return true;
            if (r1.Contains(new Vector2(r2.xMax, r2.yMin))) return true;
            return false;
        }
        /// <summary> Point where the ray intersects the plane at position with normal. </summary>
        Vector3 PointIntersectsPlane(Ray ray, Vector3 planePos, Vector3 planeNormal)
        {
            float dot = Vector3.Dot(ray.direction, planeNormal);
            float dist = 10000000f;
            if (Mathf.Abs(dot) >= 0.00001f) dist = Vector3.Dot(planePos - ray.origin, planeNormal) / dot;

            return ray.GetPoint(dist);
        }
        public Vector3 Clamp3(Vector3 v, Vector3 min, Vector3 max)
        {
            Vector3 r = v;

            r.x = Mathf.Clamp(v.x, min.x, max.x);
            r.y = Mathf.Clamp(v.y, min.y, max.y);
            r.z = Mathf.Clamp(v.z, min.z, max.z);

            return r;
        }
        bool Between01(float f) => 0f < f && f < 1f;
        */
        #endregion
    }
}