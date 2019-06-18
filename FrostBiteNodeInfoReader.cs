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
		public string ReadNodeInfo(BaseHexCommentNode node, IRemoteMemoryReader reader, MemoryBuffer memory, IntPtr nodeAddress, IntPtr nodeValue)
		{
			// 1. try the direct value
			var info = ReadPtrInfo(nodeValue, reader);
			if (!string.IsNullOrEmpty(info))
			{
				return info;
			}

			// 2. try indirect pointer
			var indirectPtr = reader.ReadRemoteIntPtr(nodeValue);
			if (indirectPtr.MayBeValid())
			{
				info = ReadPtrInfo(indirectPtr, reader);
				if (!string.IsNullOrEmpty(info))
				{
					return $"Ptr -> {info}";
				}

				// 3. try weak pointer
				var weakTempPtr = indirectPtr - IntPtr.Size;
				if (weakTempPtr.MayBeValid())
				{
					var weakPtr = reader.ReadRemoteIntPtr(weakTempPtr);
					if (weakPtr.MayBeValid())
					{
						info = ReadPtrInfo(weakPtr, reader);
						if (!string.IsNullOrEmpty(info))
						{
							return $"WeakPtr -> {info}";
						}
					}
				}
			}

			return null;
		}

		private static string ReadPtrInfo(IntPtr value, IRemoteMemoryReader process)
		{
			var getTypeFnPtr = process.ReadRemoteIntPtr(value);
			if (getTypeFnPtr.MayBeValid())
			{
#if RECLASSNET64
				var offset = process.ReadRemoteInt32(getTypeFnPtr + 3);
				var typeInfoPtr = getTypeFnPtr + offset + 7;
#else
				var typeInfoPtr = process.ReadRemoteIntPtr(getTypeFnPtr + 1);
#endif
				if (typeInfoPtr.MayBeValid())
				{
					var typeInfoDataPtr = process.ReadRemoteIntPtr(typeInfoPtr);
					if (typeInfoDataPtr.MayBeValid())
					{
						var namePtr = process.ReadRemoteIntPtr(typeInfoDataPtr);
						if (namePtr.MayBeValid())
						{
							var info = process.ReadRemoteStringUntilFirstNullCharacter(Encoding.UTF8, namePtr, 64);
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
