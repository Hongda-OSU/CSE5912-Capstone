using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Goblin_1 : MonoBehaviour, IEnemy
{
    private float viewRadius = 15f;
    private float closeDetectionDistance = 3f;
    [Range(0, 360)]
    private float viewAngle = 135f;
    private bool foundTarget = false; // This is used for testing

    private float distance;
    private Vector3 directionToTarget;

    private Transform target;
    private NavMeshAgent agent;
    private Animator animator;

    private bool isPlayingDeathAnimation = false;

    [SerializeField] protected float hp = 100f;

    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        animator.applyRootMotion = false;
        agent.speed = 1.5f;
    }

    void Update()
    {
        
        distance = Vector3.Distance(target.position, transform.position);
        directionToTarget = (target.position - transform.position).normalized;

        Debug.DrawRay(transform.position, directionToTarget, Color.red);

        if (hp <= 0)
        {
            HandleDeath();
            return;
        }

        if ((distance <= viewRadius && Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2) || 
            distance <= closeDetectionDistance)
        {
            foundTarget = true;
            agent.isStopped = false;
            animator.SetBool("FoundPlayer", true);

            FaceTarget(directionToTarget);
            agent.SetDestination(target.position);
            agent.speed = 3f;

            ResetAttackAnimationTriggers();

            if (distance < agent.stoppingDistance + 0.3f)
            {
                // Inside attacking range, attack player.
                agent.isStopped = true;
                animator.SetBool("InAttackRange", true);
                //MoveLeftOrRight();

                //AttackPlayerRandomly();
            }
            else 
            {
                // Outside attacking range.
                animator.SetBool("InAttackRange", false);
            }
        }
        else 
        {
            foundTarget = false;
            agent.isStopped = true;
            animator.SetBool("FoundPlayer", false);
        }
    }

    private void HandleDeath()
    {
        if (!isPlayingDeathAnimation)
        {
            PlayDeathAnimation();
            isPlayingDeathAnimation = true;
        }

        agent.isStopped = true;

        if ((animator.GetCurrentAnimatorStateInfo(0).IsName("2Hand-Spear-Death1") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting-Death1")) &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void PlayDeathAnimation()
    {
        float random = Random.value;

        if (random >= 0f && random < 0.5f)
        {
            animator.SetTrigger("Die_1");
        }
        else if (random >= 0.5f && random < 1f)
        {
            animator.SetTrigger("Die_2");
        }
    }

    private void MoveLeftOrRight() {
        agent.Move(0.7f * Tangent(directionToTarget) * Time.deltaTime);
        agent.Move(-0.7f * Tangent(directionToTarget) * Time.deltaTime);
        // Continue from here
    }

    private Vector3 Tangent(Vector3 direction) {
        Vector3 tangent = Vector3.Cross(direction, Vector3.up);

        if (tangent.magnitude == 0)
        {
            tangent = Vector3.Cross(direction, Vector3.right);
        }

        return tangent;
    }

    private void ResetAttackAnimationTriggers() {
        animator.ResetTrigger("Attack_1");
        animator.ResetTrigger("Attack_2");
        animator.ResetTrigger("Attack_3");
        animator.ResetTrigger("Attack_4");
    }

    private void AttackPlayerRandomly() {
        float random = Random.value;

        if (random >= 0f && random < 0.25f)
        {
            animator.SetTrigger("Attack_1");
        }
        else if (random >= 0.25f && random < 0.5f)
        {
            animator.SetTrigger("Attack_2");
        }
        else if (random >= 0.5f && random < 0.75f) 
        {
            animator.SetTrigger("Attack_3");
        }
        else if (random >= 0.75f && random < 1f)
        {
            animator.SetTrigger("Attack_4");
        }
    }

    private void FaceTarget(Vector3 direction) { 
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
    }

    public float GetHP()
    {
        return hp;
    }

    // These codes below are used by Eiditor for testing purpose.
    public Vector3 GetTargetPosition()
    {
        return target.position;
    }

    public Transform GetTransform() {
        return transform;
    }

    public float GetViewAngle()
    {
        return viewAngle;
    }

    public float GetViewRadius()
    {
        return viewRadius;
    }

    public float GetCloseDetectionDistance()
    {
        return closeDetectionDistance;
    }

    public bool FoundTarget() {
        return foundTarget;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
