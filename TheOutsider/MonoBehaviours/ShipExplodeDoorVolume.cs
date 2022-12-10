using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class ShipExplodeDoorVolume : MonoBehaviour
    {
		OWTriggerVolume _trigger;
		bool shipIsWithinVolume;
		bool playerExited;

		ShipReactorComponent reactor;

		void Awake()
		{
			_trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();
			_trigger.OnEntry += OnEntry;
			_trigger.OnExit += OnExit;

			shipIsWithinVolume = false;
			reactor = FindObjectOfType<ShipReactorComponent>();
		}
		void OnDestroy()
		{
			_trigger.OnEntry -= OnEntry;
			_trigger.OnExit -= OnExit;
		}
		void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag(OWConstants.ShipDetector)) shipIsWithinVolume = true;
		}
		void OnExit(GameObject hitObj)
        {
			if (hitObj.CompareTag(OWConstants.ShipDetector)) shipIsWithinVolume = false;

			if (hitObj.CompareTag(OWConstants.PlayerDetector)) //On player exit
            {
				if (reactor._damaged && !PlayerState.IsInsideShip() && !playerExited)
                {
					playerExited = true;
					reactor._criticalTimer = Mathf.Lerp(3f, 5f, Random.value); //Ensure or reduce time to get away.
                }
            }
		}

		public void OnShipExplode()
        {
			if (shipIsWithinVolume) StartCoroutine(ExplodeDoorinator());
        }
		IEnumerator ExplodeDoorinator()
        {
			var door = transform.Find("DestructableDoor");
			Vector3 startPos = door.localPosition;
			Quaternion startRot = door.localRotation;
			var endDoor = transform.Find("DestroyedDoorEndPos");
			Vector3 endPos = endDoor.localPosition;
			Quaternion endRot = endDoor.localRotation;

			var recorderToDestroy = transform.Find("RecorderToDestroy");
			Destroy(recorderToDestroy.gameObject);

			float t = 0f;
			while (t < 1f)
            {
				t += Time.deltaTime;
				door.localPosition = Vector3.Lerp(startPos, endPos, t);
				door.localRotation = Quaternion.Slerp(startRot, endRot, t);
				yield return null;
            }
		}
	}
}
