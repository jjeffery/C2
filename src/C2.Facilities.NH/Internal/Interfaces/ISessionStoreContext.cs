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

using System.Runtime.Remoting.Messaging;
using Castle.Facilities.NH.Internal.Implementation;

namespace Castle.Facilities.NH.Internal.Interfaces
{
	/// <summary>
	/// Used by <see cref="SessionStore{TSession}"/> for storing arbitrary
	/// data in the appropriate context. 
	/// </summary>
	/// <remarks>
	/// The default implementation is <see cref="CallContextSessionStoreContext"/>,
	/// which uses the <see cref="CallContext"/> class for storing information in
	/// the current call context. This is suitable for desktop and service applications,
	/// but is not suitable for web applications. It is very simple to write an implementation
	/// using the <c>HttpContext</c> but this assembly does not include an implementation, because
	/// that would require a dependency on the System.Web assembly.
	/// </remarks>
	public interface ISessionStoreContext
	{
		/// <summary>
		/// Get arbitrary data stored in the context.
		/// </summary>
		/// <param name="slotName">
		/// Identifies the slot where the data is stored.
		/// </param>
		/// <returns>
		/// Returns the data stored in the context slot, which may be <c>null</c>.
		/// </returns>
		object GetData(string slotName);

		/// <summary>
		/// Store arbitrary data in the context.
		/// </summary>
		/// <param name="slotName">
		/// Identifies the slot where the data is stored.
		/// </param>
		/// <param name="data">
		/// The data to store in the context slot, which may be <c>null</c>.
		/// </param>
		void SetData(string slotName, object data);
	}
}
