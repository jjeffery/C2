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
		private readonly Dictionary<string, InstallerData> _installerDataMap = new Dictionary<string, InstallerData>();
		private InstallerData _defaultInstallerData;

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

			foreach (var installer in _kernel.ResolveAll<INHibernateInstaller>())
			{
				AddInstaller(installer);
			}

			_kernel.ComponentRegistered += KernelOnComponentRegistered;
		}

		private void KernelOnComponentRegistered(string key, IHandler handler)
		{
			if (typeof(INHibernateInstaller).IsAssignableFrom(handler.ComponentModel.Implementation))
			{
				if (!TryAddInstaller(handler))
				{
					_waitList.Add(handler);
				}
			}

			CheckWaitList();

		}

		private bool TryAddInstaller(IHandler handler)
		{
			var installer = (INHibernateInstaller)handler.TryResolve(CreationContext.CreateEmpty());
			if (installer == null)
			{
				return false;
			}

			AddInstaller(installer);
			return true;
		}

		private void CheckWaitList()
		{
			var handlers = _waitList.ToArray();
			foreach (var handler in handlers)
			{
				if (TryAddInstaller(handler))
				{
					_waitList.Remove(handler);
				}
			}
		}

		private void AddInstaller(INHibernateInstaller installer)
		{
			Verify.ArgumentNotNull(installer, "installer");

			if (_installerDataMap.ContainsKey(installer.SessionFactoryKey))
			{
				var msg = string.Format("Multiple INHibernateInstallers have alias of {0}: {1}, {2}",
				                        installer.SessionFactoryKey, installer.GetType(),
				                        _installerDataMap[installer.SessionFactoryKey].Installer.GetType());
				throw new NHibernateFacilityException(msg);
			}

			var installerData = new InstallerData(installer);

			_installerDataMap.Add(installer.SessionFactoryKey, installerData);

			if (installer.IsDefault)
			{
				if (_defaultInstallerData != null)
				{
					var msg = string.Format("Multiple INHibernateInstallers have IsDefault=true: {0}, {1}",
					                        installer.GetType(), _defaultInstallerData.Installer.GetType());
					throw new NHibernateFacilityException(msg);
				}
				_defaultInstallerData = installerData;
			}
		}

		public bool IsAliasDefined(string alias)
		{
			return _installerDataMap.ContainsKey(alias);
		}

		public string DefaultAlias
		{
			get
			{
				if (_defaultInstallerData == null)
				{
					return null;
				}
				return _defaultInstallerData.Installer.SessionFactoryKey;
			}
		}

		public ISessionFactory GetSessionFactory(string alias)
		{
			InstallerData installerData;

			if (string.IsNullOrWhiteSpace(alias))
			{
				installerData = _defaultInstallerData;
				if (installerData == null)
				{
					throw new NHibernateFacilityException("No default session factory defined");
				}
			}
			else
			{
				if (!_installerDataMap.TryGetValue(alias, out installerData))
				{
					throw new NHibernateFacilityException("Unknown session factory alias: " + alias);
				}
			}

			return installerData.SessionFactory;
		}

		#region class InstallerData

		private class InstallerData
		{
			public InstallerData(INHibernateInstaller installer)
			{
				Installer = installer;
			}

			private readonly object _lockObject = new object();
			private volatile ISessionFactory _sessionFactory;
			private volatile Configuration _configuration;

			public INHibernateInstaller Installer { get; private set; }
			public Configuration Configuration
			{
				get
				{
					if (_configuration != null)
					{
						return _configuration;
					}

					lock (_lockObject)
					{
						if (_configuration == null)
						{
							_configuration = Installer.BuildConfiguration();
						}
					}

					return _configuration;
				}
			}
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
							var cfg = Configuration;
							var sessionFactory = cfg.BuildSessionFactory();
							Installer.Registered(sessionFactory, cfg);
							_sessionFactory = sessionFactory;
						}
					}

					return _sessionFactory;
				}
			}
		}

		#endregion
	}
}
