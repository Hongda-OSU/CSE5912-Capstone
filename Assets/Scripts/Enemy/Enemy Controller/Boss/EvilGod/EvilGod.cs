using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class EvilGod : BossEnemy
    {
        [Header("Evil God")]

        [SerializeField] private float dodgeChance = 0.5f;

        private LightningMissile_evilGod lightningMissile;
        private LightningExplosion_evilGod lightningExplosion;
        private LightningStorm_evilGod lightningStorm;
        private Barrage_evilGod barrage;

        private Shield_evilGod shield;
        private Blink blink;

        private bool isPerforming = false;

        private void Awake()
        {
            lightningMissile = GetComponentInChildren<LightningMissile_evilGod>();
            lightningExplosion = GetComponentInChildren<LightningExplosion_evilGod>();
            lightningStorm = GetComponentInChildren<LightningStorm_evilGod>();
            barrage = GetComponentInChildren<Barrage_evilGod>();

            shield = GetComponentInChildren<Shield_evilGod>();
            blink = GetComponentInChildren<Blink>();

            isInvincible = true;
        }


        protected override void PerformActions()
        {
            if (!PlayerStats.Instance.IsAlive)
            {
                isPerforming = true;
            }
            if (isPerforming || !isBossFightTriggered)
                return;

            if (Random.value < dodgeChance)
                Dodge();

            if (shield.IsPerformingAllowed())
            {
                OpenShield();
            }

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
                    break;

                case Status.Attacking:
                    if (isPlayerInSafeDistance)
                    {
                        Blink(transform.position + directionToPlayer * -attackRange);
                    }
                    else if (isFatigued)
                    {
                        PrepareForNextAttack();
                    }
                    status = Status.Idle;

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
            animator.applyRootMotion = false;
            isPerforming = false;
        }

        public override void ResetEnemy()
        {
            base.ResetEnemy();
            if (lightningStorm.MissileList.Count > 0)
            {
                foreach (var lighting in lightningStorm.MissileList)
                {
                    Destroy(lighting);
                }
            }
            isBossFightTriggered = false;
            isPerforming = false;

            lightningMissile.IsReady = true;
            lightningExplosion.IsReady = true;
            lightningStorm.IsReady = true;
            barrage.IsReady = true;
            blink.IsReady = true;
        }

        protected override void MoveToPlayer()
        {
            base.MoveToPlayer();

            Vector3 position = PlayerManager.Instance.Player.transform.position + 
                Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * directionToPlayer * Random.Range(3f, attackRange * 0.8f);

            Blink(position);
        }
        protected override IEnumerator PerformActionsOnWaiting()
        {
            Vector3 position = PlayerManager.Instance.Player.transform.position + Quaternion.AngleAxis(Random.Range(0f, 135f), Vector3.up) * directionToPlayer * attackRange;
            Blink(position);
            currentAttackNum = 0;
            yield return null;
        }

        protected override void PlayDeathAnimation()
        {
            animator.applyRootMotion = true;

            animator.SetTrigger("Die");
        }



        private void Dodge()
        {
            if (!WeaponManager.Instance.CarriedWeapon.wasBulletFiredThisFrame || !Physics.Raycast(player.position, WeaponManager.Instance.GetShootDirection(), out RaycastHit hit, 1000))
                return;

            var offset = Vector3.right * GetComponent<Collider>().bounds.size.x;
            if (Random.value < 0.5f)
                offset = -offset;

            var position = transform.position + offset;
            Blink(position);
        }
        private void Blink(Vector3 position)
        {
            StartCoroutine(blink.Perform(position));
        }

        private void Attack()
        {
            if (lightningStorm.IsPerformingAllowed())
                Attack_lightningStorm();

            else if (lightningExplosion.IsPerformingAllowed())
                Attack_lightningExplosion();

            else if (barrage.IsPerformingAllowed() && lightningStorm.MissileList.Count > 0)
                Attack_barrage();

            else if (lightningMissile.IsPerformingAllowed())
                Attack_lightningMissile();

            status = Status.Attacking;
        }

        private void Attack_lightningMissile()
        {
            if (!lightningMissile.IsPerformingAllowed())
                return;

            status = Status.Attacking;
            SetAttack(0);
            currentAttackNum++;
            isPerforming = true;
        }
        private IEnumerator LightningMissile_performed()
        {
            StartCoroutine(lightningMissile.Perform());

            yield return new WaitForSeconds(1f);
            isPerforming = false;
        }


        private void Attack_lightningExplosion()
        {
            if (!lightningExplosion.IsPerformingAllowed())
                return;

            status = Status.Attacking;
            SetAttack(1);
            currentAttackNum++;
            isPerforming = true;
        }
        private IEnumerator LightningExplosion_performed()
        {
            Vector3 position = PlayerManager.Instance.Player.transform.position + Quaternion.AngleAxis(Random.Range(0f, 135f), Vector3.up) * directionToPlayer * attackRange / 2;
            Blink(position);

            yield return new WaitForSeconds(Time.deltaTime);
            isPerforming = false;

            yield return StartCoroutine(lightningExplosion.Perform());
        }


        private void Attack_lightningStorm()
        {
            if (!lightningStorm.IsPerformingAllowed())
                return;

            status = Status.Attacking;
            SetAttack(2);
            currentAttackNum++;
            isPerforming = true;

            Vector3 position = PlayerManager.Instance.Player.transform.position + directionToPlayer * 5f;
            Blink(position);

        }
        private IEnumerator LightningStorm_performed()
        {
            yield return StartCoroutine(lightningStorm.Perform());
        }

        private void Attack_barrage()
        {
            if (!barrage.IsPerformingAllowed())
                return;

            status = Status.Attacking;
            SetAttack(3);
            currentAttackNum++;
            isPerforming = true;

            Vector3 position = PlayerManager.Instance.Player.transform.position + directionToPlayer * attackRange * 0.8f;
            Blink(position);
        }

        private IEnumerator Barrage_performed()
        {
            yield return StartCoroutine(barrage.Perform(lightningStorm.MissileList));
        }


        private void OpenShield()
        {
            animator.SetTrigger("Shield");
            isPerforming = true;

        }
        private IEnumerator Shield_performed()
        {
            isPerforming = false;

            yield return StartCoroutine(shield.Perform());
        }

        private void DonePerforming()
        {
            isPerforming = false;
        }

        protected override void Die()
        {
            base.Die();

            GameStateController.Instance.bossToAlive["EvilGod"] = false;
        }
    }
}
