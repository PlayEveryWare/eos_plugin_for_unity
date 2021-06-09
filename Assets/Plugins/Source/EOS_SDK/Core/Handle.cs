// Copyright Epic Games, Inc. All Rights Reserved.

using System;

namespace Epic.OnlineServices
{
	public abstract class Handle : IEquatable<Handle>
	{
		public IntPtr InnerHandle { get; internal set; }

		public Handle()
		{
		}

		public Handle(IntPtr innerHandle)
		{
			InnerHandle = innerHandle;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Handle);
		}

		public override int GetHashCode()
		{
			return (int)(0x00010000 + InnerHandle.ToInt64());
		}

		public bool Equals(Handle other)
		{
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			if (GetType() != other.GetType())
			{
				return false;
			}

			return InnerHandle == other.InnerHandle;
		}

		public static bool operator ==(Handle lhs, Handle rhs)
		{
			if (ReferenceEquals(lhs, null))
			{
				if (ReferenceEquals(rhs, null))
				{
					return true;
				}

				return false;
			}

			return lhs.Equals(rhs);
		}

		public static bool operator !=(Handle lhs, Handle rhs)
		{
			return !(lhs == rhs);
		}
	}
}