using System;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 拆除掉落物条目（带数量范围）
    /// </summary>
    [Serializable]
    public class DropItem
    {
        [XmlElement("DefName")]
        public string DefName { get; set; }

        [XmlElement("MinAmount")]
        public int MinAmount { get; set; } = 1;

        [XmlElement("MaxAmount")]
        public int MaxAmount { get; set; } = 1;
    }
}