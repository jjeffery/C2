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

using System.Linq;
using System.Text;
using Castle.Core;
using Castle.Facilities.NH.Internal.Interfaces;
using Castle.MicroKernel.ModelBuilder.Inspectors;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class NHTransactionalComponentInspector : MethodMetaInspector
	{
		private ITransactionalInfoStore _transactionalInfoStore;

		public override void ProcessModel(MicroKernel.IKernel kernel, ComponentModel model)
		{
			if (_transactionalInfoStore == null)
			{
				_transactionalInfoStore = kernel.Resolve<ITransactionalInfoStore>();
			}

			var transactionalInfo = _transactionalInfoStore.GetTransactionalInfo(model.Implementation);
			if (transactionalInfo.TransactionalMethods.Length == 0)
			{
				return;
			}

			if (model.Implementation.IsSealed)
			{
				var sb = new StringBuilder();
				sb.AppendFormat("The class {0} uses [NHTransaction] attributes, but it is sealed", model.Implementation);
				throw new NHibernateFacilityException(sb.ToString());
			}

			var problemMethods = (from m in transactionalInfo.TransactionalMethods
			                      where !m.IsVirtual
			                      select m).ToArray();

			if (problemMethods.Length > 0)
			{
				var sb = new StringBuilder();
				sb.AppendFormat(
					"The class {0} uses [NHTransaction] attributes, but the following methods need to be declared virtual:",
					model.Implementation);
				sb.AppendLine();
				foreach (var m in problemMethods)
				{
					sb.Append("  ");
					sb.Append(m.Name);
				}
				throw new NHibernateFacilityException(sb.ToString());
			}

			model.Dependencies.Add(new DependencyModel(null, typeof (NHTransactionInterceptor), false));
			model.Interceptors.Add(new InterceptorReference(typeof (NHTransactionInterceptor)));
		}

		protected override string ObtainNodeName()
		{
			return "nh-transaction-inspector";
		}
	}
}
