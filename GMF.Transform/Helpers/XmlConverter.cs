using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace GMF.Transform
{
   public sealed class XmlConverter
    {
        public static dynamic ToDynamic(XElement root)
        {
            return ToDynamic(root, true);
        }
        public static dynamic ToDynamic(XElement root, bool ignoreNamespace)
        {
            dynamic result = new ExpandoObject();
            ParseElement(result, root, ignoreNamespace);
            return result;
        }

        private static void ParseElement(dynamic parent, XElement node, bool ignoreNamespace)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    var item = new ExpandoObject();
                    var list = new List<dynamic>();
                    foreach (var element in node.Elements())
                    {
                        ParseElement(list, element, ignoreNamespace);
                    }

                    AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), item);
                }
                else
                {
                    AddElementWithProperties(parent, node, ignoreNamespace);
                }
            }
            else
            {
                if (node.HasAttributes)
                {
                    AddElementWithProperties(parent, node, ignoreNamespace);
                }
                else
                {
                    AddProperty(parent, node.Name.ToString(), node.Value.Trim());
                }
            }
        }

        private static void AddElementWithProperties(dynamic parent, XElement node, bool ignoreNamespace)
        {
            var item = new ExpandoObject();
            AddAttributesAsProperties(item, node, ignoreNamespace);
            foreach (var element in node.Elements())
            {
                ParseElement(item, element, ignoreNamespace);
            }

            if(!node.HasElements)
            {
                if(!string.IsNullOrWhiteSpace(node.Value.Trim()))
                {
                    AddProperty(item, "_value", node.Value.Trim());
                }
            }
            AddProperty(parent, node.Name.ToString(), item);
        }

        private static void AddAttributesAsProperties(dynamic parent, XElement node, bool ignoreNamespace)
        {
            foreach (var attribute in node.Attributes())
            {
                bool addProperty = true;
                if(ignoreNamespace)
                {
                    if(attribute.IsNamespaceDeclaration)
                    {
                        addProperty = false;
                    }
                }
                if(addProperty)
                {
                    AddProperty(parent, attribute);
                }
            }
        }
        private static void AddProperty(dynamic parent, XAttribute attribute)
        {
            AddProperty(parent, attribute.Name.ToString(), attribute.Value.ToString());
        }

        private static void AddProperty(dynamic parent, XElement node)
        {
            AddProperty(parent, node.Name.ToString(), node.Value.ToString());
        }
        private static void AddProperty(dynamic parent, string name, object value)
        {
            if(parent is List<dynamic>)
            {
                (parent as List<dynamic>).Add(value);
            }
            else
            {
                string safeName = name.Replace(":", "_", StringComparison.InvariantCultureIgnoreCase);
                (parent as IDictionary<string, object>)[safeName] = value;
            }
        }
    }
}
