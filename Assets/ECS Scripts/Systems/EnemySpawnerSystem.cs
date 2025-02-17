using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    // Lookup for player transforms.
    private ComponentLookup<LocalToWorld> localToWorldLookup;

    public void OnCreate(ref SystemState state)
    {
        // Create a read-only lookup for LocalToWorld.
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        // Update the lookup.
        localToWorldLookup.Update(ref state);

        // Use an ECB to safely create entities from the job.
        using (var ecb = new EntityCommandBuffer(Allocator.TempJob))
        {
            var job = new EnemySpawnerUpdateJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                localToWorldLookup = localToWorldLookup,
                ecb = ecb.AsParallelWriter()
            };

            // Schedule the job to run in parallel.
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            // Playback the ECB to actually perform the commands.
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnerUpdateJob : IJobEntity
    {
        public float deltaTime;
        [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
        public Entity playerEntity;
        public EntityCommandBuffer.ParallelWriter ecb;

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            // Query for the player entity
            var playerQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>());
            if (playerQuery.CalculateEntityCount() > 0)
            {
                playerEntity = playerQuery.GetSingletonEntity();
            }
        }

        public void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, ref EnemySpawnerComponent enemySpawner)
        {
            // Increase the timer.
            enemySpawner.Timer += deltaTime;

            // If the spawn delay hasn't been reached, exit early.
            if (enemySpawner.Timer < enemySpawner.SpawnDelay)
                return;

            // Reset the timer.
            enemySpawner.Timer = 0f;

            // Spawn an enemy using the prefab provided in EnemySpawnerComponent.
            Entity newEnemy = ecb.Instantiate(entityInQueryIndex, enemySpawner.PrefabToSpawn);

            // Build a LocalTransform from the spawn position.
            LocalTransform localTransform = LocalTransform.FromPosition(enemySpawner.SpawnPosition);
            ecb.SetComponent(entityInQueryIndex, newEnemy, localTransform);

            // Lookup the player's transform.
            if (localToWorldLookup.HasComponent(playerEntity))
            {
                LocalToWorld playerL2W = localToWorldLookup[playerEntity];
                // You can use playerL2W as needed.
            }
        }
    }
}
