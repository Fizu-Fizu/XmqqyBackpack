using UnityEngine;
using System.Collections.Generic;

namespace XmqqyBackpack
{
    public class ObjectMapManager : BaseMapManager<BuildingData>
    {
        public static ObjectMapManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        protected override BuildingData GetDataByDefName(string defName)
        {
            // 注意：请确保 DataManager 中有 GetBuilding 方法，如果原方法是 GetThing，请改为 GetBuilding
            return DataManager.GetBuilding(defName);
        }

        protected override void ApplyTexture(GameObject obj, BuildingData data)
        {
            TextureImage texImage = obj.GetComponentInChildren<TextureImage>();
            if (texImage != null && !string.IsNullOrEmpty(data.TexturePath))
                texImage.SetImageFromResources(data.TexturePath);
            else
                Debug.LogError($"ObjectMapManager: 无法设置贴图 for {data.DefName}");
        }

        public override GameObject RecreateObject(Vector3Int gridPos)
        {
            GameObject obj = base.RecreateObject(gridPos);
            if (obj != null && extraDataMap.TryGetValue(gridPos, out var extraData))
            {
                var initializable = obj.GetComponent<IExtraDataInitializable>();
                if (initializable != null)
                    initializable.Initialize(extraData);
                // 如果没有实现接口，就不做任何事（不报错）
            }
            return obj;
        }

        public void BuildObject(Vector3Int gridPos, string defName, Dictionary<string, object> extraData = null)
        {
            if (dataMap.ContainsKey(gridPos))
            {
                Debug.LogWarning($"位置 {gridPos} 已有物体");
                return;
            }
            SetData(gridPos, defName, extraData);
            RecreateObject(gridPos);
        }

        public void DigObject(Vector3Int gridPos)
        {
            RemoveAll(gridPos);
        }
    }
}