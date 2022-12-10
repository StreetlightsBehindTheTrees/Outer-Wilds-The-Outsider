using System;
using System.Collections;
using System.Collections.Generic;
using TheOutsider.OuterWildsHandling;
using UnityEngine;

namespace TheOutsider.MonoBehaviours
{
    public sealed class FriendConversationFixes : MonoBehaviour
    {
        NomaiConversationManager conversationManager;
        Action writingAppear;
        float writingWaitTimer = -1f;

        int symbolsAppeared;
        float timeToNextSymbol = -1f;

        GameObject pillarA, pillarB;

        float pillarsAppearTimer;
        float flashTimer;
        bool pillarTorchActive;

        //Things to fix:
        //1 - Symbols timing
        //2 - Pillars appearing
        //3 - Text timing
        public static FriendConversationFixes Instance { get; private set; }
        void Awake()
        {
            if (Log.ErrorIf(Instance != null, "This shouldn't happen!!!")) return;
            Instance = this;
            enabled = false;
        }
        public void InitializeOnEnter()
        {
            conversationManager = GetComponent<NomaiConversationManager>();

            pillarA = transform.Find("ResponseObjects/PillarA").gameObject;
            pillarB = transform.Find("ResponseObjects/PillarB").gameObject;
            
            SetPillarsActive(false);
        }
        public void BeginSymbolsAppearing()
        {
            enabled = true;
            symbolsAppeared = 0;
            timeToNextSymbol = 2f;
        }
        public void MakePillarsAppear()
        {
            enabled = true;
            pillarsAppearTimer = 5f;
            flashTimer = 2.5f;
            pillarTorchActive = false;
        }
        public void BeginWriting(Action appear)
        {
            enabled = true;
            writingWaitTimer = 3.5f;
            writingAppear = appear;
        }
        void Update()
        {
            if (timeToNextSymbol > 0f)
            {
                timeToNextSymbol -= Time.deltaTime;
                if (timeToNextSymbol < 0f)
                {
                    if (symbolsAppeared == 0)
                    {
                        conversationManager._solanumAnimController._symbolsAudioSource.PlayOneShot(AudioType.SolanumSymbolReveal, 1f);
                    }
                    if (symbolsAppeared < conversationManager._conversationStones.Length)
                    {
                        timeToNextSymbol = 0.7f;
                        conversationManager.OnTouchRock(symbolsAppeared);
                        symbolsAppeared++;
                    }
                    else TryDisable();
                }
            }
            else if (pillarsAppearTimer > 0f)
            {
                pillarsAppearTimer -= Time.deltaTime;
                flashTimer -= Time.deltaTime;

                if (flashTimer < 0f)
                {
                    if (!pillarTorchActive)
                    {
                        pillarTorchActive = true;
                        OWPatches.FriendTorchActivation(true);
                    }

                    flashTimer = 0.07f;
                    SetPillarsActive(!pillarA.activeSelf);
                }
                if (pillarsAppearTimer < 0f)
                {
                    SetPillarsActive(true);
                    OWPatches.FriendTorchActivation(false);
                    TryDisable();
                }
            }
            else if (writingWaitTimer > 0f)
            {
                writingWaitTimer -= Time.deltaTime;
                if (writingWaitTimer < 0f)
                {
                    writingAppear?.Invoke();
                    TryDisable();
                }
            }
        }
        void SetPillarsActive(bool active)
        {
            pillarA.SetActive(active);
            pillarB.SetActive(active);
        }

        void TryDisable()
        {
            if (timeToNextSymbol <= 0f && pillarsAppearTimer <= 0f && writingWaitTimer <= 0f)
            {
                enabled = false;
            }
        }
    }
}