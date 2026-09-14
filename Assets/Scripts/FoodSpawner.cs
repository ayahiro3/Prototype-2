using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private Vector2 spawnBoundsMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 spawnBoundsMax = new Vector2(8f, 4f);
    [SerializeField] private int maxFoodOnScreen = 10;
    [SerializeField] private string foodTag = "Food";

    private float nextSpawnTime;

    private void Update()
    {
        if (Time.time < nextSpawnTime) return;

        nextSpawnTime = Time.time + spawnInterval;

        if (GameObject.FindGameObjectsWithTag(foodTag).Length >= maxFoodOnScreen)
        {
            return;
        }

        SpawnFood();
    }

    private void SpawnFood()
    {
        Vector2 spawnPos = new Vector2(
            Random.Range(spawnBoundsMin.x, spawnBoundsMax.x),
            Random.Range(spawnBoundsMin.y, spawnBoundsMax.y)
        );

        Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }
}