﻿using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Model content description
    /// </summary>
    [Serializable]
    public class ModelContentDescription
    {
        /// <summary>
        /// Model file name
        /// </summary>
        [XmlElement("model_filename")]
        public string ModelFileName = null;
        /// <summary>
        /// Volume meshes collection
        /// </summary>
        [XmlArray("volumes")]
        [XmlArrayItem("volume", typeof(string))]
        public string[] VolumeMeshes = null;
        /// <summary>
        /// Animation description
        /// </summary>
        [XmlElement("animation_description")]
        public AnimationDescription Animation = null;
        /// <summary>
        /// Model scale
        /// </summary>
        [XmlElement("scale")]
        public float Scale = 1f;
        /// <summary>
        /// Use controller transforms
        /// </summary>
        [XmlElement("use_controller_transform")]
        public bool UseControllerTransform = true;
    }
}
