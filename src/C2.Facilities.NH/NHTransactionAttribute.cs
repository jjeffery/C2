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
using System.Data;

namespace Castle.Facilities.NH
{
	/// <summary>
	/// Indicates that an NHibernate transaction is required 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class NHTransactionAttribute : Attribute
	{
		public NHTransactionAttribute() : this(null) {}

		public NHTransactionAttribute(string alias)
		{
			Alias = alias;
			IsolationLevel = IsolationLevel.Unspecified;
		}

		public IsolationLevel IsolationLevel { get; set; }
		public string Alias { get; private set; }
	}
}
