using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    [Serializable]
    public class BuildingData : ThingDef
    {
        [XmlElement("PathCost")]
        public float PathCost { get; set; } = -1f;

        [XmlElement("WorkToBuild")]
        public int WorkToBuild { get; set; }

        [XmlElement("IsBuildable")]
        public bool IsBuildable { get; set; } = false;

        [XmlArray("CostList")]
        [XmlArrayItem("CostItem")]
        public List<CostItem> CostList { get; set; }

        [XmlElement("WorkToDeconstruct")]
        public float WorkToDeconstruct { get; set; }

        [XmlElement("HasShadow")]
        public bool HasShadow { get; set; } = false;

        [XmlElement("IsFullBlockShadow")]
        public bool IsFullBlockShadow { get; set; } = true;

        [XmlArray("DeconstructDropList")]
        [XmlArrayItem("DropItem")]
        public List<DropItem> DeconstructDropList { get; set; }

        [XmlArray("DestroyTypes")]
        [XmlArrayItem("DestroyType")]
        public List<string> DestroyTypes { get; set; } = null;

        public float GetActualDeconstructWork()
        {
            return WorkToDeconstruct;
        }
    }
}
