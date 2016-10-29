using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	class WeakPtrCodeGenerator : ICustomCodeGenerator
	{
		/// <summary>Checks if the language is C++ and the node is a WeakPtrNode.</summary>
		/// <param name="node">The node to check.</param>
		/// <param name="language">The language to check.</param>
		/// <returns>True if we can generate code, false if not.</returns>
		public bool CanGenerateCode(BaseNode node, Language language) => language == Language.Cpp && node is WeakPtrNode;

		/// <summary>Gets the member definition of the node.</summary>
		/// <param name="node">The member node.</param>
		/// <param name="language">The language to generate.</param>
		/// <returns>The member definition of the node.</returns>
		public MemberDefinition GetMemberDefinition(BaseNode node, Language language, ILogger logger)
		{
			return new MemberDefinition(node, $"fb::WeakPtr<{((BaseReferenceNode)node).InnerNode.Name}>");
		}
	}
}
