using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    // Component Lookup for Entities. Allows them to lookup player's position at run-time.
    // It does this by passing in an entity and returning their LocalToWorld.
    private ComponentLookup<LocalToWorld> localToWorldLookup;

    public void OnCreate(ref SystemState state)
    {
        // Create the lookup
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
    }


    public void OnUpdate(ref SystemState state)
    {
        // Update the lookup
        localToWorldLookup.Update(ref state);

        EnemySpawnerUpdateJob updateJob = new EnemySpawnerUpdateJob
        {
            // Pass DeltaTime
            deltaTime = SystemAPI.Time.DeltaTime
        };

        // Schedule the job to run in parallel, unity does a lot of the heavy lifting with making it run in parallel.
        updateJob.ScheduleParallel();
    }
}
