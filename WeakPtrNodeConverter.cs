using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;
using ReClassNET.DataExchange;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	class WeakPtrNodeConverter : ICustomNodeConverter
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

		/// <summary>Creates a node from the xml element. This method gets only called if <see cref="CanHandleElement(XElement)"/> returned true.</summary>
		/// <param name="element">The element to create the node from.</param>
		/// <param name="parent">The parent of the node.</param>
		/// <param name="classes">The list of classes which correspond to the node.</param>
		/// <param name="logger">The logger used to output messages.</param>
		/// <param name="node">[out] The node for the xml element.</param>
		/// <returns>True if a node was created, otherwise false.</returns>
		public bool TryCreateNodeFromElement(XElement element, ClassNode parent, IEnumerable<ClassNode> classes, ILogger logger, out BaseNode node)
		{
			node = null;

			var reference = NodeUuid.FromBase64String(element.Attribute(ReClassNetFile.XmlReferenceAttribute)?.Value, false);
			var innerClass = classes.Where(c => c.Uuid.Equals(reference)).FirstOrDefault();
			if (innerClass == null)
			{
				logger.Log(LogLevel.Warning, $"Skipping node with unknown reference: {reference}");
				logger.Log(LogLevel.Warning, element.ToString());

				return false;
			}

			var weakPtrNode = new WeakPtrNode
			{
				Name = element.Attribute(ReClassNetFile.XmlNameAttribute)?.Value ?? string.Empty,
				Comment = element.Attribute(ReClassNetFile.XmlCommentAttribute)?.Value ?? string.Empty
			};
			weakPtrNode.ChangeInnerNode(innerClass);

			node = weakPtrNode;

			return true;
		}

		/// <summary>Creates a xml element from the node. This method gets only called if <see cref="CanHandleNode(BaseNode node)"/> returned true.</summary>
		/// <param name="node">The node to create the xml element from.</param>
		/// <param name="logger">The logger used to output messages.</param>
		/// <returns>The xml element for the node.</returns>
		public XElement CreateElementFromNode(BaseNode node, ILogger logger)
		{
			return new XElement(
				ReClassNetFile.XmlNodeElement,
				new XAttribute(ReClassNetFile.XmlNameAttribute, node.Name ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlCommentAttribute, node.Comment ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlTypeAttribute, XmlType),
				new XAttribute(ReClassNetFile.XmlReferenceAttribute, (node as WeakPtrNode).InnerNode.Uuid.ToBase64String())
			);
		}
	}
}
