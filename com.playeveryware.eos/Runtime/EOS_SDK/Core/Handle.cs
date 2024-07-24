// Copyright Epic Games, Inc. All Rights Reserved.

using System;

namespace Epic.OnlineServices
{
	/// <summary>
	/// Represents an SDK handle.
	/// </summary>
	public abstract class Handle : IEquatable<Handle>, IFormattable
	{
		public IntPtr InnerHandle { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Handle" /> class.
		/// </summary>
		public Handle()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Handle" /> class with the given inner handle.
		/// </summary>
		public Handle(IntPtr innerHandle)
		{
			InnerHandle = innerHandle;
		}

		public static bool operator ==(Handle left, Handle right)
		{
			if (ReferenceEquals(left, null))
			{
				if (ReferenceEquals(right, null))
				{
					return true;
				}

				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(Handle left, Handle right)
		{
			return !(left == right);
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

		public override string ToString()
		{
			return InnerHandle.ToString();
		}

		public virtual string ToString(string format, IFormatProvider formatProvider)
		{
			if (format != null)
			{
				return InnerHandle.ToString(format);
			}

			return InnerHandle.ToString();
		}
	}
}