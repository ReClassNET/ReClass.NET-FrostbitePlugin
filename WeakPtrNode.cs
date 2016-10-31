using System;
using ReClassNET;
using ReClassNET.Nodes;
using ReClassNET.UI;
using ReClassNET.Util;

namespace FrostbitePlugin
{
	class WeakPtrNode : BaseReferenceNode
	{
		private readonly Memory memory = new Memory();

		/// <summary>Size of the node in bytes.</summary>
		public override int MemorySize => IntPtr.Size;


		/// <summary>Disable the cycle check for pointer references.</summary>
		public WeakPtrNode()
			: base(false)
		{

		}

		/// <summary>Called when the node was created. Creates a new class as inner node.</summary>
		public override void Intialize()
		{
			var node = ClassManager.CreateClass();
			node.Intialize();
			node.AddBytes(64);
			InnerNode = node;
		}

		/// <summary>Draws this node.</summary>
		/// <param name="view">The view information.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>The height the node occupies.</returns>
		public override int Draw(ViewInfo view, int x, int y)
		{
			if (IsHidden)
			{
				return DrawHidden(view, x, y);
			}

			AddSelection(view, x, y, view.Font.Height);
			AddDelete(view, x, y);
			AddTypeDrop(view, x, y);

			x = AddOpenClose(view, x, y);
			x = AddIcon(view, x, y, Icons.Pointer, -1, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(view, x, y);

			x = AddText(view, x, y, FrostbitePluginExt.Settings.TypeColor, HotSpot.NoneId, "WeakPtr") + view.Font.Width;
			x = AddText(view, x, y, FrostbitePluginExt.Settings.NameColor, HotSpot.NameId, Name) + view.Font.Width;
			x = AddText(view, x, y, FrostbitePluginExt.Settings.ValueColor, HotSpot.NoneId, $"<{InnerNode.Name}>");
			x = AddIcon(view, x, y, Icons.Change, 4, HotSpotType.ChangeType);

			x += view.Font.Width;
			AddComment(view, x, y);

			y += view.Font.Height;

			if (levelsOpen[view.Level])
			{
				var ptr = view.Memory.ReadObject<IntPtr>(Offset);
				if (!ptr.IsNull())
				{
					ptr = view.Memory.Process.ReadRemoteObject<IntPtr>(ptr);
					if (!ptr.IsNull())
					{
						ptr = ptr - IntPtr.Size;
					}
				}

				memory.Size = InnerNode.MemorySize;
				memory.Process = view.Memory.Process;
				memory.Update(ptr);

				var v = view.Clone();
				v.Address = ptr;
				v.Memory = memory;

				y = InnerNode.Draw(v, tx, y);
			}

			return y;
		}

		/// <summary>Calculates the height of the node.</summary>
		/// <param name="view">The view information.</param>
		/// <returns>The calculated height.</returns>
		public override int CalculateHeight(ViewInfo view)
		{
			if (IsHidden)
			{
				return HiddenHeight;
			}

			var h = view.Font.Height;
			if (levelsOpen[view.Level])
			{
				h += InnerNode.CalculateHeight(view);
			}
			return h;
		}
	}
}
