using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CSE5912.PolyGamers
{
    public class TipsControl : UI
    {
        [SerializeField] private bool isDisplayed = false;

        private VisualElement tips;
        private Label key;
        private Label action;

        private static TipsControl instance;
        public static TipsControl Instance { get { return instance; } }
        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(gameObject);
            instance = this;

            Initialize();

            tips = root.Q<VisualElement>("Tips");
            key = tips.Q<Label>("Key");
            action = tips.Q<Label>("Action");

            tips.style.display = DisplayStyle.None;
        }

        public void PopUpTip(string keyTip, string actionTip)
        {
            if (isDisplayed)
                return;

            isDisplayed = true;

            key.text = keyTip;
            action.text = actionTip;
            StartCoroutine(FadeIn(tips));
        }
        public void PopOffTip()
        {
            if (!isDisplayed)
                return;

            isDisplayed = false;

            StartCoroutine(FadeOut(tips));
        }
    }
}
