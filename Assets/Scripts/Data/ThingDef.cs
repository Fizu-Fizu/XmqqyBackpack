using System;
using System.Xml.Serialization;

namespace XmqqyBackpack
{
    [Serializable]
    public abstract class ThingDef
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("ParentName")]
        public string ParentName { get; set; }

        [XmlAttribute("Abstract")]
        public bool Abstract { get; set; }

        [XmlElement("DefName")]
        public string DefName { get; set; }

        [XmlElement("Label")]
        public string Label { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("TexturePath")]
        public string TexturePath { get; set; }
    }
}
