using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
    public GameObject PrefabToSpawn;
    public float SpawnDelay;

    private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            //Convert it to an entity.
            //'UsageFlags.None' means this won't move at spawn-time, as the spawner is static.
            Entity e = GetEntity(TransformUsageFlags.None);

            AddComponent(e, new EnemySpawnerComponent
            {
                //Convert prefab into an entity, dynamic flag means it can move at runtime.
                PrefabToSpawn = GetEntity(authoring.PrefabToSpawn, TransformUsageFlags.Dynamic),

                //Set properties to the below values.
                Timer = 0.0f,
                SpawnDelay = authoring.SpawnDelay,
                SpawnPosition = authoring.transform.position
            });
        }
    }
}