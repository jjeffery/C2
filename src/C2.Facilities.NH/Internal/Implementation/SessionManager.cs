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

using Castle.Facilities.NH.Internal.Interfaces;
using NHibernate;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class SessionManager : ISessionManager
	{
		private readonly ISessionStore<ISession> _sessionStore;
		private readonly ISessionStore<IStatelessSession> _statelessSessionStore;
		private readonly ISessionFactoryResolver _sessionFactoryResolver;

		public SessionManager(
			ISessionStore<ISession> sessionStore, 
			ISessionStore<IStatelessSession> statelessSessionStore,
			ISessionFactoryResolver sessionFactoryResolver)
		{
			_sessionStore = Verify.ArgumentNotNull(sessionStore, "sessionStore");
			_statelessSessionStore = Verify.ArgumentNotNull(statelessSessionStore, "statelessSessionStore");
			_sessionFactoryResolver = Verify.ArgumentNotNull(sessionFactoryResolver, "sessionFactoryResolver");
		}

		public ISession OpenSession(string alias)
		{
			alias = NormaliseAlias(alias);
			var canClose = false;
			var session = _sessionStore.FindCompatibleSession(alias);
			if (session == null)
			{
				var sessionFactory = _sessionFactoryResolver.GetSessionFactory(alias);
				session = sessionFactory.OpenSession();
				canClose = true;
				_sessionStore.Add(session, alias);
			}

			var sessionDelegate = new SessionDelegate(canClose, session);
			if (canClose)
			{
				sessionDelegate.Closed += (o, e) => _sessionStore.Remove(sessionDelegate.InnerSession, alias);
			}

			return sessionDelegate;
		}

		public IStatelessSession OpenStatelessSession(string alias)
		{
			alias = NormaliseAlias(alias);
			var canClose = false;
			var session = _statelessSessionStore.FindCompatibleSession(alias);
			if (session == null)
			{
				var sessionFactory = _sessionFactoryResolver.GetSessionFactory(alias);
				session = sessionFactory.OpenStatelessSession();
				canClose = true;
				_statelessSessionStore.Add(session, alias);
			}

			var sessionDelegate = new StatelessSessionDelegate(canClose, session);
			if (canClose)
			{
				sessionDelegate.Closed += (o, e) => _statelessSessionStore.Remove(sessionDelegate.InnerSession, alias);
			}

			return sessionDelegate;
		}

		private string NormaliseAlias(string alias)
		{
			if (alias == null)
			{
				alias = _sessionFactoryResolver.DefaultAlias;
				if (alias == null)
				{
					throw new NHibernateFacilityException("No default alias is defined");
				}
			}
			return alias;
		}
	}
}
