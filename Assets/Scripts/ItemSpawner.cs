using UnityEngine;
using static GameConstants;

/// <summary> Periodically spawns random items into the scene up to a defined limit. </summary>
public class ItemSpawner : MonoBehaviour
{
    [field: SerializeField] public int SpawnedItemsCount { get; private set; }
    [SerializeField] GameObject[] ItemPrefabs;
    [SerializeField] float spawnCD;

    void Update() => SpawnItem();

    // Spawns a random item at a random offset position above the spawner
    void SpawnItem()
    {
        if (SpawnedItemsCount >= MaxSpawnedItems) return;
        spawnCD -= Time.deltaTime;

        if (spawnCD <= 0)
        {
            spawnCD = Random.Range(SpawnCDMin, SpawnCDMax); // Random time interval for spawning items
            Vector3 randomPosition = new Vector3(Random.Range(-6, 6), Random.Range(2, 6), Random.Range(-6, 6));
            Instantiate(ItemPrefabs[Random.Range(0, ItemPrefabs.Length)], transform.position + randomPosition, Quaternion.identity, this.transform);
            SpawnedItemsCount++;

            // Fully disable this script after all items are spawned
            if (SpawnedItemsCount == MaxSpawnedItems)
            {
                Utils.SendLogMessage("All the items have been spawned".Colored("yellow"));
                this.enabled = false;
            }
        }

    }
}