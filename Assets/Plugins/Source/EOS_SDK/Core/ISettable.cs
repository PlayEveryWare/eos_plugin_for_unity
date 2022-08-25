// Copyright Epic Games, Inc. All Rights Reserved.

namespace Epic.OnlineServices
{
	internal interface ISettable<T> where T : struct
	{
		void Set(ref T other);
		void Set(ref T? other);
	}
}