using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    [Serializable]
    public class ToolProperties
    {
        /// <summary>
        /// 可破坏的类型列表
        /// </summary>
        [XmlArray("DestroyTypes")]
        [XmlArrayItem("DestroyType")]
        public List<string> DestroyTypes { get; set; }

        /// <summary>
        /// 工作速度加成倍率（1.0 = 正常）
        /// </summary>
        [XmlElement("WorkSpeedBonus")]
        public float WorkSpeedBonus { get; set; } = 1.0f;
    }
}