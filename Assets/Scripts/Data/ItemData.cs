using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 实际物品数据，继承自基础 ItemData
    /// </summary>
    [Serializable]
    public class ItemData
    {
        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        [XmlElement("MaxStack")]
        public int MaxStack { get; set; } = 1;

        /// <summary>
        /// 图标在 Resources 中的路径
        /// </summary>
        [XmlElement("IconPath")]
        public string IconPath { get; set; }

        /// <summary>
        /// 是否可堆叠（便捷属性，不参与序列化）
        /// </summary>
        [XmlIgnore]
        public bool IsStackable => MaxStack > 1;

        // ========== 合成相关 ==========

        /// <summary>
        /// 是否可以被合成
        /// </summary>
        [XmlElement("CanCraft")]
        public bool CanCraft { get; set; } = false;

        /// <summary>
        /// 合成配方（材料列表）
        /// </summary>
        [XmlArray("CraftRecipe")]
        [XmlArrayItem("CostItem")]
        public List<CostItem> CraftRecipe { get; set; }

        /// <summary>
        /// 合成所需的建筑 DefName，为 null 表示可在任意地点合成
        /// </summary>
        [XmlElement("CraftingBuildingDefName")]
        public string CraftingBuildingDefName { get; set; }
    }
}