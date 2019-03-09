using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct MySphereCollider : IComponentData
{
    public float radius;
    public float3 centerOffset;
}

public class RaySphereCollisionSystem : ComponentSystem
{
    [BurstCompile]
    public struct CheckRaySpheresIntersection : IJobProcessComponentDataWithEntity<MySphereCollider, Position>
    {
        [ReadOnly] public Ray ray;
        public NativeQueue<Entity>.Concurrent collided;

        public void Execute(Entity entity, int index, [ReadOnly] ref MySphereCollider collider, [ReadOnly] ref Position pos)
        {
            if (CheckIntersection(ray, collider, pos))
            {
                collided.Enqueue(entity);
            }
        }
    }

    public static bool CheckIntersection(Ray ray, MySphereCollider sphere, Position sphereCenter)
    {
        // Find the vector between where the ray starts the the sphere's centre
        float3 center = sphereCenter.Value + sphere.centerOffset;
        float3 difference = center - (float3)ray.origin;

        float differenceLengthSquared = difference.x * difference.x + difference.y * difference.y + difference.z * difference.z;
        float sphereRadiusSquared = sphere.radius * sphere.radius;

        float distanceAlongRay;

        // If the distance between the ray start and the sphere's centre is less than
        // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
        if (differenceLengthSquared < sphereRadiusSquared)
        {
            return true;
        }

        distanceAlongRay = math.dot(ray.direction, difference);

        // If the ray is pointing away from the sphere then we don't ever intersect
        if (distanceAlongRay < 0)
        {
            return false;
        }

        // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
        // if x = radius of sphere
        // if y = distance between ray position and sphere centre
        // if z = the distance we've travelled along the ray
        // if x^2 + z^2 - y^2 < 0, we do not intersect
        float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

        return !(dist < 0);
    }

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            NativeQueue<Entity> collidedEntities = new NativeQueue<Entity>(Allocator.TempJob);
            var checkIntersectionJob = new CheckRaySpheresIntersection()
            {
                collided = collidedEntities.ToConcurrent(),
                ray = mouseRay
            };

            checkIntersectionJob.Schedule(this).Complete();

            while (collidedEntities.Count > 0)
            {
                Debug.Log("Intersected entity: " + collidedEntities.Dequeue());
            }

            collidedEntities.Dispose();
        }
    }
}
