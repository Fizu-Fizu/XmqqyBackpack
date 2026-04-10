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

        // 公开属性
        public int ChunkSize => chunkSize;
        public float Scale => scale;

        // 区块数据缓存：存储原始噪声值 (0~1)
        private Dictionary<Vector3Int, float[,]> chunkNoiseCache = new Dictionary<Vector3Int, float[,]>();
        private HashSet<Vector3Int> loadedChunks = new HashSet<Vector3Int>();

        // 兴趣点字典：key -> 世界坐标
        private Dictionary<string, Vector3> interestPoints = new Dictionary<string, Vector3>();
        private bool needRefreshChunks = true;   // 标记是否需要重新计算区块

        // 全局偏移
        private float globalOffsetX;
        private float globalOffsetZ;

        // 区块加载/卸载事件
        public System.Action<Vector3Int, float[,]> OnChunkLoaded;
        public System.Action<Vector3Int> OnChunkUnloaded;

        private void Start()
        {
            DataManager.LoadAll();
            Random.InitState(worldSeed);
            globalOffsetX = Random.Range(-offsetRange, offsetRange);
            globalOffsetZ = Random.Range(-offsetRange, offsetRange);

            needRefreshChunks = true;
        }

        private void Update()
        {
            if (needRefreshChunks)
            {
                RefreshChunks();
                needRefreshChunks = false;
            }
        }

        /// <summary>
        /// 注册或更新一个兴趣点的位置
        /// </summary>
        public void RegisterInterestPoint(string key, Vector3 position)
        {
            if (interestPoints.ContainsKey(key))
                interestPoints[key] = position;
            else
                interestPoints.Add(key, position);
            needRefreshChunks = true;
        }

        /// <summary>
        /// 注销一个兴趣点
        /// </summary>
        public void UnregisterInterestPoint(string key)
        {
            if (interestPoints.Remove(key))
                needRefreshChunks = true;
        }

        /// <summary>
        /// 兼容旧 API：设置玩家位置（默认 key 为 "Player"）
        /// </summary>
        public void SetPlayerPosition(Vector3 newPos)
        {
            RegisterInterestPoint("Player", newPos);
        }

        /// <summary>
        /// 根据当前所有兴趣点，重新计算需要加载的区块并更新
        /// </summary>
        private void RefreshChunks()
        {
            if (interestPoints.Count == 0) return;

            // 收集所有兴趣点周围需要加载的区块
            HashSet<Vector3Int> neededChunks = new HashSet<Vector3Int>();
            foreach (var point in interestPoints.Values)
            {
                Vector3Int centerChunk = GetChunkCoordFromWorldPos(point);
                for (int x = -loadRadius; x <= loadRadius; x++)
                {
                    for (int z = -loadRadius; z <= loadRadius; z++)
                    {
                        neededChunks.Add(new Vector3Int(centerChunk.x + x, 0, centerChunk.z + z));
                    }
                }
            }

            // 卸载不再需要的区块
            List<Vector3Int> toUnload = new List<Vector3Int>();
            foreach (var chunk in loadedChunks)
                if (!neededChunks.Contains(chunk))
                    toUnload.Add(chunk);
            foreach (var chunk in toUnload)
            {
                OnChunkUnloaded?.Invoke(chunk);
                loadedChunks.Remove(chunk);
                chunkNoiseCache.Remove(chunk);
            }

            // 加载新需要的区块
            foreach (var chunk in neededChunks)
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

        private Vector3Int GetChunkCoordFromWorldPos(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / chunkSize);
            int z = Mathf.FloorToInt(worldPos.z / chunkSize);
            return new Vector3Int(x, 0, z);
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
    }
}