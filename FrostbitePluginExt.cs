using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ReClassNET;
using ReClassNET.Extensions;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.Plugins;

namespace FrostbitePlugin
{
	public class FrostbitePluginExt : Plugin
	{
		private IPluginHost host;

		internal static Settings Settings;

		public override Image Icon => Properties.Resources.logo_frostbite;

		public override bool Initialize(IPluginHost host)
		{
			//System.Diagnostics.Debugger.Launch();

			if (this.host != null)
			{
				Terminate();
			}

			this.host = host ?? throw new ArgumentNullException(nameof(host));

			Settings = host.Settings;

			return true;
		}

		public override void Terminate()
		{
			host = null;
		}

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
