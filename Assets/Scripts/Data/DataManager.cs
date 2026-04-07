using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace XmqqyBackpack
{
    public static class DataManager
    {
        private static Dictionary<string, ItemData> _items = new Dictionary<string, ItemData>();
        private static Dictionary<string, BuildingData> _buildings = new Dictionary<string, BuildingData>();
        private static Dictionary<string, GroundData> _grounds = new Dictionary<string, GroundData>();
        private static bool _isLoaded = false;

        public static void LoadAll()
        {
            if (_isLoaded) return;

            string dataPath = Path.Combine(Application.streamingAssetsPath, "Data");
            if (!Directory.Exists(dataPath))
            {
                Debug.LogError($"[DataManager] 目录不存在: {dataPath}");
                return;
            }

            string[] xmlFiles = Directory.GetFiles(dataPath, "*.xml", SearchOption.AllDirectories);
            Debug.Log($"[DataManager] 找到 {xmlFiles.Length} 个 XML 文件");

            foreach (string filePath in xmlFiles)
            {
                TryLoadFile(filePath);
            }

            _isLoaded = true;
            Debug.Log($"[DataManager] 加载完成: Item={_items.Count}, Building={_buildings.Count}, Ground={_grounds.Count}");
        }

        private static void TryLoadFile(string filePath)
        {
            // 1. 尝试作为 BuildingData 列表加载
            var buildingList = Deserialize<List<BuildingData>>(filePath, "Defs");
            if (buildingList != null && buildingList.Count > 0)
            {
                foreach (var b in buildingList)
                    if (!_buildings.ContainsKey(b.DefName))
                        _buildings[b.DefName] = b;
                Debug.Log($"[DataManager] 加载 Building: {Path.GetFileName(filePath)} -> {buildingList.Count} 条");
                return;
            }

            // 2. 尝试作为 GroundData 列表加载
            var groundList = Deserialize<List<GroundData>>(filePath, "Defs");
            if (groundList != null && groundList.Count > 0)
            {
                foreach (var g in groundList)
                    if (!_grounds.ContainsKey(g.DefName))
                        _grounds[g.DefName] = g;
                Debug.Log($"[DataManager] 加载 Ground: {Path.GetFileName(filePath)} -> {groundList.Count} 条");
                return;
            }

            // 3. 尝试作为 ItemData 列表加载
            var itemList = Deserialize<List<ItemData>>(filePath, "Defs");
            if (itemList != null && itemList.Count > 0)
            {
                foreach (var i in itemList)
                    if (!_items.ContainsKey(i.DefName))
                        _items[i.DefName] = i;
                Debug.Log($"[DataManager] 加载 Item: {Path.GetFileName(filePath)} -> {itemList.Count} 条");
                return;
            }

            Debug.LogWarning($"[DataManager] 无法识别文件内容: {filePath}");
        }

        private static T Deserialize<T>(string filePath, string rootName)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
                using (var reader = new StreamReader(filePath))
                    return (T)serializer.Deserialize(reader);
            }
            catch
            {
                return default(T);
            }
        }

        public static ItemData GetItem(string defName)
        {
            _items.TryGetValue(defName, out var data);
            if (data == null) Debug.LogWarning($"[DataManager] 未找到物品: {defName}");
            return data;
        }

        public static BuildingData GetBuilding(string defName)
        {
            _buildings.TryGetValue(defName, out var data);
            if (data == null) Debug.LogWarning($"[DataManager] 未找到建筑: {defName}");
            return data;
        }

        public static GroundData GetGround(string defName)
        {
            _grounds.TryGetValue(defName, out var data);
            if (data == null) Debug.LogWarning($"[DataManager] 未找到地面: {defName}");
            return data;
        }
    }
}