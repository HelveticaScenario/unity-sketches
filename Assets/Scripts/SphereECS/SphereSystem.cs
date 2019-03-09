using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace SphereECS
{
    public class SphereSystem : JobComponentSystem
    {

        struct SphereScaleJob : IJobProcessComponentData<Scale, SphereData>
        {
            public float time;
            public int width;
            public int height;
            public Vector2 center;
            public void Execute(ref Scale scale, [ReadOnly] ref SphereData sphereData)
            {

            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new SphereScaleJob()
            {
                time = Time.realtimeSinceStartup,
                width = 100,
                height = 100,
                center = new Vector2Int(50, 50)
            };

            return job.Schedule(this, inputDependencies);
        }
    }

}

