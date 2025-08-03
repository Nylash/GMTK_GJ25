using UnityEngine;

public class CloudsMover : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float speed;

    private void Start()
    {
        // Initial object at x = -1
        SpawnAndMove(-1f);
    }

    void SpawnAndMove(float startX)
    {
        GameObject obj = Instantiate(prefab, new Vector3(startX, prefab.transform.position.y, prefab.transform.position.z), Quaternion.identity);
        StartCoroutine(MoveAndManage(obj));
    }

    System.Collections.IEnumerator MoveAndManage(GameObject obj)
    {
        bool hasSpawnedNext = false;

        while (obj.transform.position.x < 78f)
        {
            obj.transform.position += new Vector3(speed, 0, 0);

            if (!hasSpawnedNext && obj.transform.position.x >= 0f)
            {
                hasSpawnedNext = true;
                SpawnAndMove(-75f);
            }

            yield return null;
        }

        Destroy(obj);
    }
}
