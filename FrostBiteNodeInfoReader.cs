using System;
using System.Text;
using ReClassNET.Extensions;
using ReClassNET.Memory;
using ReClassNET.Nodes;

namespace FrostbitePlugin
{
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
