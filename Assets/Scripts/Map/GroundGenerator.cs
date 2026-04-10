using System.Collections.Generic;
using UnityEngine;

namespace XmqqyBackpack
{
    [System.Serializable]
    public class NoiseToGroundMapping
    {
        public float threshold;
        public string defName;
        public float density = 1.0f;
    }

    public class GroundGenerator : MonoBehaviour
    {
        public InfiniteWorld world;
        public List<NoiseToGroundMapping> mappings;
        public int randomSeedOffset = 54321;

        private HashSet<Vector3Int> generatedChunks = new HashSet<Vector3Int>();
        private System.Random localRandom;

        private void Awake()
        {
            int seed = (world != null ? world.worldSeed : 0) + randomSeedOffset;
            localRandom = new System.Random(seed);
        }

        private void OnEnable()
        {
            if (world == null) world = FindObjectOfType<InfiniteWorld>();
            if (world != null)
            {
                world.OnChunkLoaded += HandleChunkLoaded;
                world.OnChunkUnloaded += HandleChunkUnloaded;
            }
        }

        private void OnDisable()
        {
            if (world != null)
            {
                world.OnChunkLoaded -= HandleChunkLoaded;
                world.OnChunkUnloaded -= HandleChunkUnloaded;
            }
        }

        private void HandleChunkLoaded(Vector3Int chunkCoord, float[,] noiseMap)
        {
            if (GroundMapManager.Instance == null)
            {
                Debug.LogWarning($"GroundMapManager.Instance 未就绪，跳过区块 {chunkCoord}");
                return;
            }

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
                    if (!GroundMapManager.Instance.GetAllData().ContainsKey(gridPos))
                    {
                        GroundMapManager.Instance.SetData(gridPos, defName);
                    }
                }
            }
        }

        private void HandleChunkUnloaded(Vector3Int chunkCoord)
        {
            generatedChunks.Remove(chunkCoord);
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