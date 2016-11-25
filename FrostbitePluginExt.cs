using System;
using System.Drawing;
using System.Text;
using ReClassNET;
using ReClassNET.Memory;
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

		private WeakPtrNodeConverter converter;
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
			converter = new WeakPtrNodeConverter();
			generator = new WeakPtrCodeGenerator();
			host.RegisterNodeType(typeof(WeakPtrNode), "Frostbite WeakPtr", Icon, converter, generator);

			return true;
		}

		public override void Terminate()
		{
			host.DeregisterNodeType(typeof(WeakPtrNode), converter, generator);

			host.DeregisterNodeInfoReader(reader);

			host = null;
		}
	}


	/// <summary>A custom node info reader which outputs Frostbite type infos.</summary>
	class FrostBiteNodeInfoReader : INodeInfoReader
	{
		public string ReadNodeInfo(BaseNode node, IntPtr value, MemoryBuffer memory)
		{
			// 1. try the direct value
			var info = ReadPtrInfo(value, memory);
			if (!string.IsNullOrEmpty(info))
			{
				return info;
			}

			// 2. try indirect pointer
			var indirectPtr = memory.Process.ReadRemoteObject<IntPtr>(value);
			if (indirectPtr.MayBeValid())
			{
				info = ReadPtrInfo(indirectPtr, memory);
				if (!string.IsNullOrEmpty(info))
				{
					return $"Ptr -> {info}";
				}

				// 3. try weak pointer
				var weakTempPtr = indirectPtr - IntPtr.Size;
				if (weakTempPtr.MayBeValid())
				{
					var weakPtr = memory.Process.ReadRemoteObject<IntPtr>(weakTempPtr);
					if (weakPtr.MayBeValid())
					{
						info = ReadPtrInfo(weakPtr, memory);
						if (!string.IsNullOrEmpty(info))
						{
							return $"WeakPtr -> {info}";
						}
					}
				}
			}

			return null;
		}

		private string ReadPtrInfo(IntPtr value, MemoryBuffer memory)
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
							var info = memory.Process.ReadRemoteUTF8StringUntilFirstNullCharacter(namePtr, 64);
							if (info.Length > 0 && info[0].IsPrintable())
							{
								return info;
							}
						}
					}
				}
			}

			return null;
		}
	}
}
