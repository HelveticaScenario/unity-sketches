using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;


namespace SphereECS
{
    public class SphereSystem : JobComponentSystem
    {
        SphereSpawner spawner;

        struct SphereAnimationJob : IJobProcessComponentData<Position, SphereData>
        {
            public float3 tCenter;
            public float waveHeight;
            public float density;
            public float density2;
            public float time;
            public int width;
            public int height;
            public float2 center;
            public void Execute(ref Position position, [ReadOnly] ref SphereData sphereData)
            {
                var dist = math.distance(center, sphereData.pos);
                float h = math.sin((dist / density2) + time) * dist / waveHeight;
                position.Value = new float3(sphereData.pos.x * density, h, sphereData.pos.y * density);
                // if (sphereData.x == center.x && sphereData.y == center.y)
                // {
                //     position.Value = new float3(sphereData.x * 1.3F, 1f, sphereData.y * 1.3F);
                // }
                // else
                // {
                //     position.Value = new float3(sphereData.x * 1.3F, 0f, sphereData.y * 1.3F);
                //     // position.Value = new float3(1.0f, 1.0f, 1.0f);
                // }
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            if (spawner == null)
            {
                spawner = GameObject.FindObjectOfType<SphereSpawner>();
                Debug.Log(spawner);
            }
            var job = new SphereAnimationJob()
            {
                tCenter = spawner.transform.position,
                time = Time.realtimeSinceStartup,
                width = spawner.Width,
                height = spawner.Height,
                center = spawner.center,
                density = spawner.density,
                density2 = spawner.density2,
                waveHeight = spawner.waveHeight,
            };

            return job.Schedule(this, inputDependencies);
        }
    }

}

