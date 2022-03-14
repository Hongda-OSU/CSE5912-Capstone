using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CSE5912.PolyGamers
{
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        [Header("Enemy Properties")]
        [SerializeField] protected string enemyName;
        [SerializeField] protected int level;
        [SerializeField] protected float experience;

        [SerializeField] protected float health;
        [SerializeField] protected float maxHealth;

        [SerializeField] protected float attackDamage;
        [SerializeField] protected float attackRange = 5f;

        [SerializeField] protected float agentSpeed = 0f;
        [SerializeField] protected bool isAlive = true;
        [SerializeField] protected bool isFrozen = false;

        protected Burned burned;
        protected Frozen frozen;
        protected Electrocuted electrocuted;
        protected Infected infected;

        [Header("Damage")]
        [SerializeField] private float damageFactor_physical = 1f;
        [SerializeField] private float damageFactor_fire = 1f;
        [SerializeField] private float damageFactor_cryo = 1f;
        [SerializeField] private float damageFactor_electro = 1f;
        [SerializeField] private float damageFactor_venom = 1f;
        private DamageFactor damageFactor;

        [Header("Resists")]
        [SerializeField] private float physicalResist = 0f;
        [SerializeField] private float fireResist = 0f;
        [SerializeField] private float cryoResist = 0f;
        [SerializeField] private float electroResist = 0f;
        [SerializeField] private float venomResist = 0f;
        protected Resist resist;

        [Header("Random Dropoff")]
        [SerializeField] private float dropWeaponChance;
        [SerializeField] private Firearms.WeaponType dropWeaponType = Firearms.WeaponType.AK47;
        [SerializeField] private Firearms.WeaponRarity dropWeaponRarity = Firearms.WeaponRarity.Divine;
        [SerializeField] private float dropAttachmentChance;
        [SerializeField] private Attachment.AttachmentType dropAttachmentType = Attachment.AttachmentType.Scope;
        [SerializeField] private Attachment.AttachmentRarity dropAttachmentRarity = Attachment.AttachmentRarity.Divine;

        [Header("Certain Dropoff")]
        // todo
        //[SerializeField] private

        [Header("Other")]
        [SerializeField] protected float rotateSpeed = 20f;
        [SerializeField] protected float viewRadius = 15f;
        [Range(0, 360)]
        [SerializeField] protected float viewAngle = 135f;
        [SerializeField] protected float closeDetectionRange = 3f;
        [SerializeField] protected float timeToDestroy = 5f;

        public bool playerDetected = false;
        protected bool foundTarget = false;
        protected bool isPlayingDeathAnimation = false;
        protected bool isAttackedByPlayer = false;

        protected float distanceToPlayer;
        protected Vector3 directionToPlayer;

        protected Transform player;
        protected Animator animator;
        protected NavMeshAgent agent;
        protected Collider collider3d;

        protected virtual void Update()
        {
            distanceToPlayer = Vector3.Distance(player.position, transform.position);
            directionToPlayer = (player.position - transform.position).normalized;

        }

        protected void Initialize()
        {
            player = PlayerManager.Instance.Player.transform;

            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            collider3d = GetComponent<Collider>();

            agent.speed = agentSpeed;

            burned = GetComponentInChildren<Burned>();
            frozen = GetComponentInChildren<Frozen>();
            electrocuted = GetComponentInChildren<Electrocuted>();
            infected = GetComponentInChildren<Infected>();

            damageFactor = new DamageFactor();
            damageFactor.SetDamageValues(damageFactor_physical, damageFactor_fire, damageFactor_cryo, damageFactor_electro, damageFactor_venom);

            resist = new Resist();
            resist.SetValues(physicalResist, fireResist, cryoResist, electroResist, venomResist);
        }

        public virtual void TakeDamage(Damage damage)
        {
            if (!isAlive)
            {
                return;
            }

            float value = damage.ResolvedValue;

            health -= value;

            if (health <= 0)
            {
                Die();

                //test
                DropWeapon();
                DropAttachment();
            }

            if (!isAttackedByPlayer)
            {
                isAttackedByPlayer = true;
            }
        }

        protected virtual void Die()
        {
            // remove enemy from enemy list and destroy
            isAlive = false;
            agent.isStopped = true;
            
            collider3d.enabled = false;

            StartCoroutine(RemoveAndDestroy(gameObject, timeToDestroy));

            PlayDeathAnimation();
        }
        private IEnumerator RemoveAndDestroy(GameObject gameObject, float time)
        {
            yield return new WaitForSeconds(time);
            EnemyManager.Instance.EnemyList.Remove(gameObject);
            Destroy(gameObject);
        }

        protected void DropWeapon()
        {
            if (Random.value < dropWeaponChance)
                return;

            DropoffManager.Instance.DropWeapon(dropWeaponType, dropWeaponRarity, transform.position);
        }

        protected void DropAttachment()
        {
            if (Random.value < dropAttachmentChance)
                return;

            DropoffManager.Instance.DropAttachment(dropAttachmentType, dropAttachmentRarity, transform.position);
        }



        public IEnumerator Freeze(float time)
        {
            isFrozen = true;
            agent.isStopped = true;
            agent.speed = 0 ;
            animator.speed = 0;

            yield return new WaitForSeconds(time);

            isFrozen = false;
            agent.isStopped = false;
            agent.speed = agentSpeed;
            animator.speed = 1;

            frozen.Stack = 0;
        }
        public void SlowDown(float percentage)
        {
            if (!isFrozen)
            {
                agent.speed = agentSpeed * (1 - percentage);
                animator.speed = 1 - percentage;
            }
        }

        public void StackDebuff(Element.Type type)
        {
            switch (type)
            {
                case Element.Type.Fire:
                    burned.StackUp();
                    break;
                case Element.Type.Cryo:
                    frozen.StackUp();
                    break;
                case Element.Type.Electro:
                    electrocuted.StackUp();
                    break;
                case Element.Type.Venom:
                    infected.StackUp();
                    break;
                default:
                    break;
            }
        }
        public DamageFactor GetDamageFactor()
        {
            return damageFactor;
        }
        public Resist GetResist()
        {
            return resist;
        }

        public float ComputeExtraDamage(float baseValue)
        {
            return 0f;
        }


        protected virtual void FaceTarget(Vector3 direction)
        {
            Quaternion lookRotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, directionToPlayer, rotateSpeed * Time.deltaTime, 0.0f));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }


        /*
         * used by animation event
         */

        protected abstract void PlayDeathAnimation();

        protected virtual void Hit()
        {

        }
        protected virtual void FootL()
        {

        }
        protected virtual void FootR()
        {

        }

        public string EnemyName { get { return enemyName; } }
        public int Level { get { return level; } }
        public float Experience { get { return experience;} }
        public float Health { get { return health;} }
        public float MaxHealth { get { return maxHealth;} }
        public float AttackDamage { get { return attackDamage;} }
        public bool IsAttackedByPlayer { get { return isAttackedByPlayer; } }
        public bool IsAlive { get { return isAlive; } }
        public bool IsFrozen { get { return isFrozen; } }

        public float PhysicalResist { get { return physicalResist;} }
        public float FireResist { get{ return fireResist;} }
        public float CryoResist { get { return cryoResist;} }
        public float ElectroResist { get { return electroResist;} }
        public float VenomResist { get { return venomResist;} }

        public Burned Burned { get { return burned; } }
        public Frozen Frozen { get { return frozen; } }
        public Electrocuted Electrocuted { get { return electrocuted; } }
        public Infected Infected { get { return infected; } }

        public float DistanceToPlayer { get { return distanceToPlayer; } }
        public Vector3 DirectionToPlayer { get { return directionToPlayer; } }
    }
}
