using System;
using Unity.Entities;
namespace SphereECS
{
    [Serializable]
    public struct SphereData : IComponentData
    {
        public int x;
        public int y;
    }

}
