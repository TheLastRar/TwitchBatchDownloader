using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace TwitchVodDownloaderSharp.TwitchAPI
{
    [DataContract]
    class XmlInspector : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }
        public XElement GetXml()
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(XmlInspector));
            StringBuilder sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                dcs.WriteObject(writer, this);
            }
            return XDocument.Parse(sb.ToString()).Root;
        }
    }

    class DictionaryTwitch : IDataContractSurrogate
    {
        public Type GetDataContractType(Type type)
        {
            if (type == typeof(Dictionary<string, string>) |
                type == typeof(Dictionary<string, double>))
            {
                return typeof(XmlInspector);
            }
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            if (obj is XmlInspector)
            {
                XElement xml = (obj as XmlInspector).GetXml();

                if (targetType == typeof(Dictionary<string, string>))
                {
                    Dictionary<string, string> ret;
                    ret = xml.Elements().ToDictionary(
                        x => x.Name.ToString(),
                        x => (string)x
                        );
                    return ret;
                }
                if (targetType == typeof(Dictionary<string, double>))
                {
                    Dictionary<string, double> ret;
                    ret = xml.Elements().ToDictionary(
                        x => x.Name.ToString(),
                        x => (double)x
                        );
                    return ret;
                }
            }
            return obj;
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes)
        {
            throw new NotImplementedException();
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            throw new NotImplementedException();
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData) { throw new NotImplementedException(); }
        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit) { return typeDeclaration; }
        public object GetCustomDataToExport(Type clrType, Type dataContractType) { return null; }
        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType) { return null; }
    }
}
