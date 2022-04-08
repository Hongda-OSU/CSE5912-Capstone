using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class FailedHusk : BossEnemy
    {
        [Header("Failed Husk")]
        [SerializeField] private bool isUnleashed = false;
        [SerializeField] private float unleashedHealth = 10000f;
        [SerializeField] private string unleashedName = "Flawless Form";

        [SerializeField] private float leashedSpeed = 0.5f;
        [SerializeField] private float unleashedSpeed = 1f;

        [SerializeField] private float blinkCastSpeed = 0.25f;
        [SerializeField] private float blinkCastTime = 0.5f;

        [SerializeField] private GameObject seals;
        [SerializeField] private float healthPerSeal;
        private List<GameObject> sealList = new List<GameObject>();
        [SerializeField] private float currentDamageTaken = 0f;

        [SerializeField] private float walkTime;
        [SerializeField] private float timeToBlink = 3f;

        [SerializeField] private Damager_collision sword;

        [SerializeField] private BossArea bossArea;

        private SwordZone swordZone;
        private GroundCrack groundCrack;
        private Slash slash;
        private Blink blink;

        private List<EnemySkill> skillList = new List<EnemySkill>();

        private Attack_0_failedHusk attack_0;
        private Attack_1_failedHusk attack_1;
        private Attack_2_failedHusk attack_2;
        private Attack_3_failedHusk attack_3;
        private Attack_4_failedHusk attack_4;

        [SerializeField] private bool isPerforming = false;

        private void Awake()
        {
            foreach (Transform seal in seals.transform)
            {
                sealList.Add(seal.gameObject);
            }

            sword.Initialize(this);
            sword.BaseDamage = AttackDamage;

            swordZone = GetComponentInChildren<SwordZone>();
            groundCrack = GetComponentInChildren<GroundCrack>();
            slash = GetComponentInChildren<Slash>();
            blink = GetComponentInChildren<Blink>();

            attack_0 = GetComponentInChildren<Attack_0_failedHusk>();
            attack_1 = GetComponentInChildren<Attack_1_failedHusk>();
            attack_2 = GetComponentInChildren<Attack_2_failedHusk>();
            attack_3 = GetComponentInChildren<Attack_3_failedHusk>();
            attack_4 = GetComponentInChildren<Attack_4_failedHusk>();

            skillList.Add(attack_0);
            skillList.Add(attack_1);
            skillList.Add(attack_2);
            skillList.Add(attack_3);
            skillList.Add(attack_4);

            isInvincible = true;

            maxHealth = healthPerSeal * 5;
            health = maxHealth;
        }
        protected override void Start()
        {
            base.Start();

            animator.speed = leashedSpeed;
        }
        public override void TakeDamage(Damage damage)
        {
            if (!isAlive || isInvincible)
            {
                return;
            }

            float value = damage.ResolvedValue;
            if (sealList.Count > 0)
            {
                health -= value;

                currentDamageTaken += value;
                if (currentDamageTaken > healthPerSeal)
                {
                    animator.SetTrigger("Stagger");

                    var seal = sealList[0];
                    sealList.RemoveAt(0);
                    Destroy(seal);

                    currentDamageTaken = 0;
                    if (sealList.Count == 0)
                    {
                        animator.SetTrigger("Unseal");
                        BgmControl.Instance.MainAudio.Stop();
                        GetComponentInChildren<BossInformation>().Display(false);
                        isBossFightTriggered = false;
                    }
                }
            }

            else
            {
                health -= value;

                if (health <= 0)
                {
                    Die();

                    //test
                    DropWeapon();
                    DropAttachment();
                }
            }

            if (!isAttackedByPlayer)
            {
                isAttackedByPlayer = true;
            }
        }

        private IEnumerator Unleash()
        {
            isUnleashed = true;

            animator.speed = unleashedSpeed;

            enemyName = unleashedName;

            maxHealth = unleashedHealth;
            health = maxHealth;

            StartCoroutine(bossArea.TriggerBossFight(0f));

            yield return null;
        }

        protected override void PerformActions()
        {
            //test
            PlayerStats.Instance.IsInvincible = true;

            if (isPerforming || !isBossFightTriggered)
                return;


            switch (status)
            {
                case Status.Idle:
                    if (playerDetected)
                    {
                        FaceTarget(directionToPlayer);

                        if (!isAttacking)
                        {
                            if (isPlayerInAttackRange)
                            {
                                Attack();
                            }
                            else
                            {
                                MoveToPlayer();
                            }
                        }
                    }
                    else
                    {
                        Rest();
                    }
                    break;

                case Status.Moving:
                    status = Status.Idle;

                    if (isUnleashed)
                    {
                        walkTime += Time.deltaTime;
                        if (walkTime > timeToBlink)
                        {
                            StartCoroutine(Blink_performed());
                            walkTime = 0f;
                            Attack();
                        }
                    }
                    break;

                case Status.Attacking:
                    if (isFatigued)
                    {
                        PrepareForNextAttack();
                    }
                    else
                    {
                        status = Status.Idle;
                    }

                    if (isUnleashed)
                    {
                        walkTime = 0f;
                    }

                    break;

                case Status.Retreating:

                    break;

                case Status.Waiting:
                    if (!isFatigued)
                    {
                        Rest();
                    }
                    break;
            }
        }

        public override void TriggerBossFight()
        {
            isInvincible = true;
            animator.applyRootMotion = true;
            animator.SetTrigger("Awake");
        }

        protected override void AwakeAnimationComplete()
        {
            isInvincible = false;
            isBossFightTriggered = true;
            animator.applyRootMotion = true;
            isPerforming = false;
        }

        public override void ResetEnemy()
        {
            base.ResetEnemy();
            isBossFightTriggered = false;

        }

        protected override IEnumerator PerformActionsOnWaiting()
        {
            SetMove(Direction.Backward);

            yield return new WaitForSeconds(Random.Range(1f, 3f));

            currentAttackNum = 0;
            SetMove(Direction.None);
            status = Status.Idle;
        }

        protected override void PlayDeathAnimation()
        {
            animator.applyRootMotion = true;

            animator.SetTrigger("Die");
        }


        private void Attack()
        {
            SetMove(Direction.None);
            status = Status.Attacking;

            var usable = new List<int>();
            for (int i = 0; i < skillList.Count; i++) 
            {
                var skill = skillList[i];
                if (skill.IsPerformingAllowed())
                    usable.Add(i);
            }

            if (usable.Count > 0)
                SkillAttack(usable[Random.Range(0, usable.Count)]);
            else
                PrepareForNextAttack();

        }

        private void SkillAttack(int index)
        {
            status = Status.Attacking;
            SetAttack(index);
            currentAttackNum++;
            isPerforming = true;
        }
        private IEnumerator Attack_performed(int index)
        {
            yield return StartCoroutine(skillList[index].Perform());

            isPerforming = false;
        }

        private IEnumerator BaseSwordAttack_started()
        {
            sword.IsPlayerHit = false;

            yield return null;
        }
        private IEnumerator BaseSwordAttack_finished()
        {
            sword.IsPlayerHit = true;

            yield return null;
        }

        private IEnumerator SwordZone_performed()
        {
            if (!isUnleashed)
                yield break;

            StartCoroutine(swordZone.Perform());
        }

        private IEnumerator GroundCrack_performed()
        {
            if (!isUnleashed)
                yield break;

            StartCoroutine(groundCrack.Perform());
        }
        
        /*
         *  0 = right, 
         *  1 = down,
         *  2 = left,
         *  3 = top,
         *  4 = prick,
         */
        private IEnumerator Slash_performed(int flag)
        {
            if (!isUnleashed)
                yield break;

            slash.direction = flag;
            StartCoroutine(slash.Perform());
        }
        private IEnumerator Blink_performed()
        {
            if (!isUnleashed)
                yield break;

            var position = player.position - directionToPlayer * 3f;

            animator.speed = blinkCastSpeed;
            yield return new WaitForSeconds(blinkCastTime);
            yield return StartCoroutine(blink.Perform(position));
            animator.speed = unleashedSpeed;
        }

        private void DonePerforming()
        {
            isPerforming = false;
        }

    }
}
