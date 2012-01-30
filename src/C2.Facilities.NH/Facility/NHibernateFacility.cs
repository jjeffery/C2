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

using Castle.Core.Logging;
using Castle.Facilities.NH.Internal.Implementation;
using Castle.Facilities.NH.Internal.Interfaces;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using NHibernate;
using ILoggerFactory = Castle.Core.Logging.ILoggerFactory;

namespace Castle.Facilities.NH.Facility
{
	public class NHibernateFacility : AbstractFacility
	{
		private ILogger _logger;
		private ILoggerFactory _loggerFactory = new NullLogFactory();

		protected override void Init()
		{
			// Create a logger if there is a logger factory.
			if (Kernel.HasComponent(typeof(ILoggerFactory)))
			{
				_loggerFactory = Kernel.Resolve<ILoggerFactory>();
			}
			_logger = _loggerFactory.Create(typeof(NHibernateFacility));


			_logger.Debug("Initializing NHibernateFacility");

			RegisterTransactionalInfoStore();
			RegisterTransactionInterceptor();
			RegisterSessionStoreContext();
			RegisterSessionStores();
			RegisterSessionFactoryResolver();
			RegisterSessionManager();
			AddContributors();

			_logger.Debug("NHibernateFacility is initialized");
		}

		private void RegisterTransactionalInfoStore()
		{
			Kernel.Register(Component.For<ITransactionalInfoStore>()
			                	.ImplementedBy<TransactionMetaInfoStore>()
			                	.LifestyleSingleton());
		}

		private void RegisterTransactionInterceptor()
		{
			Kernel.Register(Component.For<NHTransactionInterceptor>().LifestyleTransient());
		}

		private void RegisterSessionStoreContext()
		{
			if (!Kernel.HasComponent(typeof(ISessionStoreContext)))
			{
				Kernel.Register(Component.For<ISessionStoreContext>()
				                	.ImplementedBy<CallContextSessionStoreContext>()
				                	.LifestyleSingleton());
			}
		}

		private void RegisterSessionStores()
		{
			if (!Kernel.HasComponent(typeof(ISessionStore<ISession>)))
			{
				Kernel.Register(Component.For<ISessionStore<ISession>>()
				                	.ImplementedBy<SessionStore<ISession>>()
				                	.LifestyleSingleton());
			}

			if (!Kernel.HasComponent(typeof(ISessionStore<IStatelessSession>)))
			{
				Kernel.Register(Component.For<ISessionStore<IStatelessSession>>()
									.ImplementedBy<SessionStore<IStatelessSession>>()
									.LifestyleSingleton());
			}
		}

		private void RegisterSessionFactoryResolver()
		{
			var sessionFactoryResolver = new SessionFactoryResolver(Kernel);
			Kernel.Register(Component.For<ISessionFactoryResolver>().Instance(sessionFactoryResolver));
		}

		private void RegisterSessionManager()
		{
			if (!Kernel.HasComponent(typeof(ISessionManager)))
			{
				Kernel.Register(Component.For<ISessionManager>()
				                	.ImplementedBy<SessionManager>()
				                	.LifestyleSingleton());
			}
		}

		private void AddContributors()
		{
			Kernel.ComponentModelBuilder.AddContributor(new NHTransactionalComponentInspector());
		}
	}
}
