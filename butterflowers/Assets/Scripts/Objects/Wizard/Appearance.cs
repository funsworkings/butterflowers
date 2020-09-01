using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

namespace Wizard 
{

    public class Appearance: MonoBehaviour {

        Controller controller;
        Brain brain;

        new SkinnedMeshRenderer renderer;
        Material material;

        public enum State { Normal, Fade, Flicker, Leveling, Absorbed }
        public State state = State.Normal;

        float fadeTime = 0f, flickerTime = 0f, levelTime = 0f;
        bool flicker = false;

        [Header("Absorption")]
            [SerializeField] [Range(0f, 1f)] float flickerThreshold = .87f;
            [SerializeField] float absorbFadeTime = 1f;
            [SerializeField] float absorbFlickerRate = 1f;
            [SerializeField] float absorbLevelTime = 1f;
            [SerializeField] [Range(0f, 1f)] float absorbFlickerProbability = 1f;

        #region Accessors

        public float LevelingInterval => (state == State.Leveling) ? Mathf.Clamp01(levelTime / absorbLevelTime) : 0f;

		#endregion

		void Awake()
        {
            controller = GetComponent<Controller>();
            renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        void Start()
        {
            material = renderer.material;
            brain = controller.Brain;
        }

        void Update()
        {
            if (state != State.Fade && state != State.Leveling)
                state = EvaluateState(); // Trigger state here

            if (state == State.Normal)
                SetAbsorbWeight(0f);
            if (state == State.Absorbed)
                SetAbsorbWeight(1f);
            if (state == State.Flicker)
                Flickering();
            if (state == State.Fade)
                Fading();
            if (state == State.Leveling)
                Leveling();
        }

        State EvaluateState() {
            float absorption = brain.absorption;
            State state = State.Normal;

            if (controller.ABSORBED)
                state = State.Absorbed;
            else 
            {
                if (absorption >= flickerThreshold) 
                {
                    state = State.Flicker;
                }
            }

            return state;
        }

        public void Fade()
        {
            if (state == State.Absorbed || state == State.Leveling)
                return;

            state = State.Fade;
            fadeTime = 0f;
        }

        void Fading()
        {
            float interval = 0f;

            fadeTime += Time.deltaTime;
            interval = fadeTime / absorbFadeTime;

            SetAbsorbWeight(1f - Mathf.Clamp01(interval));

            if (interval > 1f)
                state = EvaluateState();
        }

        public void LevelUp()
        {
            state = State.Leveling;
            levelTime = 0f;
        }

        void Leveling()
        {
            float interval = 0f;

            levelTime += Time.deltaTime;
            interval = levelTime / absorbLevelTime;

            SetAbsorbWeight(Mathf.Clamp01(interval));
            if (interval > 1f)
                state = State.Absorbed;
        }

        void Flickering()
        {
            flickerTime += Time.deltaTime;
            if (flickerTime >= absorbFlickerRate) {
                float random = Random.Range(0f, 1f);

                float weight = 0f;
                if (random <= absorbFlickerProbability)
                    weight = 1f;

                SetAbsorbWeight(weight);
                flickerTime = 0f;
            }
        }

        void SetAbsorbWeight(float weight)
        {
            material.SetFloat("_Weight", weight);
        }
    }
}
