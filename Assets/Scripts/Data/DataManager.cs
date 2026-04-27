using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace XmqqyBackpack
{
    public static class DataManager
    {
        private static Dictionary<string, ThingDef> _defs = new Dictionary<string, ThingDef>();
        private static Dictionary<string, ThingDef> _templates = new Dictionary<string, ThingDef>();
        private static bool _isLoaded = false;

        public static void LoadAll()
        {
            if (_isLoaded) return;

            DefTypeRegistry.Initialize();

            string dataPath = Path.Combine(Application.streamingAssetsPath, "Data");
            if (!Directory.Exists(dataPath))
            {
                Debug.LogError($"[DataManager] 目录不存在: {dataPath}");
                return;
            }

            string[] xmlFiles = Directory.GetFiles(dataPath, "*.xml", SearchOption.AllDirectories);
            Debug.Log($"[DataManager] 找到 {xmlFiles.Length} 个 XML 文件");

            var allDefs = new List<ThingDef>();

            foreach (string filePath in xmlFiles)
            {
                var loaded = DeserializeFile(filePath);
                if (loaded != null)
                    allDefs.AddRange(loaded);
            }

            foreach (var def in allDefs)
            {
                if (!string.IsNullOrEmpty(def.Name))
                {
                    if (_templates.ContainsKey(def.Name))
                        Debug.LogWarning($"[DataManager] 重复的模板 Name: {def.Name}");
                    _templates[def.Name] = def;
                }
            }

            var resolving = new HashSet<string>();
            foreach (var def in allDefs)
            {
                if (!string.IsNullOrEmpty(def.ParentName))
                    ResolveInheritance(def, resolving);
            }

            foreach (var def in allDefs)
            {
                if (!def.Abstract && !string.IsNullOrEmpty(def.DefName))
                {
                    if (_defs.ContainsKey(def.DefName))
                        Debug.LogWarning($"[DataManager] 重复的 DefName: {def.DefName}");
                    _defs[def.DefName] = def;
                }
            }

            _isLoaded = true;
            Debug.Log($"[DataManager] 加载完成: Defs={_defs.Count}, Templates={_templates.Count}");
        }

        private static List<ThingDef> DeserializeFile(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Root;
                if (root == null)
                {
                    Debug.LogWarning($"[DataManager] 空文件: {filePath}");
                    return null;
                }

                string typeClass = root.Attribute("TypeClass")?.Value;
                if (string.IsNullOrEmpty(typeClass))
                {
                    Debug.LogWarning($"[DataManager] 缺少 TypeClass 属性: {filePath}");
                    return null;
                }

                Type concreteType = DefTypeRegistry.GetType(typeClass);
                if (concreteType == null)
                {
                    Debug.LogWarning($"[DataManager] 未知 TypeClass '{typeClass}': {filePath}");
                    return null;
                }

                var listType = typeof(List<>).MakeGenericType(concreteType);
                var serializer = new XmlSerializer(listType, new XmlRootAttribute("Defs"));

                using (var reader = new StreamReader(filePath))
                {
                    var list = (IList)serializer.Deserialize(reader);
                    var result = new List<ThingDef>();
                    foreach (var item in list)
                        result.Add((ThingDef)item);
                    Debug.Log($"[DataManager] 加载 {typeClass}: {Path.GetFileName(filePath)} -> {result.Count} 条");
                    return result;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 反序列化失败: {filePath}\n{e.Message}");
                return null;
            }
        }

        private static void ResolveInheritance(ThingDef def, HashSet<string> resolving)
        {
            if (string.IsNullOrEmpty(def.ParentName))
                return;

            if (!_templates.TryGetValue(def.ParentName, out var parent))
            {
                Debug.LogError($"[DataManager] ParentName='{def.ParentName}' 不存在 (DefName={def.DefName ?? def.Name})");
                return;
            }

            if (def.ParentName == def.Name || def.ParentName == def.DefName)
            {
                Debug.LogError($"[DataManager] 自引用继承: {def.ParentName}");
                return;
            }

            if (!resolving.Add(def.ParentName))
            {
                Debug.LogError($"[DataManager] 循环继承检测: {string.Join(" -> ", resolving)}");
                resolving.Remove(def.ParentName);
                return;
            }

            if (!string.IsNullOrEmpty(parent.ParentName))
                ResolveInheritance(parent, resolving);

            resolving.Remove(def.ParentName);

            MergeProperties(parent, def);
        }

        private static void MergeProperties(ThingDef parent, ThingDef child)
        {
            var defaultInstance = Activator.CreateInstance(child.GetType());
            var type = child.GetType();

            var flags = BindingFlags.Public | BindingFlags.Instance;
            var properties = type.GetProperties(flags)
                .Where(p => p.CanWrite && p.CanRead)
                .Where(p => p.Name != "Name" && p.Name != "ParentName" && p.Name != "Abstract");

            foreach (var prop in properties)
            {
                var childVal = prop.GetValue(child);
                var defaultVal = prop.GetValue(defaultInstance);

                if (ValuesEqual(childVal, defaultVal))
                {
                    var parentProp = parent.GetType().GetProperty(prop.Name, flags);
                    if (parentProp != null && parentProp.CanRead)
                    {
                        var parentVal = parentProp.GetValue(parent);
                        if (!ValuesEqual(parentVal, defaultVal))
                            prop.SetValue(child, parentVal);
                    }
                }
            }
        }

        private static bool ValuesEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a is IList listA && b is IList listB)
                return listA.Count == listB.Count && listA.Cast<object>().SequenceEqual(listB.Cast<object>());
            return a.Equals(b);
        }

        public static T GetDef<T>(string defName) where T : ThingDef
        {
            if (!_isLoaded) LoadAll();
            _defs.TryGetValue(defName, out var def);
            return def as T;
        }

        public static IEnumerable<T> GetAllDefs<T>() where T : ThingDef
        {
            if (!_isLoaded) LoadAll();
            return _defs.Values.OfType<T>();
        }

        public static ItemData GetItem(string defName)
        {
            return GetDef<ItemData>(defName);
        }

        public static BuildingData GetBuilding(string defName)
        {
            return GetDef<BuildingData>(defName);
        }

        public static GroundData GetGround(string defName)
        {
            return GetDef<GroundData>(defName);
        }

        public static IEnumerable<ItemData> GetAllItems()
        {
            return GetAllDefs<ItemData>();
        }

        public static IEnumerable<BuildingData> GetAllBuildings()
        {
            return GetAllDefs<BuildingData>();
        }

        public static IEnumerable<GroundData> GetAllGrounds()
        {
            return GetAllDefs<GroundData>();
        }
    }
}
