using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSE5912.PolyGamers
{
    public class Teleporter : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [SerializeField] private float animationTime = 5f;

        [SerializeField] private AudioSource risingSound;
        [SerializeField] private AudioSource shootingSound;

        [SerializeField] private Transform finalPivot;
        [SerializeField] private Transform startPivot;
        [SerializeField] private Transform powerPivot;

        [SerializeField] private Mesh inactiveMesh;
        [SerializeField] private Mesh activeMesh;

        [SerializeField] private GameObject powerPrefab;
        [SerializeField] private GameObject circlePrefab;
        [SerializeField] private GameObject portalPrefab;

        private bool isActivated = false;
        private bool isUsed = false;

        private MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = inactiveMesh;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Player" || isActivated)
                return;

            StartCoroutine(PlayActivateAnimation());

        }
        private void OnTriggerStay(Collider other)
        {
            if (other.tag != "Player" || !isActivated || isUsed)
                return;

            if (InputManager.Instance.InputSchemes.PlayerActions.Interact.triggered)
                StartCoroutine(TeleportPlayer(target.position));
        }

        private IEnumerator TeleportPlayer(Vector3 position)
        {
            isUsed = true;

            var player = PlayerManager.Instance.Player;

            GameObject portal = Instantiate(portalPrefab);
            portal.transform.position = player.transform.position - Vector3.up * 5f;

            GameStateController.Instance.SetGameState(GameStateController.GameState.Loading);
            yield return new WaitForSeconds(3f);
            player.transform.position = position;
            GameStateController.Instance.SetGameState(GameStateController.GameState.InGame);

            Destroy(portal, 1f);
            isUsed = false;
        }

        private IEnumerator PlayActivateAnimation()
        {
            GameObject circle = Instantiate(circlePrefab);
            circle.transform.position = startPivot.position;

            risingSound.Play();

            Vector3 startPosition = transform.position;
            Vector3 endPosition = finalPivot.position;

            float timeSince = 0f;
            while (timeSince < animationTime)
            {
                transform.position = Vector3.Slerp(startPosition, endPosition, timeSince / animationTime);

                timeSince += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            risingSound.Stop();

            GameObject power = Instantiate(powerPrefab);
            power.transform.position = powerPivot.position;

            yield return new WaitForSeconds(4.5f);

            meshFilter.mesh = activeMesh;
            shootingSound.Play();

            Destroy(circle, 5f);
            Destroy(power, 5f);

            isActivated = true;
        }

    }
}
