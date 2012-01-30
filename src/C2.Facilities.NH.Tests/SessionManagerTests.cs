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
using Castle.Facilities.NH.Internal.Implementation;
using Castle.Facilities.NH.Tests.Model;
using Castle.Facilities.NH.Tests.Support;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Castle.Facilities.NH.Tests
{
	[TestFixture]
	public class SessionManagerTests
	{
		private IWindsorContainer _container;
		private ISessionManager _sessionManager;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_container = new WindsorContainer();
			_container.AddFacility<NHibernateFacility>();
			_container.Register(
				Component.For<INHibernateInstaller>()
					.ImplementedBy<TestNHibernateInstaller>());

			_sessionManager = _container.Resolve<ISessionManager>();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			_container.Dispose();
			_container = null;
			_sessionManager = null;
		}

		[Test]
		public void Simple_use_with_default_alias()
		{
			using (var session = _sessionManager.OpenSession())
			{
				var blogs = session.QueryOver<Blog>().List();
				Assert.AreEqual(0, blogs.Count);
			}
		}

		[Test]
		public void Nested_sessions()
		{
			using (var session1 = _sessionManager.OpenSession())
			{
				using (var session2 = _sessionManager.OpenSession())
				{
					using (var session3 = _sessionManager.OpenSession("testdb"))
					{
						var sessionDelegate1 = (SessionDelegate) session1;
						var sessionDelegate2 = (SessionDelegate)session2;
						var sessionDelegate3 = (SessionDelegate)session3;

						Assert.AreSame(sessionDelegate1.InnerSession, sessionDelegate2.InnerSession);
						Assert.AreSame(sessionDelegate1.InnerSession, sessionDelegate3.InnerSession);
						Assert.AreSame(sessionDelegate2.InnerSession, sessionDelegate3.InnerSession);
					}
				}
			}
		}

		[Test]
		public void Nested_stateless_sessions()
		{
			using (var session1 = _sessionManager.OpenStatelessSession())
			{
				using (var session2 = _sessionManager.OpenStatelessSession())
				{
					using (var session3 = _sessionManager.OpenStatelessSession("testdb"))
					{
						var sessionDelegate1 = (StatelessSessionDelegate)session1;
						var sessionDelegate2 = (StatelessSessionDelegate)session2;
						var sessionDelegate3 = (StatelessSessionDelegate)session3;

						Assert.AreSame(sessionDelegate1.InnerSession, sessionDelegate2.InnerSession);
						Assert.AreSame(sessionDelegate1.InnerSession, sessionDelegate3.InnerSession);
						Assert.AreSame(sessionDelegate2.InnerSession, sessionDelegate3.InnerSession);
					}
				}
			}
		}
	}
}
