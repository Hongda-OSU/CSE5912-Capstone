using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CSE5912.PolyGamers
{
    public abstract class EliteEnemy : Enemy
    {
        [Header("Behavior Parameters")]
        // max number of continuous attacks
        [SerializeField] protected int maxContinuousAttackNum = 3;
        protected int currentAttackNum = 0;

        // time interval between each attack
        [SerializeField] protected float timeBetweenAttack = 3f;
        protected float timeSinceAttack = 0f;
        protected bool isReadyToAttack = true;

        // time interval range between each action when waiting for next attack
        [SerializeField] protected Vector2 timeRangeBetweenWaitActions = new Vector2(1, 2);
        protected float timeSinceWaitAction = 1f;

        // if aggro > aggroThreshold, attack player anyway
        [SerializeField] protected float aggro = 0f;
        [SerializeField] protected float aggroThreshold = 2f;

        [SerializeField] protected float maxRetreatTime = 3f;
        protected bool isRetreatFinished = true;

        protected int waitAction = -1;

        protected bool isAttacking = false;

        protected bool playerDetected = false;

        protected bool isPlayerInAttackRange;
        protected bool isPlayerInSafeDistance;


        // enemy status
        protected Status status = Status.Idle;
        protected enum Status
        {
            Idle,
            Moving,
            Retreating,
            Attacking,
            Wait,
        }

        // animation direction
        protected enum Direction
        {
            Forward,
            Backward,
            Left,
            Right,
            None = -1,
        }

        protected virtual void Start()
        {
            Initialize();

            agent.isStopped = true;
            animator.applyRootMotion = true;
        }

        protected override void Update()
        {
            if (!isAlive)
                return;

            base.Update();

            isPlayerInAttackRange = distanceToPlayer < attackRange;

            isPlayerInSafeDistance = distanceToPlayer < closeDetectionRange;

            CalculateAggro();

            PerformActions();
        }



        /*
         *  enemy actions
         */

        // perform the whole action logic
        protected abstract void PerformActions();

        // actions
        protected virtual void Rest()
        {
            status = Status.Idle;

            SetMove(Direction.None);
            SetAttack(-1);
            SetRoll(Direction.None);
        }
        protected virtual void MoveToPlayer()
        {
            status = Status.Moving;

            SetMove(Direction.Forward);

            FaceTarget(directionToPlayer);
            agent.SetDestination(player.position);
        }

        protected virtual void Attack(int index)
        {
            status = Status.Attacking;

            SetMove(Direction.None);

            SetAttack(index);
        }

        protected virtual void Retreat()
        {
            status = Status.Retreating;

            SetAttack(-1);
            SetMove(Direction.Backward);

            StartCoroutine(MoveBack());
        }

        private IEnumerator MoveBack()
        {
            isRetreatFinished = false;

            float timeSince = 0f;
            while (timeSince < maxRetreatTime && isPlayerInAttackRange && status == Status.Retreating)
            {
                timeSince += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);

                agent.destination = directionToPlayer * -5f;
                Debug.Log(directionToPlayer);
            }

            isRetreatFinished = true;
        }

        protected virtual void PrepareForNextAttack()
        {
            status = Status.Wait;

            timeSinceWaitAction += Time.deltaTime;

            float randomWaitTime = Random.Range(timeRangeBetweenWaitActions.x, timeRangeBetweenWaitActions.y);
            if (timeSinceWaitAction >= randomWaitTime)
            {
                waitAction = Random.Range(-2, 4);
                timeSinceWaitAction = 0f;

                SetMove((Direction)waitAction);
                bool roll = Random.value < 0.5f;
                if (roll)
                    SetRoll((Direction)waitAction);

                return;
            }

            SetRoll(Direction.None);

            FaceTarget(directionToPlayer);
            agent.isStopped = false;

            currentAttackNum = 0;

        }




        /*
         * generals
         */

        protected virtual void CalculateAggro()
        {
            // check if player is detected
            bool isInViewDistance = distanceToPlayer <= viewRadius;
            bool isInViewAngle = Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2;
            bool isInSafeDistance = distanceToPlayer <= closeDetectionRange;

            playerDetected = isInViewDistance && isInViewAngle || isInSafeDistance || isAttackedByPlayer;

            // increase aggro if player stays in safe distance
            if (isPlayerInSafeDistance && aggro < aggroThreshold)
            {
                aggro += Time.deltaTime;
            }
            else if (!isPlayerInSafeDistance && aggro > 0f)
            {
                aggro -= Time.deltaTime;
            }
        }

        /*
         *  animation control
         */

        protected virtual void SetMove(Direction dir)
        {
            animator.SetInteger("Move", (int)dir);
            agent.isStopped = dir == Direction.None;
        }

        protected virtual void SetAttack(int index)
        {
            /*
             * 4 attacks in total
             * -1 means stop attacking
             */
            animator.SetInteger("Attack", index);
        }

        protected virtual void SetRoll(Direction dir)
        {
            animator.SetInteger("Roll", (int)dir);
        }


        protected virtual void StartAttack()
        {
            isAttacking = true;
        }
        protected virtual void FinishAttack()
        {
            isAttacking = false;
            currentAttackNum++;
        }


    }
}
