using Unity.Entities;
using UnityEngine;


public class EnemyAuthoring : MonoBehaviour
{
    // Public fields to set enemy attributes in the Unity Editor
    public float MoveSpeed;
    public float Damage;
    public float MaxHealth;

    // Nested Baker class to convert the MonoBehaviour to an ECS entity
    private class Baker : Baker<EnemyAuthoring>
    {
        // Override the Bake method to define the conversion process
        public override void Bake(EnemyAuthoring authoring)
        {
            // Convert the GameObject to an ECS entity
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            // Add EnemyComponent to the entity with values from the authoring MonoBehaviour
            AddComponent(e, new EnemyComponent
            {
                MoveSpeed = authoring.MoveSpeed,
                Damage = authoring.Damage
            });

            // Add HealthComponent to the entity with initial health values
            AddComponent(e, new HealthComponent { MaxHealth = authoring.MaxHealth, CurrentHealth = authoring.MaxHealth });

            // Add a buffer to store damage instances for the entity
            AddBuffer<DamageBuffer>(e);

            // Add a PendingKillComponent to the entity to mark it for removal when health reaches zero
            AddComponent<PendingKillComponent>(e);

            // Disable the PendingKillComponent initially
            SetComponentEnabled<PendingKillComponent>(e, false);
        }
    }
}

// Struct to store enemy attributes as an ECS component
public struct EnemyComponent : IComponentData
{
    public float MoveSpeed;
    public float Damage;
}