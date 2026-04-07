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
            if (obj != null)
            {
                if (dataMap.TryGetValue(gridPos, out string defName))
                {
                    BuildingData buildingData = GetDataByDefName(defName);
                    if (buildingData != null)
                    {
                        var shadowCtrl = obj.GetComponentInChildren<BuildingShadowController>();
                        if (shadowCtrl != null)
                            shadowCtrl.Initialize(buildingData);
                    }
                }

                if (extraDataMap.TryGetValue(gridPos, out var extraData))
                {
                    var initializable = obj.GetComponent<IExtraDataInitializable>();
                    if (initializable != null)
                        initializable.Initialize(extraData);
                }
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