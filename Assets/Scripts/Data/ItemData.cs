using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 物品数据定义（包含基础属性、堆叠、合成等）
    /// </summary>
    [Serializable]
    public class ItemData
    {
        /// <summary>
        /// [必须] 物品唯一标识（对应 defName）
        /// </summary>
        [XmlElement("DefName")]
        public string DefName { get; set; }

        /// <summary>
        /// [必须] 物品显示名称
        /// </summary>
        [XmlElement("Label")]
        public string Label { get; set; }

        /// <summary>
        /// [可选] 物品描述
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// [可选] 最大堆叠数量，默认 1
        /// </summary>
        [XmlElement("MaxStack")]
        public int MaxStack { get; set; } = 100;

        /// <summary>
        /// [可选] 图标在 Resources 中的路径（例如 "Icons/apple"）
        /// </summary>
        [XmlElement("IconPath")]
        public string IconPath { get; set; }

        /// <summary>
        /// [可选] 是否可以被合成，默认 false
        /// </summary>
        [XmlElement("CanCraft")]
        public bool CanCraft { get; set; } = false;

        /// <summary>
        /// [可选] 合成配方（材料列表），仅在 CanCraft=true 时有效
        /// </summary>
        [XmlArray("CraftRecipe")]
        [XmlArrayItem("CostItem")]
        public List<CostItem> CraftRecipe { get; set; }

        /// <summary>
        /// [可选] 工具属性
        /// </summary>
        [XmlElement("ToolProperties")]
        public ToolProperties ToolProperties { get; set; } = null;

        /// <summary>
        /// [可选] 合成所需的建筑 DefName（指向 BuildingData）
        /// </summary>
        [XmlElement("CraftingBuildingDefName")]
        public string CraftingBuildingDefName { get; set; }
    }
}