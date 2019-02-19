using System;
using System.Drawing;
using ReClassNET.Extensions;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.UI;

namespace FrostbitePlugin
{
	public class WeakPtrNode : BaseClassWrapperNode
	{
		private readonly MemoryBuffer memory = new MemoryBuffer();

		/// <summary>Size of the node in bytes.</summary>
		public override int MemorySize => IntPtr.Size;

		/// <summary>Disable the cycle check for pointers.</summary>
		protected override bool PerformCycleCheck => false;

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "Weak Pointer";
			icon = Properties.Resources.logo_frostbite;
		}

		/// <summary>Called when the node was created. Creates a new class as inner node.</summary>
		public override void Initialize()
		{
			var node = ClassNode.Create();
			node.Initialize();
			node.AddBytes(64);
			InnerNode = node;
		}

		/// <summary>Draws this node.</summary>
		/// <param name="view">The view information.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>The pixel size the node occupies.</returns>
		public override Size Draw(ViewInfo view, int x, int y)
		{
			if (IsHidden && !IsWrapped)
			{
				return DrawHidden(view, x, y);
			}

			var origX = x;
			var origY = y;

			AddSelection(view, x, y, view.Font.Height);

			x = AddOpenClose(view, x, y);
			x = AddIcon(view, x, y, Icons.Pointer, -1, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(view, x, y);

			x = AddText(view, x, y, view.Settings.TypeColor, HotSpot.NoneId, "WeakPtr") + view.Font.Width;
			x = AddText(view, x, y, view.Settings.NameColor, HotSpot.NameId, Name) + view.Font.Width;
			x = AddText(view, x, y, view.Settings.ValueColor, HotSpot.NoneId, $"<{InnerNode.Name}>");
			x = AddIcon(view, x, y, Icons.Change, 4, HotSpotType.ChangeClassType);

			x += view.Font.Width;

			AddComment(view, x, y);

			DrawInvalidMemoryIndicator(view, y);
			AddTypeDrop(view, y);
			AddDelete(view, y);

			y += view.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[view.Level])
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

				var innerSize = InnerNode.Draw(v, tx, y);

				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		/// <summary>Calculates the height of the node.</summary>
		/// <param name="view">The view information.</param>
		/// <returns>The calculated height.</returns>
		public override int CalculateDrawnHeight(ViewInfo view)
		{
			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var h = view.Font.Height;
			if (LevelsOpen[view.Level])
			{
				h += InnerNode.CalculateDrawnHeight(view);
			}
			return h;
		}
	}
}
