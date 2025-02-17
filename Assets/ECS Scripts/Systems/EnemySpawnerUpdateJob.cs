using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]

public partial struct EnemySpawnerUpdateJob : IJobEntity
{
    // Pass in The Delta time
    public float deltaTime;
    public void Execute(ref EnemySpawnerComponent enemySpawner)
    {
        //Increase the timer by deltatime
        enemySpawner.Timer += deltaTime;

        // If less than spawn rate, quit early.
        if (enemySpawner.Timer < enemySpawner.SpawnDelay)
            return;

        //Else, Reset the timer.
        enemySpawner.Timer = 0;

        //And spawn the enemy
        //Put code below.
    }
}
