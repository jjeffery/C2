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
using System.Collections.Generic;
using Castle.Facilities.NH.Internal.Interfaces;

namespace Castle.Facilities.NH.Internal.Implementation
{
	/// <summary>
	/// Used for storing the current session for each session factory in the current context.
	/// </summary>
	/// <remarks>
	/// NHibernate already has a mechanism for storing the current session context, but this
	/// only applies to ISession objects, not IStatelessSession objects. This mechanism can
	/// be used for both.
	/// </remarks>
	/// <typeparam name="TSession"></typeparam>
	public class SessionStore<TSession> : ISessionStore<TSession> where TSession : class
	{
		private readonly ISessionStoreContext _sessionStoreContext;
		private readonly string _slotName = Guid.NewGuid().ToString();

		public SessionStore(ISessionStoreContext sessionStoreContext)
		{
			_sessionStoreContext = Verify.ArgumentNotNull(sessionStoreContext, "sessionStoreContext");
		}

		public TSession FindCompatibleSession(string alias)
		{
			Verify.ArgumentNotNull(alias, "alias");
			return GetContext().FindCompatibleSession(alias);
		}

		public void Add(TSession session, string alias)
		{
			Verify.ArgumentNotNull(session, "session");
			Verify.ArgumentNotNull(alias, "alias");
			GetContext().Add(session, alias);
		}

		public void Remove(TSession session, string alias)
		{
			Verify.ArgumentNotNull(session, "session");
			Verify.ArgumentNotNull(alias, "alias");
			GetContext().Remove(session, alias);
		}

		private SessionStoreContext GetContext()
		{
			var context = _sessionStoreContext.GetData(_slotName) as SessionStoreContext;
			if (context == null)
			{
				context = new SessionStoreContext();
				_sessionStoreContext.SetData(_slotName, context);
			}
			return context;
		}

		/// <summary>
		/// Contains all session storage information for a single context.
		/// </summary>
		/// <remarks>
		/// A context can be a web request, thread context, or call context.
		/// </remarks>
		private class SessionStoreContext
		{
			private readonly Dictionary<string, TSession> _sessionMap = new Dictionary<string, TSession>();

			public TSession FindCompatibleSession(string alias)
			{
				TSession session;
				_sessionMap.TryGetValue(alias, out session);
				return session;
			}

			public void Add(TSession session, string alias)
			{
				if (_sessionMap.ContainsKey(alias))
				{
					string msg = string.Format("Session ({0}) already exists for alias {1}", typeof (TSession), alias);
					throw new InvalidOperationException(msg);
				}

				_sessionMap.Add(alias, session);
			}

			public void Remove(TSession session, string alias)
			{
				TSession existingSession;
				_sessionMap.TryGetValue(alias, out existingSession);

				if (!ReferenceEquals(session, existingSession))
				{
					var msg = string.Format("Attempt to remove session ({0}) for alias {1} failed because session does not match",
					                        typeof (TSession), alias);
					throw new InvalidOperationException(msg);
				}

				_sessionMap.Remove(alias);
			}
		}
	}
}
