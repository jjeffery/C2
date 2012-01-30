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

using System.Data;
using Castle.Facilities.NH.Internal.Implementation;
using Moq;
using NHibernate;
using NHibernate.Engine;
using NUnit.Framework;

namespace Castle.Facilities.NH.Tests
{
	// Lots of the following tests declare local variables that are not
	// subsequently used.
	// ReSharper disable UnusedVariable

	// Some of the test methods contain underscores for readability
	// ReSharper disable InconsistentNaming

	[TestFixture]
	public class StatelessSessionDelegateTests
	{
		private Mock<IStatelessSession> _mock;
		private StatelessSessionDelegate _delegate;

		[SetUp]
		public void SetUp()
		{
			_mock = new Mock<IStatelessSession>();
			_delegate = new StatelessSessionDelegate(true, _mock.Object);
		}

		[Test]
		public void Connection()
		{
			var connectionMock = new Mock<IDbConnection>();
			_mock.Setup(s => s.Connection).Returns(connectionMock.Object);
			Assert.AreSame(connectionMock.Object, _delegate.Connection);
			_mock.Verify(s => s.Connection, Times.Exactly(1));
		}

		[Test]
		public void Transaction()
		{
			var transactionMock = new Mock<ITransaction>();
			_mock.Setup(s => s.Transaction).Returns(transactionMock.Object);
			Assert.AreSame(transactionMock.Object, _delegate.Transaction);
			_mock.Verify(s => s.Transaction, Times.Exactly(1));
		}

		[Test]
		public void IsOpen()
		{
			_mock.Setup(s => s.IsOpen).Returns(true);
			Assert.IsTrue(_delegate.IsOpen);
			_mock.Verify(s => s.IsOpen, Times.Exactly(1));
		}

		[Test]
		public void IsConnected()
		{
			_mock.Setup(s => s.IsConnected).Returns(true);
			Assert.IsTrue(_delegate.IsConnected);
			_mock.Verify(s => s.IsConnected, Times.Exactly(1));
		}

		[Test]
		public void GetSessionImplementation()
		{
			var mock = new Mock<ISessionImplementor>();
			_mock.Setup(s => s.GetSessionImplementation()).Returns(mock.Object);
			Assert.AreSame(mock.Object, _delegate.GetSessionImplementation());
			_mock.Verify(s => s.GetSessionImplementation(), Times.Exactly(1));
		}

		[Test]
		public void Close_with_canClose_true()
		{
			_delegate.Close();
			_mock.Verify(s => s.Close(), Times.Exactly(1));
		}

		[Test]
		public void Close_with_canClose_false()
		{
			_delegate = new StatelessSessionDelegate(false, _mock.Object);
			_delegate.Close();
			_mock.Verify(s => s.Close(), Times.Exactly(0));
		}

		[Test]
		public void Insert()
		{
			var entity = new object();
			const string entityName = "EntityName";
			var result1 = new object();
			var result2 = new object();

			_mock.Setup(s => s.Insert(entity)).Returns(result1);
			_mock.Setup(s => s.Insert(entityName, entity)).Returns(result2);

			Assert.AreSame(result1, _delegate.Insert(entity));
			Assert.AreSame(result2, _delegate.Insert(entityName, entity));

			_mock.Verify(s => s.Insert(entity), Times.Exactly(1));
			_mock.Verify(s => s.Insert(entityName, entity), Times.Exactly(1));
		}

		[Test]
		public void Update()
		{
			var entity = new object();
			const string entityName = "EntityName";

			_delegate.Update(entity);
			_delegate.Update(entityName, entity);

			_mock.Verify(s => s.Update(entity), Times.Exactly(1));
			_mock.Verify(s => s.Update(entityName, entity), Times.Exactly(1));
		}

		[Test]
		public void Delete()
		{
			var entity = new object();
			const string entityName = "EntityName";

			_delegate.Delete(entity);
			_delegate.Delete(entityName, entity);

			_mock.Verify(s => s.Delete(entity), Times.Exactly(1));
			_mock.Verify(s => s.Delete(entityName, entity), Times.Exactly(1));
		}



	}
}
