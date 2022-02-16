using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject enemies;

        private List<GameObject> enemyList;

        private static EnemyManager instance;
        public static EnemyManager Instance { get { return instance; } }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(instance);
            }
            instance = this;

            enemyList = new List<GameObject>();
            foreach (Transform enemy in enemies.transform)
                enemyList.Add(enemy.gameObject);
        }

        public List<GameObject> GetEnemiesInView()
        {
            List<GameObject> result = new List<GameObject>();

            var planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
            foreach (GameObject enemy in enemyList)
            {
                if (enemy != null && GeometryUtility.TestPlanesAABB(planes, enemy.GetComponent<Collider>().bounds))
                {
                    result.Add(enemy);
                }
            }
            return result;
        }
    }
}
