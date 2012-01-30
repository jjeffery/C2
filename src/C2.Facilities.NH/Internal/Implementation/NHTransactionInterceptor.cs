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
using Castle.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Castle.Facilities.NH.Internal.Interfaces;
using Castle.MicroKernel;
using NHibernate;
using IInterceptor = Castle.DynamicProxy.IInterceptor;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class NHTransactionInterceptor : IInterceptor, IOnBehalfAware
	{
		private readonly IKernel _kernel;
		private readonly ITransactionalInfoStore _transactionalInfoStore;
		private TransactionMetaInfo _transactionalInfo;

		public NHTransactionInterceptor(IKernel kernel, ITransactionalInfoStore transactionalInfoStore)
		{
			_kernel = Verify.ArgumentNotNull(kernel, "kernel");
			_transactionalInfoStore = Verify.ArgumentNotNull(transactionalInfoStore, "transactionalInfoStore");
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation == null || invocation.Method == null || invocation.Method.DeclaringType == null)
			{
				return;
			}

			// Get the method to call on the implementation type.
			var method = invocation.Method.DeclaringType.IsInterface
			             	? invocation.MethodInvocationTarget
			             	: invocation.Method;

			var attribute = _transactionalInfo.GetAttribute(method);
			if (attribute == null)
			{
				invocation.Proceed();
				return;
			}

			var sessionManager = _kernel.Resolve<ISessionManager>();
			ISession session = null;
			try
			{
				session = sessionManager.OpenSession(attribute.Alias);
				PerformTransaction(invocation, session, attribute);
			}
			finally
			{
				if (session != null)
				{
					session.Dispose();
				}
				_kernel.ReleaseComponent(sessionManager);
			}
		}

		private void PerformTransaction(IInvocation invocation, ISession session, NHTransactionAttribute attribute)
		{
			ITransaction tx = null;

			if (!session.Transaction.IsActive)
			{
				if (attribute.IsolationLevel == IsolationLevel.Unspecified)
				{
					tx = session.BeginTransaction();
				}
				else
				{
					tx = session.BeginTransaction(attribute.IsolationLevel);
				}
			}

			try
			{
				invocation.Proceed();
				
				if (tx != null)
				{
					// if we created the transaction, we commit it.
					tx.Commit();
				}
			}
			catch (Exception)
			{
				// Regardless of whether we created the transaction, if an unhandled exception
				// occurs, the transaction gets rolled back.
				session.Transaction.Rollback();
				throw;
			}
		}

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			_transactionalInfo = _transactionalInfoStore.GetTransactionalInfo(target.Implementation);
		}
	}
}
