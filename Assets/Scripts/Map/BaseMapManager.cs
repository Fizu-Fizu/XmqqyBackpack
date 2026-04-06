using System.Collections.Generic;
using UnityEngine;

namespace XmqqyBackpack
{
    public abstract class BaseMapManager<T> : MonoBehaviour where T : class
    {
        protected Dictionary<Vector3Int, string> dataMap = new Dictionary<Vector3Int, string>();
        protected Dictionary<Vector3Int, Dictionary<string, object>> extraDataMap = new Dictionary<Vector3Int, Dictionary<string, object>>();
        protected Dictionary<Vector3Int, GameObject> objectRefMap = new Dictionary<Vector3Int, GameObject>();

        [SerializeField] protected GameObject tilePrefab;
        [SerializeField] protected Transform parentContainer;

        public void Initialize(GameObject prefab, Transform container)
        {
            tilePrefab = prefab;
            parentContainer = container;
        }

        public void SetData(Vector3Int gridPos, string defName, Dictionary<string, object> extraData = null)
        {
            dataMap[gridPos] = defName;
            if (extraData != null)
                extraDataMap[gridPos] = extraData;
            else if (!extraDataMap.ContainsKey(gridPos))
                extraDataMap[gridPos] = new Dictionary<string, object>();
        }

        public string GetDefName(Vector3Int gridPos)
        {
            dataMap.TryGetValue(gridPos, out string name);
            return name;
        }

        public Dictionary<string, object> GetExtraData(Vector3Int gridPos)
        {
            extraDataMap.TryGetValue(gridPos, out var data);
            return data;
        }

        public void DestroyObjectOnly(Vector3Int gridPos)
        {
            if (objectRefMap.TryGetValue(gridPos, out GameObject obj))
            {
                Destroy(obj);
                objectRefMap.Remove(gridPos);
            }
        }

        public void RemoveAll(Vector3Int gridPos)
        {
            DestroyObjectOnly(gridPos);
            dataMap.Remove(gridPos);
            extraDataMap.Remove(gridPos);
        }

        public virtual GameObject RecreateObject(Vector3Int gridPos)
        {
            if (!dataMap.ContainsKey(gridPos)) return null;
            if (objectRefMap.ContainsKey(gridPos)) return objectRefMap[gridPos];

            string defName = dataMap[gridPos];
            T data = GetDataByDefName(defName);
            if (data == null) return null;

            GameObject obj = Instantiate(tilePrefab, new Vector3(gridPos.x, gridPos.y, 0), Quaternion.identity);
            if (parentContainer != null) obj.transform.SetParent(parentContainer);
            objectRefMap[gridPos] = obj;

            ApplyTexture(obj, data);
            return obj;
        }

        protected abstract T GetDataByDefName(string defName);
        protected abstract void ApplyTexture(GameObject obj, T data);

        public IReadOnlyDictionary<Vector3Int, string> GetAllData() => dataMap;
        public void ClearAll()
        {
            foreach (var obj in objectRefMap.Values) Destroy(obj);
            dataMap.Clear();
            extraDataMap.Clear();
            objectRefMap.Clear();
        }
    }
}