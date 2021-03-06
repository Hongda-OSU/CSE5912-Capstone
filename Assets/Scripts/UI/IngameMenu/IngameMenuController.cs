using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CSE5912.PolyGamers
{
    public class IngameMenuController : MonoBehaviour
    {
        [SerializeField] private AudioSource menuOpenAudio;
        private IngameMenu ingameMenu;

        public bool isDisplayed = false;

        InputActions inputSchemes;

        private static IngameMenuController instance;
        public static IngameMenuController Instance { get { return instance; } }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            ingameMenu = IngameMenu.Instance;
        }

        public void Initialize(InputActions inputSchemes)
        {
            this.inputSchemes = inputSchemes;
        }

        public void SwitchActive()
        {
            if (!ingameMenu.GetComponent<IngameMenu>().IsFadingComplete)
                return;

            isDisplayed = !isDisplayed;

            menuOpenAudio.Play();

            WeaponsPanelControl.Instance.ResetPanel();
            SkillsPanelControl.Instance.ResetPanel();

            PostProcessingController.Instance.SetBlurryCameraView(isDisplayed);

            if (isDisplayed)
            {
                GameStateController.Instance.SetGameState(GameStateController.GameState.InMenu);

                PlayerHudPanelControl.Instance.Root.style.display = DisplayStyle.None;
            }
            else
            {
                GameStateController.Instance.SetGameState(GameStateController.GameState.InGame);

                PlayerHudPanelControl.Instance.Root.style.display = DisplayStyle.Flex;
            }
            EnemyHealthBarControl.Instance.DisplayEnemyHealthBars(!isDisplayed);

            StartCoroutine(ingameMenu.GetComponent<IngameMenu>().DisplayMenu(isDisplayed));
        }

    }
}
