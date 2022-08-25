// Copyright Epic Games, Inc. All Rights Reserved.

using System;
using System.Linq;
using System.Text;

namespace Epic.OnlineServices
{
	public sealed partial class Helper
	{
		/// <summary>
		/// Converts an <see cref="IntPtr" /> to a <see cref="Handle" /> of the specified <typeparamref name="THandle"/>.
		/// </summary>
		/// <typeparam name="THandle">The type of <see cref="Handle" /> to convert to.</typeparam>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert<THandle>(IntPtr from, out THandle to)
			where THandle : Handle, new()
		{
			to = null;

			if (from != IntPtr.Zero)
			{
				to = new THandle();
				to.InnerHandle = from;
			}
		}

		/// <summary>
		/// Converts a <see cref="Handle" /> to an <see cref="IntPtr" />.
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(Handle from, out IntPtr to)
		{
			to = IntPtr.Zero;

			if (from != null)
			{
				to = from.InnerHandle;
			}
		}

		/// <summary>
		/// Converts from a <see cref="byte" />[] to a <see cref="string" />.
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(byte[] from, out string to)
		{
			to = null;

			if (from == null)
			{
				return;
			}

			to = Encoding.ASCII.GetString(from.Take(GetAnsiStringLength(from)).ToArray());
		}

		/// <summary>
		/// Converts from a <see cref="string" /> of the specified length to a <see cref="byte" />[].
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="fromLength">The length to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(string from, out byte[] to, int fromLength)
		{
			if (from == null)
			{
				from = "";
			}

			to = Encoding.ASCII.GetBytes(new string(from.Take(fromLength).ToArray()).PadRight(fromLength, '\0'));
		}

		/// <summary>
		/// Converts from a <typeparamref name="TArray"/>[] to an <see cref="int" />.
		/// Outputs the length of the <typeparamref name="TArray"/>[].
		/// </summary>
		/// <typeparam name="TArray">The type of <see cref="Array" /> to convert from.</typeparam>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value; the length of the <typeparamref name="TArray"/>[].</param>
		private static void Convert<TArray>(TArray[] from, out int to)
		{
			to = 0;

			if (from != null)
			{
				to = from.Length;
			}
		}

		/// <summary>
		/// Converts from a <typeparamref name="TArray"/>[] to an <see cref="uint" />.
		/// Outputs the length of the <typeparamref name="TArray"/>[].
		/// </summary>
		/// <typeparam name="TArray">The type of <see cref="Array" /> to convert from.</typeparam>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value; the length of the <typeparamref name="TArray"/>[].</param>
		private static void Convert<TArray>(TArray[] from, out uint to)
		{
			to = 0;

			if (from != null)
			{
				to = (uint)from.Length;
			}
		}

		/// <summary>
		/// Converts from an <see cref="ArraySegment{TArray}" /> to an <see cref="int" />.
		/// Outputs the length of the <see cref="ArraySegment{TArray}" />.
		/// </summary>
		/// <typeparam name="TArray">The type of the <see cref="Array" />.</typeparam>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value; the length of the <see cref="ArraySegment{TArray}" />.</param>
		private static void Convert<TArray>(ArraySegment<TArray> from, out int to)
		{
			to = from.Count;
		}

		/// <summary>
		/// Converts from an <see cref="ArraySegment{TArray}" /> to an <see cref="uint" />.
		/// Outputs the length of the <see cref="ArraySegment{TArray}" />.
		/// </summary>
		/// <typeparam name="TArray">The type of the <see cref="Array" />.</typeparam>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value; the length of the <see cref="ArraySegment{TArray}" />.</param>
		private static void Convert<T>(ArraySegment<T> from, out uint to)
		{
			to = (uint)from.Count;
		}

		/// <summary>
		/// Converts from an <see cref="int" /> to a <see cref="bool" />. 
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(int from, out bool to)
		{
			to = from != 0;
		}

		/// <summary>
		/// Converts from an <see cref="bool" /> to an <see cref="int" />. 
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(bool from, out int to)
		{
			to = from ? 1 : 0;
		}

		/// <summary>
		/// Converts from a <see cref="DateTimeOffset" />? to a <see cref="long" />.
		/// Outputs the number of seconds represented by the <see cref="DateTimeOffset" />? as a unix timestamp.
		/// A <see langword="null" /> <see cref="DateTimeOffset" />? equates to a value of -1, which means unset in the SDK.
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(DateTimeOffset? from, out long to)
		{
			to = -1;

			if (from.HasValue)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				long unixTimestampTicks = (from.Value.UtcDateTime - unixStart).Ticks;
				long unixTimestampSeconds = unixTimestampTicks / TimeSpan.TicksPerSecond;
				to = unixTimestampSeconds;
			}
		}

		/// <summary>
		/// Converts from a <see cref="long" /> to a <see cref="DateTimeOffset" />?.
		/// </summary>
		/// <param name="from">The value to convert from.</param>
		/// <param name="to">The converted value.</param>
		private static void Convert(long from, out DateTimeOffset? to)
		{
			to = null;

			if (from >= 0)
			{
				DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				long unixTimeStampTicks = from * TimeSpan.TicksPerSecond;
				to = new DateTimeOffset(unixStart.Ticks + unixTimeStampTicks, TimeSpan.Zero);
			}
		}
	}
}