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

	/// <summary>A custom node info reader which outputs Frostbite type infos.</summary>
	public class FrostBiteNodeInfoReader : INodeInfoReader
	{
		public string ReadNodeInfo(BaseHexCommentNode node, IntPtr nodeAddress, IntPtr nodeValue, MemoryBuffer memory)
		{
			// 1. try the direct value
			var info = ReadPtrInfo(nodeValue, memory);
			if (!string.IsNullOrEmpty(info))
			{
				return info;
			}

			// 2. try indirect pointer
			var indirectPtr = memory.Process.ReadRemoteIntPtr(nodeValue);
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
					var weakPtr = memory.Process.ReadRemoteIntPtr(weakTempPtr);
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

		private static string ReadPtrInfo(IntPtr value, MemoryBuffer memory)
		{
			var getTypeFnPtr = memory.Process.ReadRemoteIntPtr(value);
			if (getTypeFnPtr.MayBeValid())
			{
#if RECLASSNET64
				var offset = memory.Process.ReadRemoteInt32(getTypeFnPtr + 3);
				var typeInfoPtr = getTypeFnPtr + offset + 7;
#else
				var typeInfoPtr = memory.Process.ReadRemoteIntPtr(getTypeFnPtr + 1);
#endif
				if (typeInfoPtr.MayBeValid())
				{
					var typeInfoDataPtr = memory.Process.ReadRemoteIntPtr(typeInfoPtr);
					if (typeInfoDataPtr.MayBeValid())
					{
						var namePtr = memory.Process.ReadRemoteIntPtr(typeInfoDataPtr);
						if (namePtr.MayBeValid())
						{
							var info = memory.Process.ReadRemoteStringUntilFirstNullCharacter(Encoding.UTF8, namePtr, 64);
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
