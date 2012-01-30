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
using Castle.Facilities.NH.Internal.Implementation;

namespace Castle.Facilities.NH.Internal.Interfaces
{
	/// <summary>
	/// An in-memory store which keeps track of type meta-information required
	/// for implementing the interceptor for classes with one or more 
	/// [NHTransaction] attributes.
	/// </summary>
	public interface ITransactionalInfoStore
	{
		/// <summary>
		/// Returns the meta-information for a given type. If the information
		/// is already cached for this type this is a very quick operation.
		/// Otherwise the type meta-information is inspected and the results
		/// are cached.
		/// </summary>
		/// <param name="type">Type</param>
		TransactionMetaInfo GetTransactionalInfo(Type type);
	}
}
