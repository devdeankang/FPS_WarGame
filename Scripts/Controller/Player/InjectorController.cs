using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace WarGame
{
    public class InjectorController : MonoBehaviour
    {

        #region Variables

        public string StartAnim;
        public string IdleAnim;
        public string EndAnim;
        public string SubstanceAnim;
        public Animator HandsAnimator;
        public Animator LampAnimator;

        public AudioSource StartSFX;
        public float StartSFXDelay = 0.3f;
        public AudioSource EndSFX;

        public string LampToIdle = "InjectorLampIdle";
        public string LampToWork = "InjectorLampToWork";
        public int InjectorSubstanceLayer = 4;

        public GameObject GasParticles;

        public SkinnedMeshRenderer InjectionMesh;
        public string BlendShaderParameter = "_Blend";
        public string BlendAnimatorParameter = "InjectionHandBlend";
        [Range(0f, 1f)] public float BlendParameterValue;

        public float UsingSpeed = 0.2f;
        public PostProcessingController PostProcessingController;

        public PostProcessingController.ColorGradingSettings EffectSettings;
        public AnimationCurve EffectCurve;
        public float EffectDuration = 5f;

        public PlayerHealth TargetHealth;
        public PlayerInventory Inventory;

        public GameObject[] Props;
        float effectTime = 0;
        MedkitItem medkitItem;

        bool isInjectionInProcess;
        bool isEndInProcess;

        public UnityEvent InjectionEndEvent = new UnityEvent();
        public UnityEvent InjectionStartEvent = new UnityEvent();

        #endregion

        public int FlasksCount { get { return medkitItem.GetCurrentCount(); } }

        public float CurrentFraction { get { return currentFraction; } }

        float currentFraction { get { return medkitItem.FractionAmount; } }

        public bool InjectionInProcess { get { return isInjectionInProcess; } }

        float CurrentHealth { get { return TargetHealth.Health; } }

        float MaxHealth { get { return TargetHealth.StartHealth; } }


        private void Awake()
        {
            foreach(GameObject g in Props)
            {
                g.SetActive(false);
            }

            medkitItem = new MedkitItem("MedKit");
            Inventory.AddItem(medkitItem);
        }

        public void StartInject()
        {
            if (isInjectionInProcess || CurrentHealth >= MaxHealth)
                return;

            isInjectionInProcess = true;
            isEndInProcess = false;

            
            foreach (GameObject g in Props)
            {
                g.SetActive(false);
            }
            StartCoroutine(WaitForInject());
        }

        IEnumerator WaitForInject()
        {
            AnimatorStateInfo state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
            while (!state.IsName("Default State"))
            {
                state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            StartSFX.PlayDelayed(StartSFXDelay);
            HandsAnimator.Play(StartAnim);
            HandsAnimator.Play(SubstanceAnim, InjectorSubstanceLayer, 1f - currentFraction);
            foreach (GameObject g in Props)
            {
                g.SetActive(true);
            }
            GasParticles.SetActive(false);
            InjectionStartEvent.Invoke();
            LampAnimator.Play(LampToIdle);
            StartCoroutine(LoopProcessing());
            effectTime = 0;
        }

        public void ForceInjectionEnd()
        {
            if (isEndInProcess)
                return;

            isEndInProcess = true;
            StartCoroutine(WaitForIdle());
        }

        public void EndInject()
        {
            StartCoroutine(WaitForEnd());
        }

        public void VisualInject()
        {
            GasParticles.SetActive(true);
            StartCoroutine(EffectProcessing());
        }

        IEnumerator EffectProcessing()
        {
            PostProcessingController.ColorGradingSettings currentSettings = PostProcessingController.ColorSettings;
            while (effectTime <= EffectDuration)
            {
                effectTime += Time.deltaTime;

                float fraction = effectTime / EffectDuration;
                float value = EffectCurve.Evaluate(fraction);

                PostProcessingController.LerpColorGrading(currentSettings, EffectSettings, value);

                yield return null;
            }
        }

        IEnumerator LoopProcessing()
        {
            AnimatorStateInfo state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
            while(!state.IsName(IdleAnim))
            {
                state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            LampAnimator.Play(LampToWork);
            while (state.IsName(IdleAnim))
            {
                state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
                if (!isEndInProcess)
                {
                    float deltaFraction = UsingSpeed * Time.deltaTime;
                    medkitItem.ConsumeFraction(deltaFraction);
                    TargetHealth.Heal(MaxHealth * deltaFraction);
                    float clampedHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
                    float deltaFromClamped = CurrentHealth - clampedHealth;
                    deltaFromClamped = Mathf.Clamp(deltaFromClamped, 0, 1);

                    float deltaFractionFromClamped = deltaFromClamped / MaxHealth;
                    medkitItem.ConsumeFraction(-deltaFractionFromClamped);

                    if (currentFraction <= 0)
                    {
                        if (FlasksCount > 0)
                        {
                            medkitItem.Consume(1);
                        }
                        else
                        {
                            ForceInjectionEnd();
                            break;
                        }
                    }

                    if (CurrentHealth >= MaxHealth)
                    {
                        ForceInjectionEnd();
                    }

                    HandsAnimator.Play(SubstanceAnim, InjectorSubstanceLayer, 1f - currentFraction);
                }
                yield return null;
            }
        }

        IEnumerator WaitForIdle()
        {
            AnimatorStateInfo state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
            while (!state.IsName(IdleAnim))
            {
                state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            EndSFX.Play();
            LampAnimator.Play(LampToIdle);
            HandsAnimator.CrossFadeInFixedTime(EndAnim, 0.05f);
        }

        IEnumerator WaitForEnd()
        {
            AnimatorStateInfo state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
            while(state.normalizedTime < 0.9f)
            {
                state = HandsAnimator.GetCurrentAnimatorStateInfo(0);
                yield return null;
            }
            foreach (GameObject g in Props)
            {
                g.SetActive(false);
            }
            InjectionEndEvent.Invoke();
            isInjectionInProcess = false;
        }

        private void Update()
        {
            BlendParameterValue = HandsAnimator.GetFloat(BlendAnimatorParameter);
            Material mat = InjectionMesh.sharedMaterial;
            mat.SetFloat(BlendShaderParameter, BlendParameterValue);
        }
    }

    public class MedkitItem : BaseItem
    {
        public float FractionAmount;

        public MedkitItem(string itemName) : base(itemName)
        {
            MaxCapacity = 5;
            FractionAmount = 0f;
            IconName = "MedKit";
        }

        public virtual void ConsumeFraction(float delta)
        {
            FractionAmount -= delta;
        }

        public override void Added(int count)
        {
            base.Added(count);
            if (GetCurrentCount() > 0)
                FractionAmount = 1f;
            else
                FractionAmount = 0;
        }

        public override void Consumed(int count)
        {
            base.Consumed(count);

            if (GetCurrentCount() > 0)
                FractionAmount = 1f;
            else
                FractionAmount = 0;
        }
    }
}