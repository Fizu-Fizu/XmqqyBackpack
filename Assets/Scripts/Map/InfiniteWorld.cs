using System.Collections.Generic;
using UnityEngine;


namespace XmqqyBackpack
{
    public class InfiniteWorld : MonoBehaviour
    {
        [Header("世界参数")]
        public int worldSeed = 1028;
        [SerializeField] private float scale = 5f;
        [SerializeField] private float offsetRange = 100000f;

        [Header("区块参数")]
        [SerializeField] private int chunkSize = 8;
        [SerializeField] private int loadRadius = 2;

        [Header("玩家参数")]
        [SerializeField] private Vector3 playerPosition = Vector3.zero;

        // 公开属性供生成器访问
        public int ChunkSize => chunkSize;
        public float Scale => scale;

        // 区块数据缓存：存储原始噪声值 (0~1)
        private Dictionary<Vector3Int, float[,]> chunkNoiseCache = new Dictionary<Vector3Int, float[,]>();
        private HashSet<Vector3Int> loadedChunks = new HashSet<Vector3Int>();
        private Vector3Int lastPlayerChunk;

        // 全局偏移
        private float globalOffsetX;
        private float globalOffsetZ;

        // 区块加载/卸载事件，用于通知生成器
        public System.Action<Vector3Int, float[,]> OnChunkLoaded;
        public System.Action<Vector3Int> OnChunkUnloaded;

        private void Start()
        {
            DataManager.LoadAll();
            Random.InitState(worldSeed);
            globalOffsetX = Random.Range(-offsetRange, offsetRange);
            globalOffsetZ = Random.Range(-offsetRange, offsetRange);

            lastPlayerChunk = GetChunkCoordFromWorldPos(playerPosition);
            LoadChunksAroundCenter(lastPlayerChunk);
        }

        private void Update()
        {
            Vector3Int currentChunk = GetChunkCoordFromWorldPos(playerPosition);
            if (currentChunk != lastPlayerChunk)
            {
                lastPlayerChunk = currentChunk;
                LoadChunksAroundCenter(lastPlayerChunk);
            }
        }

        private Vector3Int GetChunkCoordFromWorldPos(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / chunkSize);
            int z = Mathf.FloorToInt(worldPos.z / chunkSize);
            return new Vector3Int(x, 0, z);
        }

        private void LoadChunksAroundCenter(Vector3Int center)
        {
            int minX = center.x - loadRadius;
            int maxX = center.x + loadRadius;
            int minZ = center.z - loadRadius;
            int maxZ = center.z + loadRadius;

            HashSet<Vector3Int> needed = new HashSet<Vector3Int>();
            for (int x = minX; x <= maxX; x++)
                for (int z = minZ; z <= maxZ; z++)
                    needed.Add(new Vector3Int(x, 0, z));

            // 卸载
            List<Vector3Int> toUnload = new List<Vector3Int>();
            foreach (var chunk in loadedChunks)
                if (!needed.Contains(chunk))
                    toUnload.Add(chunk);
            foreach (var chunk in toUnload)
            {
                OnChunkUnloaded?.Invoke(chunk);
                loadedChunks.Remove(chunk);
                chunkNoiseCache.Remove(chunk);
            }

            // 加载
            foreach (var chunk in needed)
            {
                if (!loadedChunks.Contains(chunk))
                {
                    if (!chunkNoiseCache.ContainsKey(chunk))
                        chunkNoiseCache[chunk] = GenerateChunkNoise(chunk);
                    loadedChunks.Add(chunk);
                    OnChunkLoaded?.Invoke(chunk, chunkNoiseCache[chunk]);
                }
            }
        }

        /// <summary> 生成区块内每个格子的原始噪声值 (0~1) </summary>
        private float[,] GenerateChunkNoise(Vector3Int chunkCoord)
        {
            float[,] noiseMap = new float[chunkSize, chunkSize];
            float originX = chunkCoord.x * chunkSize;
            float originZ = chunkCoord.z * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float worldX = originX + x;
                    float worldZ = originZ + z;
                    float noise = Mathf.PerlinNoise((worldX / scale) + globalOffsetX,
                                                    (worldZ / scale) + globalOffsetZ);
                    noiseMap[x, z] = noise;
                }
            }
            return noiseMap;
        }

        /// <summary> 公开方法：获取某个世界坐标的原始噪声值 </summary>
        public float GetNoiseAtWorldPos(Vector3 worldPos)
        {
            int chunkX = Mathf.FloorToInt(worldPos.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(worldPos.z / chunkSize);
            Vector3Int chunkCoord = new Vector3Int(chunkX, 0, chunkZ);

            if (chunkNoiseCache.TryGetValue(chunkCoord, out float[,] noiseMap))
            {
                int localX = Mathf.Abs(Mathf.FloorToInt(worldPos.x) - chunkX * chunkSize);
                int localZ = Mathf.Abs(Mathf.FloorToInt(worldPos.z) - chunkZ * chunkSize);
                if (localX >= 0 && localX < chunkSize && localZ >= 0 && localZ < chunkSize)
                    return noiseMap[localX, localZ];
            }
            return 0f;
        }

        /// <summary> 外部调用：更新玩家位置 </summary>
        public void SetPlayerPosition(Vector3 newPos)
        {
            playerPosition = newPos;
        }
    }
}