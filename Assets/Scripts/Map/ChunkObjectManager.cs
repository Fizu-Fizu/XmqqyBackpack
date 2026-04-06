using UnityEngine;
using System.Collections;

namespace XmqqyBackpack
{
    /// <summary>
    /// 监听区块事件，统一管理所有 MapManager 的物体销毁和重建
    /// </summary>
    public class ChunkObjectManager : MonoBehaviour
    {
        public InfiniteWorld world;
        public GroundMapManager groundMap;
        public ObjectMapManager objectMap;

        private void OnEnable()
        {
            if (world == null) world = FindObjectOfType<InfiniteWorld>();
            if (groundMap == null) groundMap = FindObjectOfType<GroundMapManager>();
            if (objectMap == null) objectMap = FindObjectOfType<ObjectMapManager>();

            if (world != null)
            {
                world.OnChunkUnloaded += HandleChunkUnloaded;
                world.OnChunkLoaded += HandleChunkLoaded;
            }
        }

        private void OnDisable()
        {
            if (world != null)
            {
                world.OnChunkUnloaded -= HandleChunkUnloaded;
                world.OnChunkLoaded -= HandleChunkLoaded;
            }
        }

        private void HandleChunkUnloaded(Vector3Int chunkCoord)
        {
            int chunkSize = world.ChunkSize;
            int originX = chunkCoord.x * chunkSize;
            int originZ = chunkCoord.z * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    Vector3Int pos = new Vector3Int(originX + x, originZ + z, 0);
                    groundMap?.DestroyObjectOnly(pos);
                    objectMap?.DestroyObjectOnly(pos);
                }
            }
        }

        private void HandleChunkLoaded(Vector3Int chunkCoord, float[,] noiseMap)
        {
            // 延迟一帧，等待 GroundGenerator 和 ObjectGenerator 完成数据写入
            StartCoroutine(RebuildChunkObjectsNextFrame(chunkCoord));
        }

        private IEnumerator RebuildChunkObjectsNextFrame(Vector3Int chunkCoord)
        {
            yield return null; // 等待一帧

            int chunkSize = world.ChunkSize;
            int originX = chunkCoord.x * chunkSize;
            int originZ = chunkCoord.z * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    Vector3Int pos = new Vector3Int(originX + x, originZ + z, 0);
                    groundMap?.RecreateObject(pos);
                    objectMap?.RecreateObject(pos);
                }
            }
        }
    }
}