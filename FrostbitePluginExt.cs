using System.Collections.Generic;
using System.Drawing;
using ReClassNET.Nodes;
using ReClassNET.Plugins;

namespace FrostbitePlugin
{
	public class FrostbitePluginExt : Plugin
	{
		public override Image Icon => Properties.Resources.logo_frostbite;

		public override IReadOnlyList<INodeInfoReader> GetNodeInfoReaders()
		{
			// Register the InfoReader

			return new[] { new FrostBiteNodeInfoReader() };
		}

		public override CustomNodeTypes GetCustomNodeTypes()
		{
			// Register the WeakPtr node

			return new CustomNodeTypes
			{
				CodeGenerator = new WeakPtrCodeGenerator(),
				Serializer = new WeakPtrNodeConverter(),
				NodeTypes = new[] { typeof(WeakPtrNode) }
			};
		}
	}
}
