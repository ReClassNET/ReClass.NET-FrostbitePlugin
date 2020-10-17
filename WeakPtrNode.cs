using System;
using System.Drawing;
using ReClassNET.Controls;
using ReClassNET.Extensions;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.UI;

namespace FrostbitePlugin
{
	public class WeakPtrNode : BaseClassWrapperNode
	{
		private readonly MemoryBuffer memory = new MemoryBuffer();

		public override int MemorySize => IntPtr.Size;

		protected override bool PerformCycleCheck => false;

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "Weak Pointer";
			icon = Properties.Resources.logo_frostbite;
		}

		public override void Initialize()
		{
			var node = ClassNode.Create();
			node.Initialize();
			node.AddBytes(64);
			ChangeInnerNode(node);
		}

		public override Size Draw(DrawContext context, int x, int y)
		{
			if (IsHidden && !IsWrapped)
			{
				return DrawHidden(context, x, y);
			}

			var origX = x;
			var origY = y;

			AddSelection(context, x, y, context.Font.Height);

			x = AddOpenCloseIcon(context, x, y);
			x = AddIcon(context, x, y, context.IconProvider.Pointer, -1, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(context, x, y);

			x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "WeakPtr") + context.Font.Width;
			x = AddText(context, x, y, context.Settings.NameColor, HotSpot.NameId, Name) + context.Font.Width;
			x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, $"<{InnerNode.Name}>");
			x = AddIcon(context, x, y, context.IconProvider.Change, 4, HotSpotType.ChangeClassType);

			x += context.Font.Width;

			AddComment(context, x, y);

			DrawInvalidMemoryIndicatorIcon(context, y);
			AddContextDropDownIcon(context, y);
			AddDeleteIcon(context, y);

			y += context.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[context.Level])
			{
				var ptr = context.Memory.ReadObject<IntPtr>(Offset);
				if (!ptr.IsNull())
				{
					ptr = context.Process.ReadRemoteObject<IntPtr>(ptr);
					if (!ptr.IsNull())
					{
						ptr -= IntPtr.Size;
					}
				}

				memory.Size = InnerNode.MemorySize;
				memory.UpdateFrom(context.Process, ptr);

				var v = context.Clone();
				v.Address = ptr;
				v.Memory = memory;

				var innerSize = InnerNode.Draw(v, tx, y);

				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		public override int CalculateDrawnHeight(DrawContext context)
		{
			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var h = context.Font.Height;
			if (LevelsOpen[context.Level])
			{
				h += InnerNode.CalculateDrawnHeight(context);
			}
			return h;
		}
	}
}
