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

using System.Collections.Generic;
using System.Linq;
using Castle.Core.Logging;
using Castle.Facilities.NH.Internal.Interfaces;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using NHibernate;
using NHibernate.Cfg;
using ILoggerFactory = Castle.Core.Logging.ILoggerFactory;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class SessionFactoryResolver : ISessionFactoryResolver
	{
		public ILogger Logger { get; set; }

		private readonly IKernel _kernel;
		private readonly IList<IHandler> _waitList = new List<IHandler>();
		private readonly Dictionary<string, DatabaseSettings> _databaseMap = new Dictionary<string, DatabaseSettings>();
		private DatabaseSettings _defaultDatabaseSettings;

		public SessionFactoryResolver(IKernel kernel)
		{
			_kernel = Verify.ArgumentNotNull(kernel, "kernel");
			if (_kernel.HasComponent(typeof(ILoggerFactory)))
			{
				var loggerFactory = _kernel.Resolve<ILoggerFactory>();
				Logger = loggerFactory.Create(typeof(SessionFactoryResolver));
			}
			else
			{
				Logger = NullLogger.Instance;
			}

			foreach (var builder in _kernel.ResolveAll<IConfigurationBuilder>())
			{
				AddBuilder(builder);
			}

			_kernel.ComponentRegistered += KernelOnComponentRegistered;
		}

		private void KernelOnComponentRegistered(string key, IHandler handler)
		{
			if (typeof(IConfigurationBuilder).IsAssignableFrom(handler.ComponentModel.Implementation))
			{
				if (!TryAddBuilder(handler))
				{
					_waitList.Add(handler);
				}
			}

			CheckWaitList();
		}

		private bool TryAddBuilder(IHandler handler)
		{
			var builder = (IConfigurationBuilder)handler.TryResolve(CreationContext.CreateEmpty());
			if (builder == null)
			{
				return false;
			}

			AddBuilder(builder);
			return true;
		}

		private void CheckWaitList()
		{
			var handlers = _waitList.ToArray();
			foreach (var handler in handlers)
			{
				if (TryAddBuilder(handler))
				{
					_waitList.Remove(handler);
				}
			}
		}

		private void AddBuilder(IConfigurationBuilder builder)
		{
			Verify.ArgumentNotNull(builder, "builder");

			if (_databaseMap.ContainsKey(builder.Alias))
			{
				var msg = string.Format("Multiple IConfigurationBuilders have alias of {0}: {1}, {2}",
				                        builder.Alias, builder.GetType(),
				                        _databaseMap[builder.Alias].Builder.GetType());
				throw new NHibernateFacilityException(msg);
			}

			var databaseSettings = new DatabaseSettings(builder, _kernel);

			_databaseMap.Add(builder.Alias, databaseSettings);

			if (builder.IsDefault)
			{
				if (_defaultDatabaseSettings != null)
				{
					var msg = string.Format("Multiple IConfigurationBuilders have IsDefault=true: {0}, {1}",
					                        builder.GetType(), _defaultDatabaseSettings.Builder.GetType());
					throw new NHibernateFacilityException(msg);
				}
				_defaultDatabaseSettings = databaseSettings;
			}
		}

		public bool IsAliasDefined(string alias)
		{
			return _databaseMap.ContainsKey(alias);
		}

		public string DefaultAlias
		{
			get
			{
				if (_defaultDatabaseSettings == null)
				{
					return null;
				}
				return _defaultDatabaseSettings.Builder.Alias;
			}
		}

		public ISessionFactory GetSessionFactory(string alias)
		{
			DatabaseSettings databaseSettings;

			if (string.IsNullOrEmpty(alias))
			{
				databaseSettings = _defaultDatabaseSettings;
				if (databaseSettings == null)
				{
					throw new NHibernateFacilityException("No default session factory defined");
				}
			}
			else
			{
				if (!_databaseMap.TryGetValue(alias, out databaseSettings))
				{
					throw new NHibernateFacilityException("Unknown session factory alias: " + alias);
				}
			}

			return databaseSettings.SessionFactory;
		}

		#region class DatabaseSettings

		private class DatabaseSettings
		{
			public DatabaseSettings(IConfigurationBuilder builder, IKernel kernel)
			{
				Builder = builder;
				_kernel = kernel;
			}

			private readonly object _lockObject = new object();
			private volatile ISessionFactory _sessionFactory;
			private readonly IKernel _kernel;

			public IConfigurationBuilder Builder { get; private set; }

			public ISessionFactory SessionFactory
			{
				get
				{
					if (_sessionFactory != null)
					{
						return _sessionFactory;
					}

					lock (_lockObject)
					{
						if (_sessionFactory == null)
						{
							var cfg = GetConfiguration();
							var sessionFactory = cfg.BuildSessionFactory();
							Builder.Registered(sessionFactory, cfg);
							_sessionFactory = sessionFactory;
						}
					}

					return _sessionFactory;
				}
			}

			private Configuration GetConfiguration()
			{
				var configuration = Builder.BuildConfiguration();
				foreach (var contributor in _kernel.ResolveAll<IConfigurationContributor>())
				{
					contributor.Contribute(Builder.Alias, Builder.IsDefault, configuration);
					_kernel.ReleaseComponent(contributor);
				}

				return configuration;
			}
		}

		#endregion
	}
}
