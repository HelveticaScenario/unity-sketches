using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SphereECS
{
    public class SphereSpawner : MonoBehaviour
    {
        public int Width = 100;
        public int Height = 100;
        public float waveHeight = 1.0f;
        public float density = 0.5f;
        public float density2 = 3.0f;
        public Vector2 center = new Vector2(50, 50);
        public GameObject Prefab;
        EntityManager manager;

        void Start()
        {
            manager = World.Active.GetOrCreateManager<EntityManager>();
            Entity entityPrefab = manager.Instantiate(Prefab);
            print(entityPrefab);
            // var instance = manager.Instantiate(entityPrefab);
            // print(instance);

            // // Create entity prefab from the game object hierarchy once
            // Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, World.Active);
            // var entityManager = World.Active.EntityManager;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Efficiently instantiate a bunch of entities from the already converted entity prefab
                    var instance = manager.Instantiate(entityPrefab);

                    // Place the instantiated entity in a grid with some noise
                    // var position = transform.TransformPoint(new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));
                    var position = new float3(x*0.5f, 0f, y*0.5f);
                    manager.SetComponentData(instance, new Position { Value = position });
                    manager.SetComponentData(instance, new SphereData { pos = new float2(x, y) });
                }
            }
            manager.DestroyEntity(entityPrefab);

        }
    }
}

