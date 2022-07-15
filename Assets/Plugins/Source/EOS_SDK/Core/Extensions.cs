// Copyright Epic Games, Inc. All Rights Reserved.

namespace Epic.OnlineServices
{
	public static class Extensions
	{
		/// <summary>
		/// Checks whether the given result indicates that the operation has completed. Some operations may callback with a result indicating that they will callback again.
		/// </summary>
		/// <param name="result">The result to check.</param>
		/// <returns>Whether the operation has completed or not.</returns>
		public static bool IsOperationComplete(this Result result)
		{
			return Common.IsOperationComplete(result);
		}

		/// <summary>
		/// Converts a byte array into a hex string, e.g. "A56904FF".
		/// </summary>
		/// <param name="byteArray">The byte array to convert.</param>
		/// <returns>A hex string, e.g. "A56904FF".</returns>
		public static string ToHexString(this byte[] byteArray)
		{
			var segment = new System.ArraySegment<byte>(byteArray);
			return Common.ToString(segment);
		}
	}
}