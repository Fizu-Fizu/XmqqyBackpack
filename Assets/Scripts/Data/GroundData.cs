using System;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    [Serializable]
    public class GroundData : ThingDef
    {
        [XmlElement("PathCost")]
        public float PathCost { get; set; } = 1.0f;

        [XmlElement("Fertility")]
        public float Fertility { get; set; } = 0f;

        [XmlElement("IsBuildable")]
        public bool IsBuildable { get; set; } = true;
    }
}
