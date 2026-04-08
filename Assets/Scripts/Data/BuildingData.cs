using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 建筑数据定义
    /// </summary>
    [Serializable]
    public class BuildingData
    {
        /// <summary>
        /// [必须] 物体唯一标识
        /// </summary>
        [XmlElement("DefName")]
        public string DefName { get; set; }

        /// <summary>
        /// [必须] 物体显示名称
        /// </summary>
        [XmlElement("Label")]
        public string Label { get; set; }

        /// <summary>
        /// [可选] 物体描述
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// [可选] 移动代价（默认 -1 表示不可行走）
        /// </summary>
        [XmlElement("PathCost")]
        public float PathCost { get; set; } = -1f;

        /// <summary>
        /// [可选] 建造工作量
        /// </summary>
        [XmlElement("WorkToBuild")]
        public int WorkToBuild { get; set; }

        /// <summary>
        /// [可选] 是否可被玩家建造
        /// </summary>
        [XmlElement("IsBuildable")]
        public bool IsBuildable { get; set; } = false;

        /// <summary>
        /// [可选] 建造所需材料列表
        /// </summary>
        [XmlArray("CostList")]
        [XmlArrayItem("CostItem")]
        public List<CostItem> CostList { get; set; }

        /// <summary>
        /// [可选] 物体分类标签
        /// </summary>
        [XmlElement("Category")]
        public string Category { get; set; }

        /// <summary>
        /// [可选] 贴图路径（相对于 Resources）
        /// </summary>
        [XmlElement("TexturePath")]
        public string TexturePath { get; set; }

        /// <summary>
        /// [必须] 拆除工作量，XML中必须提供正值
        /// </summary>
        [XmlElement("WorkToDeconstruct")]
        public float WorkToDeconstruct { get; set; }

        /// <summary>
        /// [可选] 是否有阴影，默认 false
        /// </summary>
        [XmlElement("HasShadow")]
        public bool HasShadow { get; set; } = false;

        /// <summary>
        /// [可选] 是否为完整方块阴影
        /// </summary>
        [XmlElement("IsFullBlockShadow")]
        public bool IsFullBlockShadow { get; set; } = true;

        /// <summary>
        /// 获取拆除工作量
        /// </summary>
        public float GetActualDeconstructWork()
        {
            return WorkToDeconstruct;
        }
    }
}