using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<EnemySpawnerComponent>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach(RefRW<EnemySpawnerComponent> spawner in SystemAPI.Query<RefRW<EnemySpawnerComponent>>())
		{
			spawner.ValueRW.Timer += SystemAPI.Time.DeltaTime;
			
			if(spawner.ValueRO.Timer < spawner.ValueRO.SpawnDelay)
			{
				continue;
			}

			Entity newEnemy = state.EntityManager.Instantiate(spawner.ValueRO.PrefabToSpawn);

			LocalTransform newLT = LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition);

			state.EntityManager.SetComponentData(newEnemy, newLT);

			spawner.ValueRW.Timer -= spawner.ValueRO.SpawnDelay;
		}
	}

	private void UpdateSpawner(ref SystemState state, RefRW<EnemySpawnerComponent> spawner)
	{
		// Increment timer by delta
		spawner.ValueRW.Timer += SystemAPI.Time.DeltaTime;

		// If less than Spawn Rate timer, exit early.
		if (spawner.ValueRO.Timer < spawner.ValueRO.SpawnDelay)
			return;

		// Otherwise, spawn an enemy and reset timer.
		SpawnEnemy(ref state, spawner);
		spawner.ValueRW.Timer = 0;
	}

	private void SpawnEnemy(ref SystemState state, RefRW<EnemySpawnerComponent> spawner)
	{
		// Create the entity, passing the prefab to spawn
		Entity entity = state.EntityManager.Instantiate(spawner.ValueRO.PrefabToSpawn);

		// Build local transform this object by making a localtransform component from a position
		LocalTransform localTransform = LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition);

		//Then, set the entity's local transform to the transform we just built.
		state.EntityManager.SetComponentData(entity, localTransform);

	}

}
