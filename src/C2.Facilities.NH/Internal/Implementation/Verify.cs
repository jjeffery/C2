#region License

// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;

namespace Castle.Facilities.NH.Internal.Implementation
{
	/// <summary>
	/// Utility class for simplifying checks.
	/// </summary>
	/// <remarks>
	/// Code borrowed from Quokka project. Could be replaced by contract API
	/// in .NET 4.0 and later.
	/// </remarks>
	internal static class Verify
	{
		public static T ArgumentNotNull<T>(T arg, string name) where T : class
		{
			if (arg == null)
			{
				throw new ArgumentNullException(name);
			}
			return arg;
		}
	}
}
