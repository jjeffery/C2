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
using System.Data;
using System.Data.SQLite;
using System.Linq.Expressions;
using Castle.Facilities.NH.Internal.Implementation;
using Castle.Facilities.NH.Tests.Model;
using Moq;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NUnit.Framework;

namespace Castle.Facilities.NH.Tests
{

	// Lots of the following tests declare local variables that are not
	// subsequently used.
	// ReSharper disable UnusedVariable

	// Some of the test methods contain underscores for readability
	// ReSharper disable InconsistentNaming


	[TestFixture]
	public class SessionDelegateTests
	{
		private Mock<ISession> _mock;
		private SessionDelegate _delegate;

		[SetUp]
		public void SetUp()
		{
			_mock = new Mock<ISession>();
			_delegate = new SessionDelegate(true, _mock.Object);
		}

		[Test]
		public void ActiveEntityMode()
		{
			_mock.SetupGet(s => s.ActiveEntityMode).Returns(EntityMode.Xml);
			Assert.AreEqual(EntityMode.Xml, _delegate.ActiveEntityMode);
			_mock.VerifyGet(s => s.ActiveEntityMode);
		}

		[Test]
		public void FlushMode()
		{
			_mock.SetupGet(s => s.FlushMode).Returns(NHibernate.FlushMode.Always);
			Assert.AreEqual(NHibernate.FlushMode.Always, _delegate.FlushMode);
			_mock.VerifyGet(s => s.FlushMode);
			_delegate.FlushMode= NHibernate.FlushMode.Never;
			_mock.VerifySet(s => s.FlushMode = NHibernate.FlushMode.Never);
		}

		[Test]
		public void CacheMode()
		{
			_mock.SetupGet(s => s.CacheMode).Returns(NHibernate.CacheMode.Refresh);
			Assert.AreEqual(NHibernate.CacheMode.Refresh, _delegate.CacheMode);
			_mock.VerifyGet(s => s.CacheMode);
			_delegate.CacheMode = NHibernate.CacheMode.Normal;
			_mock.VerifySet(s => s.CacheMode = NHibernate.CacheMode.Normal);
		}

		[Test]
		public void SessionFactory()
		{
			var sessionFactory = _delegate.SessionFactory;
			_mock.VerifyGet(s => s.SessionFactory);
		}

		[Test]
		public void Connection()
		{
			var connection = _delegate.Connection;
			_mock.VerifyGet(s => s.Connection);
		}

		[Test]
		public void IsOpen()
		{
			var isOpen = _delegate.IsOpen;
			_mock.VerifyGet(s => s.IsOpen);
		}

		[Test]
		public void IsConnected()
		{
			var isConnected = _delegate.IsConnected;
			_mock.VerifyGet(s => s.IsConnected);
		}

		[Test]
		public void DefaultReadOnly()
		{
			var defaultReadOnly = _delegate.DefaultReadOnly;
			_mock.VerifyGet(s => s.DefaultReadOnly);

			_delegate.DefaultReadOnly = true;
			_mock.VerifySet((s => s.DefaultReadOnly = true));
		}

		[Test]
		public void Transaction()
		{
			var transaction = _delegate.Transaction;
			_mock.VerifyGet(s => s.Transaction);
		}

		[Test]
		public void Statistics()
		{
			var statistics = _delegate.Statistics;
			_mock.VerifyGet(s => s.Statistics);
		}

		[Test]
		public void Flush()
		{
			_delegate.Flush();
			_mock.Verify(s => s.Flush(), Times.Exactly(1));
		}

		[Test]
		public void Disconnect()
		{
			var connection = new SQLiteConnection();

			_mock.Setup(s => s.Disconnect()).Returns(connection);
			var c = _delegate.Disconnect();
			Assert.AreSame(connection, c);
			_mock.Verify(s => s.Disconnect(), Times.Exactly(1));
		}

		[Test]
		public void Reconnect()
		{
			_delegate.Reconnect();
			_mock.Verify(s => s.Reconnect(), Times.Exactly(1));
		}

		[Test]
		public void ReconnectWithDbConnection()
		{
			var connection = new SQLiteConnection();
			_delegate.Reconnect(connection);
			_mock.Verify(s => s.Reconnect(connection), Times.Exactly(1));
		}

		[Test]
		public void Close_with_CanClose_true()
		{
			var connection = new SQLiteConnection();
			_mock.Setup(s => s.Close()).Returns(connection);
			var c = _delegate.Close();
			Assert.AreSame(connection, c);
			_mock.Verify(s => s.Close(), Times.Exactly(1));
		}

		[Test]
		public void Close_with_CanClose_false()
		{
			// override the setup method
			_delegate = new SessionDelegate(false, _mock.Object);

			var connection = new SQLiteConnection();
			_mock.Setup(s => s.Close()).Returns(connection);
			var c = _delegate.Close();
			Assert.IsNull(c);
			_mock.Verify(s => s.Close(), Times.Never());
		}

		[Test]
		public void CancelQuery()
		{
			_delegate.CancelQuery();
			_mock.Verify(s => s.CancelQuery(), Times.Exactly(1));
		}

		[Test]
		public void IsDirty()
		{
			_mock.Setup(s => s.IsDirty()).Returns(true);
			Assert.IsTrue(_delegate.IsDirty());
			_mock.Verify(s => s.IsDirty(), Times.Exactly(1));
		}

		[Test]
		public void IsReadOnly()
		{
			var obj = new object();
			_mock.Setup(s => s.IsReadOnly(obj)).Returns(true);
			Assert.IsTrue(_delegate.IsReadOnly(obj));
			_mock.Verify(s => s.IsReadOnly(obj), Times.Exactly(1));
		}

		[Test]
		public void SetReadOnly()
		{
			var obj = new object();
			const bool readOnly = true;

			_delegate.SetReadOnly(obj, readOnly);
			_mock.Verify(s => s.SetReadOnly(obj, readOnly), Times.Exactly(1));
		}

		[Test]
		public void GetIdentifier()
		{
			var obj = new object();
			var identifier = new object();
			_mock.Setup(s => s.GetIdentifier(obj)).Returns(identifier);
			Assert.AreSame(identifier, _delegate.GetIdentifier(obj));
			_mock.Verify(s => s.GetIdentifier(obj), Times.Exactly(1));
		}

		[Test]
		public void Contains()
		{
			var obj = new object();
			_mock.Setup(s => s.Contains(obj)).Returns(true);
			Assert.IsTrue(_delegate.Contains(obj));
			_mock.Verify(s => s.Contains(obj), Times.Exactly(1));
		}

		[Test]
		public void Evict()
		{
			var obj = new object();
			_delegate.Evict(obj);
			_mock.Verify(s => s.Evict(obj), Times.Exactly(1));
		}

		[Test]
		public void Load()
		{
			var type = typeof (object);
			var id = new object();
			var lockMode = LockMode.UpgradeNoWait;
			const string entityName = "entityName";
			var obj = new object();
			var result1 = new object();
			var result2 = new object();
			var result3 = new object();
			var result4 = new object();
			var result5 = new object();
			var result6 = new object();

			_mock.Setup(s => s.Load(type, id, lockMode)).Returns(result1);
			_mock.Setup(s => s.Load(entityName, id, lockMode)).Returns(result2);
			_mock.Setup(s => s.Load(type, id)).Returns(result3);
			_mock.Setup(s => s.Load<object>(id, lockMode)).Returns(result4);
			_mock.Setup(s => s.Load<object>(id)).Returns(result5);
			_mock.Setup(s => s.Load(entityName, id)).Returns(result6);

			Assert.AreSame(result1, _delegate.Load(type, id, lockMode));
			Assert.AreSame(result2, _delegate.Load(entityName, id, lockMode));
			Assert.AreSame(result3, _delegate.Load(type, id));
			Assert.AreSame(result4, _delegate.Load<object>(id, lockMode));
			Assert.AreSame(result5, _delegate.Load<object>(id));
			Assert.AreSame(result6, _delegate.Load(entityName, id));
			_delegate.Load(obj, id);

			_mock.Verify(s => s.Load(type, id, lockMode), Times.Exactly(1));
			_mock.Verify(s => s.Load(type, id, lockMode), Times.Exactly(1));
			_mock.Verify(s => s.Load(type, id), Times.Exactly(1));
			_mock.Verify(s => s.Load<object>(id, lockMode), Times.Exactly(1));
			_mock.Verify(s => s.Load<object>(id), Times.Exactly(1));
			_mock.Verify(s => s.Load(entityName, id), Times.Exactly(1));
			_mock.Verify(s => s.Load(obj, id), Times.Exactly(1));
		}

		[Test]
		public void Replicate()
		{
			var obj = new object();
			var replicationMode = ReplicationMode.LatestVersion;
			const string entityName = "entityName";

			_delegate.Replicate(obj, replicationMode);
			_delegate.Replicate(entityName, obj, replicationMode);

			_mock.Verify(s => s.Replicate(obj, replicationMode), Times.Exactly(1));
			_mock.Verify(s => s.Replicate(entityName, obj, replicationMode), Times.Exactly(1));
		}

		[Test]
		public void Save()
		{
			var obj = new object();
			var id = new object();
			const string entityName = "EntityName";
			var result1 = new object();
			var result2 = new object();

			_mock.Setup(s => s.Save(obj)).Returns(result1);
			_mock.Setup(s => s.Save(entityName, obj)).Returns(result2);

			_delegate.Save(obj, id);
			Assert.AreSame(result1, _delegate.Save(obj));
			Assert.AreSame(result2, _delegate.Save(entityName, obj));

			_mock.Verify(s => s.Save(obj, id), Times.Exactly(1));
			_mock.Verify(s => s.Save(obj), Times.Exactly(1));
			_mock.Verify(s => s.Save(entityName, obj), Times.Exactly(1));
		}

		[Test]
		public void SaveOrUpdate()
		{
			var obj = new object();
			var id = new object();
			const string entityName = "EntityName";

			_delegate.SaveOrUpdate(obj);
			_delegate.SaveOrUpdate(entityName, obj);

			_mock.Verify(s => s.SaveOrUpdate(obj), Times.Exactly(1));
			_mock.Verify(s => s.SaveOrUpdate(entityName, obj), Times.Exactly(1));
		}

		[Test]
		public void Update()
		{
			var obj = new object();
			var id = new object();
			const string entityName = "EntityName";

			_delegate.Update(obj, id);
			_delegate.Update(obj);
			_delegate.Update(entityName, obj);

			_mock.Verify(s => s.Update(obj, id), Times.Exactly(1));
			_mock.Verify(s => s.Update(obj), Times.Exactly(1));
			_mock.Verify(s => s.Update(entityName, obj), Times.Exactly(1));
		}

		[Test]
		public void Merge()
		{
			var obj = new object();
			// Choose an abitrary class that is not object, so as not to confuse
			// the generic methods with the non-generic methods. Here we choose
			// Blog, which is a test model class.
			var blog = new Blog(); 
			var id = new object();
			const string entityName = "EntityName";
			var result1 = new Blog();
			var result2 = new Blog();
			var result3 = new Blog();
			var result4 = new Blog();

			_mock.Setup(s => s.Merge(obj)).Returns(result1);
			_mock.Setup(s => s.Merge(entityName, obj)).Returns(result2);
			_mock.Setup(s => s.Merge(blog)).Returns(result3);
			_mock.Setup(s => s.Merge(entityName, blog)).Returns(result4);


			Assert.AreSame(result1, _delegate.Merge(obj));
			Assert.AreSame(result2, _delegate.Merge(entityName, obj));
			Assert.AreSame(result3, _delegate.Merge(blog));
			Assert.AreSame(result4, _delegate.Merge(entityName, blog));

			_mock.Verify(s => s.Merge(obj), Times.Exactly(1));
			_mock.Verify(s => s.Merge(entityName, obj), Times.Exactly(1));
			_mock.Verify(s => s.Merge(blog), Times.Exactly(1));
			_mock.Verify(s => s.Merge(entityName, blog), Times.Exactly(1));
		}

		[Test]
		public void Persist()
		{
			var obj = new object();
			const string entityName = "EntityName";

			_delegate.Persist(obj);
			_delegate.Persist(entityName, obj);

			_mock.Verify(s => s.Persist(obj), Times.Exactly(1));
			_mock.Verify(s => s.Persist(entityName, obj), Times.Exactly(1));
		}

		[Test]
		public void SaveOrUpdateCopy()
		{
			var obj = new object();
			const string id = "id";

			_delegate.SaveOrUpdateCopy(obj);
			_delegate.SaveOrUpdateCopy(obj, id);

			// SaveOrUpdateCopy is obsolete, but we still want to test
			// that it passes through to the inner session correctly
#pragma warning disable 612,618
			_mock.Verify(s => s.SaveOrUpdateCopy(obj), Times.Exactly(1));
			_mock.Verify(s => s.SaveOrUpdateCopy(obj, id), Times.Exactly(1));
#pragma warning restore 612,618
		}

		[Test]
		public void Delete()
		{
			var obj = new object();
			const string query = "query";
			const string entityName = "EntityName";
			var value = new object();
			var values = new[] { new object(), new object() };
			var type = new SByteType();
			var types = new IType[] {new SByteType(), new SingleType(),};
			const int result1 = 1;
			const int result2 = 2;

			_mock.Setup(s => s.Delete(query, value, type)).Returns(result1);
			_mock.Setup(s => s.Delete(query, values, types)).Returns(result2);

			_delegate.Delete(obj);
			_delegate.Delete(query);
			_delegate.Delete(entityName, obj);
			Assert.AreEqual(result1, _delegate.Delete(query, value, type));
			Assert.AreEqual(result2, _delegate.Delete(query, values, types));

			_mock.Verify(s => s.Delete(obj), Times.Exactly(1));
			_mock.Verify(s => s.Delete(entityName, obj), Times.Exactly(1));
			_mock.Verify(s => s.Delete(query), Times.Exactly(1));
			_mock.Verify(s => s.Delete(query, value, type), Times.Exactly(1));
			_mock.Verify(s => s.Delete(query, values, types), Times.Exactly(1));

		}

		[Test]
		public void Lock()
		{
			var obj = new object();
			const string entityName = "EntityNameLock";
			var lockMode = LockMode.UpgradeNoWait;

			_delegate.Lock(obj, lockMode);
			_delegate.Lock(entityName, obj, lockMode);

			_mock.Verify(s => s.Lock(obj, lockMode), Times.Exactly(1));
			_mock.Verify(s => s.Lock(entityName, obj, lockMode), Times.Exactly(1));
		}

		[Test]
		public void Refresh()
		{
			var obj = new object();
			var lockMode = LockMode.Upgrade;

			_delegate.Refresh(obj);
			_delegate.Refresh(obj, lockMode);

			_mock.Verify(s => s.Refresh(obj), Times.Exactly(1));
			_mock.Verify(s => s.Refresh(obj, lockMode), Times.Exactly(1));
		}

		[Test]
		public void GetCurrentLockMode()
		{
			var lockMode = LockMode.Force;
			var obj = new object();
			_mock.Setup(s => s.GetCurrentLockMode(obj)).Returns(lockMode);
			Assert.AreEqual(lockMode, _delegate.GetCurrentLockMode(obj));
			_mock.Verify(s => s.GetCurrentLockMode(obj), Times.Exactly(1));
		}


		[Test]
		public void BeginTransaction()
		{
			var txMock1 = new Mock<ITransaction>();
			var txMock2 = new Mock<ITransaction>();

			_mock.Setup(s => s.BeginTransaction()).Returns(txMock1.Object);
			_mock.Setup(s => s.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(txMock2.Object);

			Assert.AreSame(txMock1.Object, _delegate.BeginTransaction());
			Assert.AreSame(txMock2.Object, _delegate.BeginTransaction(IsolationLevel.RepeatableRead));

			_mock.Verify(s => s.BeginTransaction(), Times.Exactly(1));
			_mock.Verify(s => s.BeginTransaction(IsolationLevel.RepeatableRead), Times.Exactly(1));
		}

		[Test]
		public void CreateCriteria()
		{
			var criteriaMock1 = new Mock<ICriteria>();
			var criteriaMock2 = new Mock<ICriteria>();
			var criteriaMock3 = new Mock<ICriteria>();
			var criteriaMock4 = new Mock<ICriteria>();
			var criteriaMock5 = new Mock<ICriteria>();
			var criteriaMock6 = new Mock<ICriteria>();

			const string alias = "alias";
			const string entityName = "EntityNameCriteria";
			var persistentClass = typeof (Blog);

			_mock.Setup(s => s.CreateCriteria<Blog>()).Returns(criteriaMock1.Object);
			_mock.Setup(s => s.CreateCriteria<Blog>(alias)).Returns(criteriaMock2.Object);
			_mock.Setup(s => s.CreateCriteria(persistentClass)).Returns(criteriaMock3.Object);
			_mock.Setup(s => s.CreateCriteria(persistentClass, alias)).Returns(criteriaMock4.Object);
			_mock.Setup(s => s.CreateCriteria(entityName)).Returns(criteriaMock5.Object);
			_mock.Setup(s => s.CreateCriteria(entityName, alias)).Returns(criteriaMock6.Object);

			Assert.AreSame(criteriaMock1.Object, _delegate.CreateCriteria<Blog>());
			Assert.AreSame(criteriaMock2.Object, _delegate.CreateCriteria<Blog>(alias));
			Assert.AreSame(criteriaMock3.Object, _delegate.CreateCriteria(persistentClass));
			Assert.AreSame(criteriaMock4.Object, _delegate.CreateCriteria(persistentClass, alias));
			Assert.AreSame(criteriaMock5.Object, _delegate.CreateCriteria(entityName));
			Assert.AreSame(criteriaMock6.Object, _delegate.CreateCriteria(entityName, alias));

			_mock.Verify(s => s.CreateCriteria<Blog>(), Times.Exactly(1));
			_mock.Verify(s => s.CreateCriteria<Blog>(alias), Times.Exactly(1));
			_mock.Verify(s => s.CreateCriteria(persistentClass), Times.Exactly(1));
			_mock.Verify(s => s.CreateCriteria(persistentClass, alias), Times.Exactly(1));
			_mock.Verify(s => s.CreateCriteria(entityName), Times.Exactly(1));
			_mock.Verify(s => s.CreateCriteria(entityName, alias), Times.Exactly(1));
		}

		[Test]
		public void QueryOver()
		{
			var queryOverMock1 = new Mock<IQueryOver<Blog, Blog>>();
			var queryOverMock2 = new Mock<IQueryOver<Blog, Blog>>();
			var queryOverMock3 = new Mock<IQueryOver<Blog, Blog>>();
			var queryOverMock4 = new Mock<IQueryOver<Blog, Blog>>();

			var blog = new Blog();
			Expression<Func<Blog>> alias = () => blog;
			const string entityName = "Blog";

			_mock.Setup(s => s.QueryOver<Blog>()).Returns(queryOverMock1.Object);
			_mock.Setup(s => s.QueryOver(alias)).Returns(queryOverMock2.Object);
			_mock.Setup(s => s.QueryOver<Blog>(entityName)).Returns(queryOverMock3.Object);
			_mock.Setup(s => s.QueryOver(entityName, alias)).Returns(queryOverMock4.Object);

			Assert.AreSame(queryOverMock1.Object, _delegate.QueryOver<Blog>());
			Assert.AreSame(queryOverMock2.Object, _delegate.QueryOver(alias));
			Assert.AreSame(queryOverMock3.Object, _delegate.QueryOver<Blog>(entityName));
			Assert.AreSame(queryOverMock4.Object, _delegate.QueryOver(entityName, alias));

			_mock.Verify(s => s.QueryOver<Blog>(), Times.Exactly(1));
			_mock.Verify(s => s.QueryOver(alias), Times.Exactly(1));
			_mock.Verify(s => s.QueryOver<Blog>(entityName), Times.Exactly(1));
			_mock.Verify(s => s.QueryOver(entityName, alias), Times.Exactly(1));
		}

		[Test]
		public void CreateQuery()
		{
			var mockQuery = new Mock<IQuery>();
			_mock.Setup(s => s.CreateQuery(It.IsAny<string>())).Returns(mockQuery.Object);
			Assert.AreSame(mockQuery.Object, _delegate.CreateQuery("query"));
			_mock.Verify(s => s.CreateQuery("query"), Times.Exactly(1));
		}

		[Test]
		public void CreateFilter()
		{
			var mockQuery = new Mock<IQuery>();
			var collection = new object();
			const string query = "query";

			_mock.Setup(s => s.CreateFilter(collection, query)).Returns(mockQuery.Object);
			Assert.AreSame(mockQuery.Object, _delegate.CreateFilter(collection, query));
			_mock.Verify(s => s.CreateFilter(collection, query), Times.Exactly(1));
		}

		[Test]
		public void GetNamedQuery()
		{
			var mockQuery = new Mock<IQuery>();
			_mock.Setup(s => s.GetNamedQuery(It.IsAny<string>())).Returns(mockQuery.Object);
			Assert.AreSame(mockQuery.Object, _delegate.GetNamedQuery("query"));
			_mock.Verify(s => s.GetNamedQuery("query"), Times.Exactly(1));
		}

		[Test]
		public void CreateSQLQuery()
		{
			var mockQuery = new Mock<ISQLQuery>();
			_mock.Setup(s => s.CreateSQLQuery(It.IsAny<string>())).Returns(mockQuery.Object);
			Assert.AreSame(mockQuery.Object, _delegate.CreateSQLQuery("query"));
			_mock.Verify(s => s.CreateSQLQuery("query"), Times.Exactly(1));
		}

		[Test]
		public void Clear()
		{
			_delegate.Clear();
			_mock.Verify(s => s.Clear(), Times.Exactly(1));
		}

		[Test]
		public void Get()
		{
			var result1 = new object();
			var result2 = new object();
			var result3 = new object();
			var result4 = new Blog();
			var result5 = new Blog();
			var clazz = typeof (Blog);
			var id = new object();
			var lockMode = LockMode.UpgradeNoWait;
			const string entityName = "Blog";

			_mock.Setup(s => s.Get(clazz, id)).Returns(result1);
			_mock.Setup(s => s.Get(clazz, id, lockMode)).Returns(result2);
			_mock.Setup(s => s.Get(entityName, id)).Returns(result3);
			_mock.Setup(s => s.Get<Blog>(id)).Returns(result4);
			_mock.Setup(s => s.Get<Blog>(id, lockMode)).Returns(result5);

			Assert.AreSame(result1, _delegate.Get(clazz, id));
			Assert.AreSame(result2, _delegate.Get(clazz, id, lockMode));
			Assert.AreSame(result3, _delegate.Get(entityName, id));
			Assert.AreSame(result4, _delegate.Get<Blog>(id));
			Assert.AreSame(result5, _delegate.Get<Blog>(id, lockMode));

			_mock.Verify(s => s.Get(clazz, id), Times.Exactly(1));
			_mock.Verify(s => s.Get(clazz, id, lockMode), Times.Exactly(1));
			_mock.Verify(s => s.Get(entityName, id), Times.Exactly(1));
			_mock.Verify(s => s.Get<Blog>(id), Times.Exactly(1));
			_mock.Verify(s => s.Get<Blog>(id, lockMode), Times.Exactly(1));
		}

		[Test]
		public void GetEntityName()
		{
			const string result = "EntityName";
			var obj = new object();

			_mock.Setup(s => s.GetEntityName(obj)).Returns(result);
			Assert.AreEqual(result, _delegate.GetEntityName(obj));
			_mock.Verify(s => s.GetEntityName(obj), Times.Exactly(1));
		}

		[Test]
		public void EnableFilter()
		{
			const string filterName = "FilterName";
			var mockFilter = new Mock<IFilter>();

			_mock.Setup(s => s.EnableFilter(filterName)).Returns(mockFilter.Object);
			Assert.AreSame(mockFilter.Object, _delegate.EnableFilter(filterName));
			_mock.Verify(s => s.EnableFilter(filterName), Times.Exactly(1));
		}

		[Test]
		public void GetEnabledFilter()
		{
			const string filterName = "FilterName";
			var mockFilter = new Mock<IFilter>();

			_mock.Setup(s => s.GetEnabledFilter(filterName)).Returns(mockFilter.Object);
			Assert.AreSame(mockFilter.Object, _delegate.GetEnabledFilter(filterName));
			_mock.Verify(s => s.GetEnabledFilter(filterName), Times.Exactly(1));
		}

		[Test]
		public void DisableFilter()
		{
			const string filterName = "FilterName";
			_delegate.DisableFilter(filterName);
			_mock.Verify(s => s.DisableFilter(filterName), Times.Exactly(1));
		}

		[Test]
		public void CreateMultiQuery()
		{
			var mockMultiQuery = new Mock<IMultiQuery>();
			_mock.Setup(s => s.CreateMultiQuery()).Returns(mockMultiQuery.Object);
			Assert.AreSame(mockMultiQuery.Object, _delegate.CreateMultiQuery());
			_mock.Verify(s => s.CreateMultiQuery(), Times.Exactly(1));
		}

		[Test]
		public void SetBatchSize()
		{
			const int batchSize = 11;
			var mockSession = new Mock<ISession>();
			_mock.Setup(s => s.SetBatchSize(It.IsAny<int>())).Returns(mockSession.Object);
			Assert.AreSame(mockSession.Object, _delegate.SetBatchSize(batchSize));
			_mock.Verify(s => s.SetBatchSize(batchSize), Times.Exactly(1));
		}

		[Test]
		public void GetSessionImplementation()
		{
			var mockSessionImplementor = new Mock<ISessionImplementor>();
			_mock.Setup(s => s.GetSessionImplementation()).Returns(mockSessionImplementor.Object);
			Assert.AreSame(mockSessionImplementor.Object, _delegate.GetSessionImplementation());
			_mock.Verify(s => s.GetSessionImplementation(), Times.Exactly(1));
		}

		[Test]
		public void CreateMultiCriteria()
		{
			var mockMultiCriteria = new Mock<IMultiCriteria>();
			_mock.Setup(s => s.CreateMultiCriteria()).Returns(mockMultiCriteria.Object);
			Assert.AreSame(mockMultiCriteria.Object, _delegate.CreateMultiCriteria());
			_mock.Verify(s => s.CreateMultiCriteria(), Times.Exactly(1));
		}

		[Test]
		public void GetSession()
		{
			const EntityMode entityMode = EntityMode.Xml;
			var mockSession = new Mock<ISession>();
			_mock.Setup(s => s.GetSession(It.IsAny<EntityMode>())).Returns(mockSession.Object);
			Assert.AreSame(mockSession.Object, _delegate.GetSession(entityMode));
			_mock.Verify(s => s.GetSession(entityMode), Times.Exactly(1));
		}

		[Test]
		public void InnerSession()
		{
			Assert.AreSame(_mock.Object, _delegate.InnerSession); 
		}

		[Test]
		public void NestedSessionDelegates()
		{
			var delegate2 = new SessionDelegate(false, _delegate);

			// tests that the inner session for the new delegate is not
			// the first delegate, but rather the inner session for the first
			// delegate.
			Assert.AreSame(_delegate.InnerSession, delegate2.InnerSession);
		}


		[Test]
		public void Dispose_with_CanClose_true()
		{
			int closedEventCallCount = 0;
			var d = _delegate;
			_delegate.Closed += delegate(object obj, EventArgs e)
			                    	{
			                    		Assert.AreSame(d, obj);
			                    		closedEventCallCount += 1;
			                    	};
			 _delegate.Dispose();
			_mock.Verify(s => s.Dispose(), Times.Exactly(1));
			Assert.AreEqual(1, closedEventCallCount);
		}

		[Test]
		public void Dispose_with_CanClose_false()
		{
			_delegate.Closed += (obj, e) => Assert.Fail("Closed event should not be called");

			// override the setup method
			_delegate = new SessionDelegate(false, _mock.Object);

			_delegate.Dispose();
			_mock.Verify(s => s.Dispose(), Times.Never());
		}

		[Test]
		public void Close_and_then_Dispose_with_CanClose_true()
		{
			int closedEventCallCount = 0;
			var d = _delegate;
			_delegate.Closed += delegate(object obj, EventArgs e)
			{
				Assert.AreSame(d, obj);
				closedEventCallCount += 1;
			};

			_delegate.Close();
			Assert.AreEqual(1, closedEventCallCount);
			_delegate.Dispose();
			_mock.Verify(s => s.Close(), Times.Exactly(1));
			_mock.Verify(s => s.Dispose(), Times.Exactly(1));
			Assert.AreEqual(1, closedEventCallCount);
		}
	}
}
