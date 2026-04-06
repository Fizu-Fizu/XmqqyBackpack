using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace XmqqyBackpack
{
    public static class DataManager
    {
        private static Dictionary<string, BuildingData> _buildings = new Dictionary<string, BuildingData>();
        private static Dictionary<string, GroundData> _grounds = new Dictionary<string, GroundData>();
        private static bool _isLoaded = false;

        public static void LoadAll()
        {
            if (_isLoaded) return;

            string dataPath = Path.Combine(Application.streamingAssetsPath, "Datas");
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
            Debug.Log($"[DataManager] 加载完成: Building={_buildings.Count}, Ground={_grounds.Count}");
        }

        private static void TryLoadFile(string filePath)
        {
            try
            {
                // 先尝试作为 BuildingData 列表加载
                var buildingList = Deserialize<List<BuildingData>>(filePath, "Defs");
                if (buildingList != null && buildingList.Count > 0)
                {
                    foreach (var b in buildingList)
                        if (!_buildings.ContainsKey(b.DefName))
                            _buildings[b.DefName] = b;
                    Debug.Log($"[DataManager] 加载 Building: {Path.GetFileName(filePath)} -> {buildingList.Count} 条");
                    return;
                }

                // 再尝试作为 GroundData 列表加载
                var groundList = Deserialize<List<GroundData>>(filePath, "Defs");
                if (groundList != null && groundList.Count > 0)
                {
                    foreach (var g in groundList)
                        if (!_grounds.ContainsKey(g.DefName))
                            _grounds[g.DefName] = g;
                    Debug.Log($"[DataManager] 加载 Ground: {Path.GetFileName(filePath)} -> {groundList.Count} 条");
                    return;
                }

                Debug.LogWarning($"[DataManager] 无法识别文件内容: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataManager] 加载失败 {filePath}: {e.Message}");
            }
        }

        private static T Deserialize<T>(string filePath, string rootName)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
            using (var reader = new StreamReader(filePath))
                return (T)serializer.Deserialize(reader);
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