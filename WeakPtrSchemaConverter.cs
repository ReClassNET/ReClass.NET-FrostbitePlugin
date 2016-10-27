using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using ReClassNET.DataExchange;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	class WeakPtrSchemaNode : SchemaCustomNode
	{
		public SchemaClassNode InnerNode { get; }

		public WeakPtrSchemaNode(SchemaClassNode inner)
		{
			Contract.Requires(inner != null);

			InnerNode = inner;
		}
	}

	class WeakPtrSchemaConverter : ICustomSchemaConverter
	{
		private const string XmlType = "FrostBite::WeakPtr";

		public bool CanReadNode(BaseNode node) => node is WeakPtrNode;

		public bool CanReadNode(XElement element) => element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value == XmlType;

		public bool CanWriteNode(SchemaCustomNode node) => node is WeakPtrSchemaNode;

		public SchemaCustomNode ReadFromNode(BaseNode node, IReadOnlyDictionary<ClassNode, SchemaClassNode> classes, ILogger logger)
		{
			return new WeakPtrSchemaNode(classes[(node as WeakPtrNode).InnerNode as ClassNode])
			{
				Name = node.Name,
				Comment = node.Comment
			};
		}

		public SchemaCustomNode ReadFromXml(XElement element, IReadOnlyDictionary<string, SchemaClassNode> classes, ILogger logger)
		{
			var reference = element.Attribute(ReClassNetFile.XmlReferenceAttribute)?.Value;
			if (reference == null || !classes.ContainsKey(reference))
			{
				logger.Log(LogLevel.Warning, $"Skipping node with unknown reference: {reference}");
				logger.Log(LogLevel.Warning, element.ToString());

				return null;
			}

			return new WeakPtrSchemaNode(classes[reference])
			{
				Name = element.Attribute(ReClassNetFile.XmlNameAttribute)?.Value,
				Comment = element.Attribute(ReClassNetFile.XmlCommentAttribute)?.Value
			};
		}

		public BaseNode WriteToNode(SchemaCustomNode schema, IReadOnlyDictionary<SchemaClassNode, ClassNode> classes, ILogger logger)
		{
			return new WeakPtrNode()
			{
				Name = schema.Name,
				Comment = schema.Comment,
				InnerNode = classes[(schema as WeakPtrSchemaNode).InnerNode]
			};
		}

		public XElement WriteToXml(SchemaCustomNode node, ILogger logger)
		{
			return new XElement(
				ReClassNetFile.XmlNodeElement,
				new XAttribute(ReClassNetFile.XmlNameAttribute, node.Name ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlCommentAttribute, node.Comment ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlTypeAttribute, XmlType),
				new XAttribute(ReClassNetFile.XmlReferenceAttribute, (node as WeakPtrSchemaNode).InnerNode.Name ?? string.Empty)
			);
		}
	}
}
