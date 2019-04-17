using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	public class WeakPtrCodeGenerator : CustomCppCodeGenerator
	{
		public override bool CanHandle(BaseNode node)
		{
			return node is WeakPtrNode;
		}

		public override BaseNode TransformNode(BaseNode node)
		{
			return node;
		}

		public override string GetTypeDefinition(BaseNode node, GetTypeDefinitionFunc defaultGetTypeDefinitionFunc, ResolveWrappedTypeFunc defaultResolveWrappedTypeFunc, ILogger logger)
		{
			return $"fb::WeakPtr<class {((ClassNode)((WeakPtrNode)node).InnerNode).Name}>";
		}
	}
}
