using UnityEngine;
using UnityEngine.UI;

namespace XmqqyBackpack
{
    /// <summary>
    /// 合成表中材料或产物的单个图标+数量条目
    /// </summary>
    public class CraftingSlotUI : MonoBehaviour
    {
        public Image iconImage;
        public Text amountText;

        /// <summary>
        /// 设置显示内容
        /// </summary>
        /// <param name="defName">物品标识</param>
        /// <param name="amount">数量（材料需求数量或产物产出数量）</param>
        public void SetItem(string defName, int amount)
        {
            ItemData data = DataManager.GetItem(defName);
            if (data != null && !string.IsNullOrEmpty(data.IconPath))
            {
                Sprite icon = Resources.Load<Sprite>(data.IconPath);
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
    }
}