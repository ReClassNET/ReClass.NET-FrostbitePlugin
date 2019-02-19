using System.Collections.Generic;
using System.Xml.Linq;
using ReClassNET.DataExchange.ReClass;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	public class WeakPtrNodeConverter : ICustomNodeSerializer
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
		/// <returns>True if a node was created, otherwise false.</returns>
		public BaseNode CreateNodeFromElement(XElement element, BaseNode parent, IEnumerable<ClassNode> classes, ILogger logger, CreateNodeFromElementHandler defaultHandler)
		{
			return new WeakPtrNode();
		}

		/// <summary>Creates a xml element from the node. This method gets only called if <see cref="CanHandleNode(BaseNode)"/> returned true.</summary>
		/// <param name="node">The node to create the xml element from.</param>
		/// <param name="logger">The logger used to output messages.</param>
		/// <returns>The xml element for the node.</returns>
		public XElement CreateElementFromNode(BaseNode node, ILogger logger, CreateElementFromNodeHandler defaultHandler)
		{
			return new XElement(
				ReClassNetFile.XmlNodeElement,
				new XAttribute(ReClassNetFile.XmlTypeAttribute, XmlType)
			);
		}
	}
}
