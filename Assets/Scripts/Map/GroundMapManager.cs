using UnityEngine;
using System.Collections.Generic;

namespace XmqqyBackpack
{
    public class GroundMapManager : BaseMapManager<GroundData>
    {
        public static GroundMapManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        protected override GroundData GetDataByDefName(string defName)
        {
            return DataManager.GetGround(defName);
        }

        protected override void ApplyTexture(GameObject obj, GroundData data)
        {
            TextureImage texImage = obj.GetComponentInChildren<TextureImage>();
            if (texImage != null && !string.IsNullOrEmpty(data.TexturePath))
                texImage.SetImageFromResources(data.TexturePath);
            else
                Debug.LogError($"GroundMapManager: 无法设置贴图 for {data.DefName}");
        }

        public void BuildGround(Vector3Int gridPos, string defName, Dictionary<string, object> extraData = null)
        {
            if (dataMap.ContainsKey(gridPos))
            {
                Debug.LogWarning($"位置 {gridPos} 已有地面");
                return;
            }
            SetData(gridPos, defName, extraData);
            RecreateObject(gridPos);
        }

        public void DigGround(Vector3Int gridPos)
        {
            RemoveAll(gridPos);
        }
    }
}