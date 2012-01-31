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

using Castle.Facilities.NH.Facility;
using Castle.Facilities.NH.Tests.Support;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Castle.Facilities.NH.Tests
{
	[TestFixture]
	public class NHTransactionTests
	{
		[Test]
		public void Calls_public_transactional_method_in_transaction()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<NHibernateFacility>();
				container.Register(
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<TestConfigurationBuilder>(),

					Component.For<ITestComponent>()
						.ImplementedBy<TestComponent>()
					);

				var testComponent = container.Resolve<ITestComponent>();

				testComponent.PublicTransactionalMethod();
			}
		}

		[Test]
		public void Calls_public_generic_transactional_method_in_transaction()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<NHibernateFacility>();
				container.Register(
					Component.For<IConfigurationBuilder>()
						.ImplementedBy<TestConfigurationBuilder>(),

					Component.For(typeof(IGeneric<>))
						.ImplementedBy(typeof(GenericClass<>))
					);

				var testComponent = container.Resolve<IGeneric<int>>();

				testComponent.DoSomethingWith(0);
			}
		}

		public interface IGeneric<T>
		{
			void DoSomethingWith(T t);
		}

		public class GenericClass<T> : IGeneric<T>
		{
			public ISessionManager SessionManager { get; set; }

			[NHTransaction]
			public void DoSomethingWith(T t)
			{
				using (var session = SessionManager.OpenSession())
				{
					Assert.IsTrue(session.Transaction.IsActive, "session should be active");
				}
			}
		}

		[Test]
		public void Throws_exception_when_non_virtual_methods_have_NHTransactionAttribute()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<NHibernateFacility>();

				try
				{
					container.Register(Component.For<TransactionalClassWithNonVirtualTransactionMethod>());
					Assert.Fail("Expected exception");
				}
				catch (NHibernateFacilityException ex)
				{
					Assert.IsTrue(ex.Message.EndsWith("  NonVirtualTransactionalMethod"));
					Assert.IsTrue(ex.Message.StartsWith("The class "));
					Assert.IsTrue(
						ex.Message.Contains("uses [NHTransaction] attributes, but the following methods need to be declared virtual:"));
				}
			}
		}

		[Test]
		public void Throws_exception_when_sealed_class_methods_have_NHTransactionAttribute()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<NHibernateFacility>();

				try
				{
					container.Register(Component.For<SealedClassWithTransactionalMethod>());
					Assert.Fail("Expected exception");
				}
				catch (NHibernateFacilityException ex)
				{
					Assert.IsTrue(ex.Message.EndsWith(" uses [NHTransaction] attributes, but it is sealed"), ex.Message);
					Assert.IsTrue(ex.Message.StartsWith("The class "));
				}
			}
		}

		public class TransactionalClassWithNonVirtualTransactionMethod
		{
			[NHTransaction]
			// ReSharper disable UnusedMember.Global
			public void NonVirtualTransactionalMethod()
			{
				
			}
			// ReSharper restore UnusedMember.Global
		}

		public class BaseClass
		{
			public virtual void DoSomething() {}
		}

		public sealed class SealedClassWithTransactionalMethod : BaseClass
		{
			[NHTransaction]
			public override void DoSomething()
			{
				
			}
		}

		public interface ITestComponent
		{
			void PublicTransactionalMethod();
			void CallProtectedTransactionalMethod();
		}

		public class TestComponent : ITestComponent
		{
			public ISessionManager SessionManager { get; set; }

			public void CallProtectedTransactionalMethod()
			{
				ProtectedTransactionalMethod();
			}

			[NHTransaction]
			public void PublicTransactionalMethod()
			{
				using (var session = SessionManager.OpenSession())
				{
					Assert.IsTrue(session.Transaction.IsActive, "Session transaction should be active");
				}
			}

			[NHTransaction]
			protected virtual void ProtectedTransactionalMethod()
			{
				using (var session = SessionManager.OpenSession())
				{
					Assert.IsTrue(session.Transaction.IsActive, "Session transaction should be active");
				}
			}
		}
	}
}
