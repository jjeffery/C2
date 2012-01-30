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
using Moq;
using NHibernate;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Castle.Facilities.NH.Tests
{
	[TestFixture]
	public class SessionStoreTests
	{
		[Test]
		public void Throws_exception_with_null_constructor_args()
		{
			try
			{
				new SessionStore<ISession>(null);
			}
			catch (ArgumentNullException) {}
		}

		[Test]
		public void Throws_exception_for_duplicate_alias()
		{
			var mockSession1 = new Mock<ISession>();
			var mockSession2 = new Mock<ISession>();
			var contextStore = new CallContextSessionStoreContext();
			var sessionStore = new SessionStore<ISession>(contextStore);
			const string alias = "alias";

			sessionStore.Add(mockSession1.Object, alias);

			try
			{
				sessionStore.Add(mockSession2.Object, alias);
				Assert.Fail("Expected exception");
			}
			catch (InvalidOperationException ex)
			{
				const string expectedMsg = "Session (NHibernate.ISession) already exists for alias alias";
				Assert.AreEqual(expectedMsg, ex.Message);
			}
		}

		[Test]
		public void Throws_exception_when_removing_wrong_session_for_alias()
		{
			var mockSession1 = new Mock<ISession>();
			var mockSession2 = new Mock<ISession>();
			var contextStore = new CallContextSessionStoreContext();
			var sessionStore = new SessionStore<ISession>(contextStore);
			const string alias = "alias";

			sessionStore.Add(mockSession1.Object, alias);

			try
			{
				sessionStore.Remove(mockSession2.Object, alias);
				Assert.Fail("Expected exception");
			}
			catch (InvalidOperationException ex)
			{
				const string expectedMessage = "Attempt to remove session (NHibernate.ISession) for alias alias"
				                               + " failed because session does not match";
				Assert.AreEqual(expectedMessage, ex.Message);
			}
		}
	}
}
