using System;
using System.Drawing;
using System.Text;
using ReClassNET;
using ReClassNET.Nodes;
using ReClassNET.Plugins;
using ReClassNET.Util;

namespace FrostbitePlugin
{
	public class FrostbitePluginExt : Plugin
	{
		private IPluginHost host;

		internal static Settings Settings;

		private INodeInfoReader reader;

		private WeakPtrSchemaConverter converter;
		private WeakPtrCodeGenerator generator;

		public override Image Icon => Properties.Resources.logo_frostbite;

		public override bool Initialize(IPluginHost host)
		{
			//System.Diagnostics.Debugger.Launch();

			if (this.host != null)
			{
				Terminate();
			}

			if (host == null)
			{
				throw new ArgumentNullException(nameof(host));
			}

			this.host = host;

			Settings = host.Settings;

			// Register the InfoReader
			reader = new FrostBiteNodeInfoReader();
			host.RegisterNodeInfoReader(reader);

			// Register the WeakPtr node
			converter = new WeakPtrSchemaConverter();
			generator = new WeakPtrCodeGenerator();
			host.RegisterNodeType(typeof(WeakPtrNode), "Frostbite WeakPtr", Icon, converter, generator);

			return true;
		}

		public override void Terminate()
		{
			host.UnregisterNodeType(typeof(WeakPtrNode), converter, generator);

			host.UnregisterNodeInfoReader(reader);

			host = null;
		}
	}


	/// <summary>A custom node info reader which outputs Frostbite type infos.</summary>
	class FrostBiteNodeInfoReader : INodeInfoReader
	{
		public string ReadNodeInfo(BaseNode node, IntPtr value, Memory memory)
		{
			var getTypeFnPtr = memory.Process.ReadRemoteObject<IntPtr>(value);
			if (getTypeFnPtr.MayBeValid())
			{
#if WIN64
				var offset = memory.Process.ReadRemoteObject<int>(getTypeFnPtr + 3);
				var typeInfoPtr = getTypeFnPtr + offset + 7;
#else
				var typeInfoPtr = memory.Process.ReadRemoteObject<IntPtr>(getTypeFnPtr + 1);
#endif
				if (typeInfoPtr.MayBeValid())
				{
					var typeInfoDataPtr = memory.Process.ReadRemoteObject<IntPtr>(typeInfoPtr);
					if (typeInfoDataPtr.MayBeValid())
					{
						var namePtr = memory.Process.ReadRemoteObject<IntPtr>(typeInfoDataPtr);
						if (namePtr.MayBeValid())
						{
							return memory.Process.ReadRemoteString(Encoding.UTF8, namePtr, 64);
						}
					}
				}
			}

			return null;
		}
	}
}
