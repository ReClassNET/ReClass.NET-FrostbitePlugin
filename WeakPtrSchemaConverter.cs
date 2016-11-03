using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml.Linq;
using ReClassNET.DataExchange;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	/// <summary>A schema node for the WeakPtrNode.</summary>
	class WeakPtrSchemaNode : SchemaReferenceNode
	{
		public WeakPtrSchemaNode(SchemaClassNode inner)
			: base(SchemaType.Custom, inner)
		{
			Contract.Requires(inner != null);
		}
	}

	class WeakPtrSchemaConverter : ICustomSchemaConverter
	{
		/// <summary>Name of the type used in the XML data.</summary>
		private const string XmlType = "FrostBite::WeakPtr";

		/// <summary>Checks if the node can be handled.</summary>
		/// <param name="node">The node to check.</param>
		/// <returns>True if we can handle the node, false if not.</returns>
		public bool CanHandleNode(BaseNode node) => node is WeakPtrNode;

		/// <summary>Checks if the element can be handled.</summary>
		/// <param name="element">The element to check.</param>
		/// <returns>True if we can read node, false if not.</returns>
		public bool CanHandleElement(XElement element) => element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value == XmlType;

		/// <summary>Checks if the schema can be handled.</summary>
		/// <param name="schema">The schema to check.</param>
		/// <returns>True if we can handle schema, false if not.</returns>
		public bool CanHandleSchema(SchemaNode schema) => schema is WeakPtrSchemaNode;

		/// <summary>Creates the schema which represents the node.</summary>
		/// <param name="node">The node to convert.</param>
		/// <param name="classes">The mapping from classes to their schema.</param>
		/// <param name="logger">The logger.</param>
		/// <returns>The schema which represents the node.</returns>
		public SchemaNode CreateSchemaFromNode(BaseNode node, IReadOnlyDictionary<ClassNode, SchemaClassNode> classes, ILogger logger)
		{
			return new WeakPtrSchemaNode(classes[(node as WeakPtrNode).InnerNode as ClassNode])
			{
				Name = node.Name,
				Comment = node.Comment
			};
		}

		/// <summary>Creates the schema which represents the element.</summary>
		/// <param name="element">The element to convert.</param>
		/// <param name="classes">The mapping from class names to their schema.</param>
		/// <param name="logger">The logger.</param>
		/// <returns>The schema which represents the element.</returns>
		public SchemaNode CreateSchemaFromElement(XElement element, IReadOnlyDictionary<string, SchemaClassNode> classes, ILogger logger)
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

		/// <summary>Creates the node which is represented by the schema.</summary>
		/// <param name="schema">The schema to convert.</param>
		/// <param name="classes">The mapping from class schemas to their nodes.</param>
		/// <param name="logger">The logger.</param>
		/// <returns>The node which is represented by the schema.</returns>
		public BaseNode CreateNodeFromSchema(SchemaNode schema, IReadOnlyDictionary<SchemaClassNode, ClassNode> classes, ILogger logger)
		{
			var node = new WeakPtrNode
			{
				Name = schema.Name,
				Comment = schema.Comment
			};
			node.ChangeInnerNode(classes[(schema as WeakPtrSchemaNode).InnerNode]);
			return node;
		}

		/// <summary>Creates the element which represents the schema.</summary>
		/// <param name="schema">The schema to convert.</param>
		/// <param name="logger">The logger.</param>
		/// <returns>The element which represents the schema.</returns>
		public XElement CreateElementFromSchema(SchemaNode schema, ILogger logger)
		{
			return new XElement(
				ReClassNetFile.XmlNodeElement,
				new XAttribute(ReClassNetFile.XmlNameAttribute, schema.Name ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlCommentAttribute, schema.Comment ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlTypeAttribute, XmlType),
				new XAttribute(ReClassNetFile.XmlReferenceAttribute, (schema as WeakPtrSchemaNode).InnerNode.Name ?? string.Empty)
			);
		}
	}
}
