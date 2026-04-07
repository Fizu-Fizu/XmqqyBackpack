using UnityEngine;

namespace XmqqyBackpack
{
    /// <summary>
    /// 控制建筑阴影的显示逻辑
    /// 挂载于预制体中的 Shadow 对象上
    /// </summary>
    public class BuildingShadowController : MonoBehaviour
    {
        [Header("子物体引用")]
        [SerializeField] private GameObject shadowFull;   // 完整方块阴影（Shadow_1）
        [SerializeField] private GameObject shadowPartial;// 不完整方块阴影（Shadow_2）

        /// <summary>
        /// 根据建筑数据初始化阴影显示
        /// </summary>
        public void Initialize(BuildingData data)
        {
            if (data == null)
            {
                Debug.LogWarning("BuildingShadowController: 建筑数据为空，关闭所有阴影");
                SetShadowsActive(false, false);
                return;
            }

            bool hasShadow = data.HasShadow;
            bool isFullBlock = data.IsFullBlockShadow;

            if (!hasShadow)
            {
                // 无阴影：全部关闭
                SetShadowsActive(false, false);
            }
            else
            {
                // 有阴影：根据方块完整性选择显示哪个
                SetShadowsActive(isFullBlock, !isFullBlock);
            }
        }

        private void SetShadowsActive(bool fullActive, bool partialActive)
        {
            if (shadowFull != null)
                shadowFull.SetActive(fullActive);
            if (shadowPartial != null)
                shadowPartial.SetActive(partialActive);
        }

        // 可在编辑器中自动绑定，避免手动拖拽
        private void Reset()
        {
            // 尝试自动查找子物体
            Transform shadow1 = transform.Find("Shadow_1");
            if (shadow1 != null) shadowFull = shadow1.gameObject;

            Transform shadow2 = transform.Find("Shadow_2");
            if (shadow2 != null) shadowPartial = shadow2.gameObject;
        }
    }
}