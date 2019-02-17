using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
	public class WeakPtrCodeGenerator : ICustomCppCodeGenerator
	{
		public bool CanHandle(BaseNode node)
		{
			return node is WeakPtrNode;
		}

		public BaseNode TransformNode(BaseNode node)
		{
			return node;
		}

		public string GetTypeDefinition(BaseNode node, GetTypeDefinitionFunc defaultGetTypeDefinitionFunc, ResolveWrappedTypeFunc defaultResolveWrappedTypeFunc, ILogger logger)
		{
			return $"fb::WeakPtr<class {((ClassNode)((WeakPtrNode)node).InnerNode).Name}>";
		}
	}
}
