using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class SwordZone : EnemySkill
    {
        [SerializeField] private GameObject swordZonePrefab;
        [SerializeField] private float size;
        //[SerializeField] private GameObject swordPrefab;

        //[SerializeField] private float delay;
        //[SerializeField] private float speed;
        //[SerializeField] private float radius;
        //[SerializeField] private float height;
        //[SerializeField] private float offset;
        //[SerializeField] private int row;
        //[SerializeField] private int numberPerRow;

        //[SerializeField] private LayerMask layerMask;

        public override IEnumerator Perform()
        {
            GameObject swordZone = Instantiate(swordZonePrefab);

            swordZone.GetComponent<Damager_collision>().Initialize(enemy);

            swordZone.transform.position = enemy.transform.position;

            var scale = Vector3.one * size;
            swordZone.transform.localScale = scale;
            foreach (Transform child in swordZone.transform)
                child.transform.localScale = scale;


            var main = swordZone.GetComponent<ParticleSystem>().main;
            float totalDuration = main.duration + main.startLifetime.constant;
            Destroy(swordZone, totalDuration / 2);

            yield return null;
        }
        //    GameObject go = new GameObject("SwordZone");

        //    var origin = transform.position;
        //    var degree = 360f / numberPerRow;

        //    for (int j = 0; j < row; j++)
        //    {
        //        var r = radius + row * j * offset;

        //        for (int i = 0; i < numberPerRow; i++)
        //        {
        //            var radian = Mathf.Deg2Rad * i * degree;
        //            var x = Mathf.Cos(radian) * r;
        //            var z = Mathf.Sin(radian) * r;

        //            var position = new Vector3(x, height, z) + origin;

        //            GameObject sword = Instantiate(swordPrefab, go.transform);
        //            sword.GetComponent<Damager_collision>().Initialize(enemy);
        //            sword.transform.position = position;
                    
        //            StartCoroutine(Drop(sword));

        //        }
        //    }
        //    yield return null;
        //}

        //private IEnumerator Drop(GameObject sword)
        //{
        //    yield return new WaitForSeconds(Random.Range(0f, 0.3f));

        //    var position = sword.transform.position;
        //    if (Physics.Raycast(sword.transform.position, Vector3.down, out RaycastHit hit, layerMask))
        //    {
        //        position = hit.point + Vector3.up * sword.GetComponent<Renderer>().bounds.size.y / 2;
        //    }

        //    yield return new WaitForSeconds(delay);

        //    while (Vector3.Distance(position, sword.transform.position) > 0.01f)
        //    {
        //        sword.transform.position = Vector3.MoveTowards(sword.transform.position, position, speed * Time.deltaTime);
        //        yield return new WaitForSeconds(Time.deltaTime);
        //    }

        //    sword.GetComponent<Damager_collision>().enabled = false;

        //    Destroy(sword, 5f);
        //}
        public override bool IsPerformingAllowed()
        {
            return true;
        }
    }
}
