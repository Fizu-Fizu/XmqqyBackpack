using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    [Serializable]
    public class ItemData : ThingDef
    {
        [XmlElement("MaxStack")]
        public int MaxStack { get; set; } = 100;

        [XmlElement("IconPath")]
        public string IconPath { get; set; }

        [XmlElement("CanCraft")]
        public bool CanCraft { get; set; } = false;

        [XmlArray("CraftRecipe")]
        [XmlArrayItem("CostItem")]
        public List<CostItem> CraftRecipe { get; set; }

        [XmlElement("ToolProperties")]
        public ToolProperties ToolProperties { get; set; } = null;

        [XmlElement("CraftingBuildingDefName")]
        public string CraftingBuildingDefName { get; set; }
    }
}
