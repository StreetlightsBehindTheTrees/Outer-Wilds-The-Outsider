using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheOutsider.OuterWildsHandling
{
    public sealed class TitleScreenHandler
    {
        static bool isReturnToTitle = false;

        GameObject brambles;
        RectTransform EotELogo;
        RectTransform theOutsiderLogo;
        Image EotEImage;

        Light campfireLight;
        float timeAfterCampfireLight;
        bool otherModWithSubtitleExists;

        Vector3 logoPos;

        public void OnSceneLoad(GameObject prefab_titleRoot)
        {
            Transform planetoid = null;
            var allObjs = Object.FindObjectsOfType<Transform>();
            foreach (var item in allObjs)   //Find planetoid and remove trees(?)
            {
                GameObject go = item.gameObject;
                string objName = go.name;

                if (objName == "PlanetPivot") planetoid = item;
                if (objName.Contains("Riebeck")) go.SetActive(false); //Spawn Nomai?
            }

            brambles = GameObject.Instantiate(prefab_titleRoot);
            brambles.SetActive(true);

            Transform tf = brambles.transform;
            tf.parent = planetoid;
            tf.localPosition = Vector3.zero;
            tf.localRotation = Quaternion.Euler(0f, 40f, 0f);
            tf.localScale = Vector3.one * 0.1f;

            campfireLight = Object.FindObjectOfType<Light>();

            if (isReturnToTitle) //Don't make warning text appear on return.
            {
                brambles.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            }
            isReturnToTitle = true;
        }
        public void OnUpdate()
        {
            if (brambles == null) return;

            if (campfireLight.intensity > 2.7f) {
                timeAfterCampfireLight += Time.deltaTime;
            }
            if (timeAfterCampfireLight < 6f) return;

            if (EotELogo == null)
            {
                //otherModWithSubtitleExists = ;

                var obj = GameObject.Find("Logo_EchoesOfTheEye");
                if (obj != null)
                {
                    EotELogo = obj.GetComponent<RectTransform>();
                    EotEImage = obj.GetComponent<Image>();
                    logoPos = EotELogo.localPosition;

                    theOutsiderLogo = GameObject.Find("TheOutsiderLogo").GetComponent<RectTransform>();
                    theOutsiderLogo.SetParent(EotELogo);
                    theOutsiderLogo.localRotation = Quaternion.identity;
                    theOutsiderLogo.localScale = Vector3.zero;
                }
            }

            if (!otherModWithSubtitleExists)
            {
                if (EotELogo != null)
                {
                    var col = EotEImage.color;
                    float t = Time.deltaTime * (1.1f - col.a) * 5f;

                    col.a = Mathf.Lerp(col.a, 0f, t);
                    EotEImage.color = col;
                }
            }

            if (timeAfterCampfireLight < 8f) return;
            if (theOutsiderLogo != null)
            {
                theOutsiderLogo.localPosition = new Vector3(otherModWithSubtitleExists ? 800f : 0f, -23f);
                
                theOutsiderLogo.localScale = Vector3.Lerp(theOutsiderLogo.localScale, Vector3.one * 0.75f, Time.deltaTime);
            }
        }
    }
}