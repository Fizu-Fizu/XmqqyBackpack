using System;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    /// <summary>
    /// 合成材料条目（用于配方）
    /// </summary>
    [Serializable]
    public class CostItem
    {
        [XmlElement("DefName")]
        public string DefName { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }
    }
}