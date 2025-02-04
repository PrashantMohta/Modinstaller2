﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Scarab.Models
{
    public static class SerializationConstants
    {
        public const string NAMESPACE = "https://github.com/HollowKnight-Modding/HollowKnight.ModLinks/HollowKnight.ModManager";
    }

    [Serializable]
    public record Manifest
    {
        // Internally handle the Link/Links either-or divide
        private Links? _links;
        private Link? _link;

        public VersionWrapper Version;

        public string Name { get; set; }

        [XmlElement]
        public Link? Link
        {
            get => throw new NotImplementedException("This is only for XML Serialization!");
            set => _link = value;
        }

        public Links Links
        {
            get =>
                _links ??= new Links
                {
                    Windows = _link ?? throw new InvalidDataException(nameof(_link)),
                    Mac = _link,
                    Linux = _link
                };
            set => _links = value;
        }

        [XmlArray("Dependencies")]
        [XmlArrayItem("Dependency")]
        public string[] Dependencies { get; set; }

        public string Description { get; set; }

        // For serializer and nullability
        public Manifest()
        {
            Version = null!;
            Name = null!;
            Dependencies = null!;
            Description = null!;
        }

        public override string ToString()
        {
            return "{\n"
                + $"\t{nameof(Version)}: {Version},\n"
                + $"\t{nameof(Name)}: {Name},\n"
                + $"\t{nameof(Links)}: {(object?) _link ?? Links},\n"
                + $"\t{nameof(Dependencies)}: {string.Join(", ", Dependencies)},\n"
                + $"\t{nameof(Description)}: {Description}\n"
                + "}";
        }
    }

    [Serializable]
    public record VersionWrapper : IXmlSerializable
    {
        public VersionWrapper() => Value = null!;

        public Version Value { get; set; }

        public XmlSchema? GetSchema() => null;
        public void ReadXml(XmlReader reader) => Value = Version.Parse(reader.ReadElementContentAsString());
        public void WriteXml(XmlWriter writer) => writer.WriteString(Value.ToString());

        public static implicit operator VersionWrapper(Version v) => new() { Value = v };

        public override string ToString() => Value.ToString();
    }

    public class Links
    {
        public Link Windows = null!;
        public Link Mac = null!;
        public Link Linux = null!;

        public override string ToString()
        {
            return "Links {"
                + $"\t{nameof(Windows)} = {Windows},\n"
                + $"\t{nameof(Mac)} = {Mac},\n"
                + $"\t{nameof(Linux)} = {Linux}\n"
                + "}";
        }

        public string GetOSUrl()
        {
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Windows.URL,
                PlatformID.MacOSX => Mac.URL,
                PlatformID.Unix => Linux.URL,

                var val => throw new NotSupportedException(val.ToString())
            };
        }
    }

    public class Link
    {
        [XmlAttribute]
        public string SHA256 = null!;

        [XmlText]
        public string URL = null!;

        public override string ToString()
        {
            return $"[Link: {nameof(SHA256)} = {SHA256}, {nameof(URL)}: {URL}]";
        }
    }

    [Serializable]
    public class ApiManifest
    {
        public int Version { get; set; }

        [XmlArray("Files")]
        [XmlArrayItem("File")]
        public List<string> Files { get; set; }

        public Links Links { get; set; }

        // For serializer and nullability
        public ApiManifest()
        {
            Files = null!;
            Links = null!;
        }
    }

    [XmlRoot(Namespace = SerializationConstants.NAMESPACE)]
    public class ApiLinks
    {
        public ApiManifest Manifest { get; set; } = null!;
    }

    [XmlRoot(Namespace = SerializationConstants.NAMESPACE)]
    public class ModLinks
    {
        [XmlElement("Manifest")]
        public Manifest[] Manifests { get; set; } = null!;

        public override string ToString()
        {
            return "ModLinks {[\n"
                + string.Join("\n", Manifests.Select(x => x.ToString()))
                + "]}";
        }
    }
}