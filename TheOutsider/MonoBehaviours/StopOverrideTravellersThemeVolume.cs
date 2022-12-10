using System;
using System.Collections;
using System.Collections.Generic;
using TheOutsider.OuterWildsHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class StopOverrideTravellersThemeVolume : MonoBehaviour
    {
		OWTriggerVolume _trigger;
		bool _initialized;

		void Start()
		{
			_trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();
			_trigger.OnEntry += OnEntry;
			_initialized = true;
		}
		void OnDestroy()
		{
			if (_initialized) _trigger.OnEntry -= OnEntry;
		}
		void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag(OWConstants.PlayerDetector))
			{
				_trigger.OnEntry -= OnEntry;

				OWPatches.ReturnToNormalTravelMusic = true;
				OWPatches.StopTravelMusicUpdate = false;
			}
		}
	}
}
