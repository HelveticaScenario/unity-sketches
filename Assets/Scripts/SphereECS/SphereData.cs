using System;
using Unity.Entities;
using Unity.Mathematics;
namespace SphereECS
{
    [Serializable]
    public struct SphereData : IComponentData
    {
        public float2 pos;
    }

}
