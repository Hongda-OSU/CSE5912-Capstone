using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CSE5912.PolyGamers
{
    public class BossInformation : UI
    {
        [SerializeField] private float distanceToDisplay = 10f;

        [SerializeField] private float width = 1000f;
        //[SerializeField] private float height = 20f;

        private Label detail;
        private VisualElement healthBar;
        private VisualElement maxHealthBar;
        private bool displayHealthBar = false;

        private VisualElement debuffs;
        private List<EnemyHealthBar.DebuffSlot> debuffSlotList;

        private IEnumerable<EasingFunction> transitionFunction;

        private float prevHealth;

        private GameObject target;
        private Enemy enemy;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            Initialize();

            detail = root.Q<Label>("Detail");
            maxHealthBar = root.Q<VisualElement>("MaxHealthBar");
            healthBar = maxHealthBar.Q<VisualElement>("HealthBar");

            transitionFunction = maxHealthBar.resolvedStyle.transitionTimingFunction;

            target = transform.parent.gameObject;
            enemy = target.GetComponent<Enemy>();

            debuffs = root.Q<VisualElement>("Debuffs");
            debuffSlotList = new List<EnemyHealthBar.DebuffSlot>();
            foreach (var child in debuffs.Children())
            {
                var slot = new EnemyHealthBar.DebuffSlot(child);
                debuffSlotList.Add(slot);
                slot.Clear();
            }
        }

        private void LateUpdate()
        {
            if (displayHealthBar)
            {
                SetHealthBar();
            }
            else
            {
                maxHealthBar.style.width = 0f;
                healthBar.style.width = 0f;
                prevHealth = enemy.Health;
                root.style.display = DisplayStyle.None;
            }
            healthBar.style.maxWidth = maxHealthBar.resolvedStyle.width;
        }

        public override void Display(bool enabled)
        {
            displayHealthBar = enabled;

            root.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetHealthBar()
        {

            detail.text = enemy.EnemyName;

            healthBar.style.width = width * enemy.Health / enemy.MaxHealth;

            maxHealthBar.style.width = width;


            // debuff

            debuffSlotList[0].Display(enemy.Burned);
            debuffSlotList[1].Display(enemy.Frozen);
            debuffSlotList[2].Display(enemy.Electrocuted);
            debuffSlotList[3].Display(enemy.Infected);

            var deltaHealth = prevHealth - enemy.Health;
            if (deltaHealth > 0)
            {
                //healthBar.style.transitionDuration = new StyleList<TimeValue>(0f);
                //maxHealthBar.style.transitionDuration = new StyleList<TimeValue>(0f);

                VisualElement deltaEffect_right = new VisualElement();
                maxHealthBar.Add(deltaEffect_right);

                deltaEffect_right.style.width = width * deltaHealth / enemy.MaxHealth / 2;
                deltaEffect_right.style.height = healthBar.resolvedStyle.height;
                deltaEffect_right.style.backgroundColor = Color.white;
                deltaEffect_right.style.left = width / 2 + width * enemy.Health / enemy.MaxHealth / 2;
                deltaEffect_right.style.position = Position.Absolute;

                StartCoroutine(FadeOut(deltaEffect_right));


                VisualElement deltaEffect_left = new VisualElement();
                maxHealthBar.Add(deltaEffect_left);

                deltaEffect_left.style.width = width * deltaHealth / enemy.MaxHealth / 2;
                deltaEffect_left.style.height = healthBar.resolvedStyle.height;
                deltaEffect_left.style.backgroundColor = Color.white;
                deltaEffect_left.style.right = width / 2 + width * enemy.Health / enemy.MaxHealth / 2;
                deltaEffect_left.style.position = Position.Absolute;

                StartCoroutine(FadeOut(deltaEffect_left));

            }


            prevHealth = enemy.Health;
        }
        public IEnumerator FadeOut()
        {
            yield return StartCoroutine(FadeOut(root));
            Display(false);
        }
    }
}
