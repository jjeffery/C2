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
using Castle.Facilities.NH.Tests.Support;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Castle.Facilities.NH.Tests
{
	[TestFixture]
	public class SessionFactoryResolverTests
	{
		[Test]
		public void Installers_defined_before_SessionFactoryResolver_are_processed()
		{
			var container = new WindsorContainer();

			// Register installer prior to registering 
			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<DefaultInstaller>()
				);

			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);
			Assert.IsTrue(sessionFactoryResolver.IsAliasDefined("default-installer"));
		}

		[Test]
		public void Installers_defined_after_SessionFactoryResolver_are_processed()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			// Register installer prior to registering 
			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<DefaultInstaller>()
				);

			Assert.IsTrue(sessionFactoryResolver.IsAliasDefined("default-installer"));
		}

		[Test]
		public void Installers_with_dependencies_are_processed_when_possible()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			// Register installer prior to registering 
			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<InstallerWithDependency>()
				);

			Assert.IsFalse(sessionFactoryResolver.IsAliasDefined("installer-with-dependency"));

			container.Register(Component.For<DependencyClass>());

			Assert.IsTrue(sessionFactoryResolver.IsAliasDefined("installer-with-dependency"));
		}

		[Test]
		public void No_default_installer()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			Assert.IsNull(sessionFactoryResolver.DefaultAlias);

			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<NonDefaultInstaller>()
				);

			Assert.IsNull(sessionFactoryResolver.DefaultAlias);

			try
			{
				var sessionFactory = sessionFactoryResolver.GetSessionFactory(null);
				Assert.Fail("Exception expected");
			}
			catch (NHibernateFacilityException ex)
			{
				Assert.AreEqual("No default session factory defined", ex.Message);
			}
		}

		[Test]
		public void Unknown_alias()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<NonDefaultInstaller>()
				);

			try
			{
				var sessionFactory = sessionFactoryResolver.GetSessionFactory("some-random-alias");
				Assert.Fail("Exception expected");
			}
			catch (NHibernateFacilityException ex)
			{
				Assert.AreEqual("Unknown session factory alias: some-random-alias", ex.Message);
			}
		}

		[Test]
		public void Default_installer_recognised()
		{
			using (var container = new WindsorContainer())
			{
				var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

				// Register installer prior to registering 
				container.Register(
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<TestConfigurationBuilder>(),
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<NonDefaultInstaller>()
					);

				Assert.AreEqual("testdb", sessionFactoryResolver.DefaultAlias);

				var testdbSessionFactory = sessionFactoryResolver.GetSessionFactory("testdb");
				var defaultSessionFactory = sessionFactoryResolver.GetSessionFactory(null);

				Assert.AreSame(testdbSessionFactory, defaultSessionFactory);
			}
		}

		[Test]
		public void Throws_exception_when_two_default_installers()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			// Register installer prior to registering 
			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<DefaultInstaller>()
				);

			try
			{
				// Register installer prior to registering 
				container.Register(
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<AnotherDefaultInstaller>()
					);
				Assert.Fail("Exception expected");
			}
			catch (NHibernateFacilityException ex)
			{
				Assert.IsTrue(ex.Message.StartsWith("Multiple IConfigurationBuilders have IsDefault=true:"));
			}
		}

		[Test]
		public void Throws_exception_when_two_installers_have_the_same_name()
		{
			var container = new WindsorContainer();
			var sessionFactoryResolver = new SessionFactoryResolver(container.Kernel);

			// Register installer prior to registering 
			container.Register(
				Component.For<IConfigurationBuilder>()
					.ImplementedBy<NonDefaultInstaller>()
				);

			try
			{
				// Register installer prior to registering 
				container.Register(
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<NonDefaultInstaller>()
						.Named("another-name-that-does-not-clash")
					);
				Assert.Fail("Exception expected");
			}
			catch (NHibernateFacilityException ex)
			{
				Assert.IsTrue(ex.Message.StartsWith("Multiple IConfigurationBuilders have alias of non-default-installer:"));
			}
		}

		[Test]
		public void Throws_exception_if_null_argument_in_constructor()
		{
			try
			{
				new SessionFactoryResolver(null);
				Assert.Fail("Expected exception");
			}
			catch (ArgumentNullException) {}
		}

		#region class DefaultInstaller

		// ReSharper disable ClassNeverInstantiated.Local
		private class DefaultInstaller : IConfigurationBuilder
		{
			public bool IsDefault
			{
				get { return true; }
			}

			public string Alias
			{
				get { return "default-installer"; }
			}

			public Configuration BuildConfiguration()
			{
				throw new NotImplementedException();
			}

			public void Registered(ISessionFactory factory, Configuration configuration)
			{
				
			}
		}

		#endregion

		#region class AnotherDefaultInstaller

		// ReSharper disable ClassNeverInstantiated.Local
		private class AnotherDefaultInstaller : IConfigurationBuilder
		{
			public bool IsDefault
			{
				get { return true; }
			}

			public string Alias
			{
				get { return "another-default-installer"; }
			}

			public Configuration BuildConfiguration()
			{
				throw new NotImplementedException();
			}

			public void Registered(ISessionFactory factory, Configuration configuration)
			{

			}
		}

		#endregion

		#region class NonDefaultInstaller

		// ReSharper disable ClassNeverInstantiated.Local
		private class NonDefaultInstaller : IConfigurationBuilder
		{
			public bool IsDefault
			{
				get { return false; }
			}

			public string Alias
			{
				get { return "non-default-installer"; }
			}

			public Configuration BuildConfiguration()
			{
				throw new NotImplementedException();
			}

			public void Registered(ISessionFactory factory, Configuration configuration)
			{

			}
		}

		#endregion

		#region class InstallerWithDependency

		public class InstallerWithDependency : IConfigurationBuilder
		{
			// ReSharper disable UnusedParameter.Local
			public InstallerWithDependency(DependencyClass dependencyClass)
			{
				
			}
			// ReSharper restore UnusedParameter.Local

			public bool IsDefault
			{
				get { return false; }
			}

			public string Alias
			{
				get { return "installer-with-dependency"; }
			}

			public Configuration BuildConfiguration()
			{
				throw new NotImplementedException();
			}

			public void Registered(ISessionFactory factory, Configuration configuration)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region class DependencyClass

		public class DependencyClass {}

		#endregion
	}
}
