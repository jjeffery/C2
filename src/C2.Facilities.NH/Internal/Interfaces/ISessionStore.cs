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

using NHibernate;

namespace Castle.Facilities.NH.Internal.Interfaces
{
	/// <summary>
	/// Used for storing a session in the current context. The current
	/// context can be the web request or call context, depending on whether
	/// the application is a web application or not.
	/// </summary>
	/// <typeparam name="TSession">
	/// Type of session. Will be either <see cref="ISession"/> or
	/// <see cref="IStatelessSession"/>.
	/// </typeparam>
	/// <remarks>
	/// Two separate session stores are maintained, one for <see cref="ISession"/>
	/// objects and one for <see cref="IStatelessSession"/> objects. This generic
	/// interface is used to access both.
	/// </remarks>
	public interface ISessionStore<TSession> where TSession : class
	{
		/// <summary>
		/// Find an existing open session for the alias.
		/// </summary>
		/// <param name="alias">Session factory alias</param>
		/// <returns>
		/// Compatible session, or <c>null</c> if none exists
		/// </returns>
		TSession FindCompatibleSession(string alias);

		/// <summary>
		/// Add the session to the session store
		/// </summary>
		/// <param name="session">Open session</param>
		/// <param name="alias">Session factory alias associated with this session.</param>
		void Add(TSession session, string alias);

		/// <summary>
		/// Remove the session from the session store
		/// </summary>
		/// <param name="session">Open session</param>
		/// <param name="alias">Session factory alias associated with this session.</param>
		void Remove(TSession session, string alias);
	}
}
