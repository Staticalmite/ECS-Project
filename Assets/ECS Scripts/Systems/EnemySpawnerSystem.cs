using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    // Lookup for LocalToWorld component to track player position
    private ComponentLookup<LocalToWorld> localToWorldLookup;

    public void OnCreate(ref SystemState state)
    {
        // Initialize the lookup for LocalToWorld (read-only)
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        // Update the LocalToWorld lookup before use
        localToWorldLookup.Update(ref state);

        // Find the player entity (assumes there is only one player entity)
        Entity playerEntity = Entity.Null; // Default to null if player is not found
        var playerQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>());

        if (!playerQuery.IsEmpty)
        {
            playerEntity = playerQuery.GetSingletonEntity(); // Retrieve the player entity
        }

        // Create an EntityCommandBuffer for safely modifying entities in a job
        using (var ecb = new EntityCommandBuffer(Allocator.TempJob))
        {
            // Define the job for updating enemy spawners
            var job = new EnemySpawnerUpdateJob
            {
                deltaTime = SystemAPI.Time.DeltaTime, // Pass deltaTime
                localToWorldLookup = localToWorldLookup, // Pass lookup for player transform
                ecb = ecb.AsParallelWriter(), // Use ParallelWriter for safe multi-threading
                playerEntity = playerEntity // Pass the player entity reference
            };

            // Schedule the job in parallel, ensuring proper dependency management
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete(); // Ensure the job completes before proceeding

            // Apply all recorded entity modifications
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public partial struct EnemySpawnerUpdateJob : IJobEntity
    {
        public float deltaTime; // Time since the last frame update
        [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup; // Read-only lookup for player transform
        public Entity playerEntity; // Reference to the player entity
        public EntityCommandBuffer.ParallelWriter ecb; // ECB for parallel entity modifications

        public void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, ref EnemySpawnerComponent enemySpawner)
        {
            // Increase the timer for the spawner
            enemySpawner.Timer += deltaTime;

            // If the timer hasn't reached the spawn delay, exit early
            if (enemySpawner.Timer < enemySpawner.SpawnDelay)
                return;

            // Reset the timer after spawning an enemy
            enemySpawner.Timer = 0f;

            // Instantiate a new enemy using the prefab stored in the spawner component
            Entity newEnemy = ecb.Instantiate(entityInQueryIndex, enemySpawner.PrefabToSpawn);

            // Set the new enemy's position to the spawner's predefined spawn location
            LocalTransform localTransform = LocalTransform.FromPosition(enemySpawner.SpawnPosition);
            ecb.SetComponent(entityInQueryIndex, newEnemy, localTransform);

            // Ensure the player entity exists before accessing its transform
            if (playerEntity != Entity.Null && localToWorldLookup.HasComponent(playerEntity))
            {
                // Get the player's current world transform (not used yet but can be used for AI movement)
                if (playerEntity != Entity.Null && localToWorldLookup.HasComponent(playerEntity))
                {
                    LocalToWorld playerL2W = localToWorldLookup[playerEntity];
                }

                // Example: Use playerL2W.Position to direct enemies toward the player
            }

            // Add an EnemyComponent to the spawned entity with movement speed
            ecb.AddComponent(entityInQueryIndex, newEnemy, new EnemyComponent { MoveSpeed = 3.5f , Damage = 2.0f});
        }
    }
}
