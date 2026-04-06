using System.Collections.Generic;
using UnityEngine;

namespace XmqqyBackpack
{
    [System.Serializable]
    public class NoiseToObjectMapping
    {
        [Tooltip("噪声值 >= 该阈值时使用此物体（列表应按阈值从高到低排列）")]
        public float threshold;
        public string defName;
        [Tooltip("生成密度（0-1），只有随机值 <= 密度时才生成")]
        public float density = 1.0f;
    }

    public class ObjectGenerator : MonoBehaviour
    {
        public InfiniteWorld world;
        public List<NoiseToObjectMapping> mappings;
        public int randomSeedOffset = 12345;

        private HashSet<Vector3Int> generatedChunks = new HashSet<Vector3Int>();

        private void OnEnable()
        {
            if (world == null) world = FindObjectOfType<InfiniteWorld>();
            if (world != null)
                world.OnChunkLoaded += HandleChunkLoaded;
        }

        private void OnDisable()
        {
            if (world != null)
                world.OnChunkLoaded -= HandleChunkLoaded;
        }

        private void HandleChunkLoaded(Vector3Int chunkCoord, float[,] noiseMap)
        {
            if (generatedChunks.Contains(chunkCoord)) return;
            generatedChunks.Add(chunkCoord);

            int chunkSize = world.ChunkSize;
            int originX = chunkCoord.x * chunkSize;
            int originZ = chunkCoord.z * chunkSize;

            int chunkSeed = chunkCoord.GetHashCode() + randomSeedOffset;
            System.Random chunkRandom = new System.Random(chunkSeed);

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float noise = noiseMap[x, z];
                    string defName = GetDefNameForNoise(noise);
                    if (string.IsNullOrEmpty(defName)) continue;

                    float density = GetDensityForDef(defName);
                    if (chunkRandom.NextDouble() > density) continue;

                    Vector3Int gridPos = new Vector3Int(originX + x, originZ + z, 0);
                    if (!ObjectMapManager.Instance.GetAllData().ContainsKey(gridPos))
                    {
                        ObjectMapManager.Instance.SetData(gridPos, defName);
                    }
                }
            }
        }

        private string GetDefNameForNoise(float noise)
        {
            foreach (var m in mappings)
                if (noise >= m.threshold) return m.defName;
            return null;
        }

        private float GetDensityForDef(string defName)
        {
            foreach (var m in mappings)
                if (m.defName == defName) return m.density;
            return 1.0f;
        }
    }
}