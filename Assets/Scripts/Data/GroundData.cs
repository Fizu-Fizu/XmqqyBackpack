using System;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 地面类型数据定义
    /// </summary>
    [Serializable]
    public class GroundData
    {
        /// <summary>
        /// [必须] 地面唯一标识
        /// </summary>
        [XmlElement("DefName")]
        public string DefName { get; set; }

        /// <summary>
        /// [必须] 地面显示名称
        /// </summary>
        [XmlElement("Label")]
        public string Label { get; set; }

        /// <summary>
        /// [可选] 地面描述
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; }

        /// <summary>
        /// [可选] 移动代价（默认 1.0，数值越大行走越慢）
        /// </summary>
        [XmlElement("PathCost")]
        public float PathCost { get; set; } = 1.0f;

        /// <summary>
        /// [可选] 肥力值（0-1），用于植物生长
        /// </summary>
        [XmlElement("Fertility")]
        public float Fertility { get; set; } = 0f;

        /// <summary>
        /// [可选] 是否可在上面建造建筑（默认 true）
        /// </summary>
        [XmlElement("IsBuildable")]
        public bool IsBuildable { get; set; } = true;

        /// <summary>
        /// [可选] 贴图路径（相对于 Resources 文件夹，不含扩展名）
        /// </summary>
        [XmlElement("TexturePath")]
        public string TexturePath { get; set; }
    }
}