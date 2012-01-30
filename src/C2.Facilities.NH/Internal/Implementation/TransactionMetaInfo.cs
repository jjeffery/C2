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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class TransactionMetaInfo
	{
		private static readonly MethodInfo[] EmptyMethods = new MethodInfo[0];
		private static readonly NHTransactionAttribute[] EmptyAttributes = new NHTransactionAttribute[0];

		private static readonly Dictionary<MethodInfo, NHTransactionAttribute> EmptyAttributeMap =
			new Dictionary<MethodInfo, NHTransactionAttribute>();

		private readonly Dictionary<MethodInfo, NHTransactionAttribute> _attributeMap;

		public Type Type { get; private set; }
		public MethodInfo[] TransactionalMethods { get; private set; }
		public NHTransactionAttribute[] TransactionAttributes { get; private set; }

		public TransactionMetaInfo(Type type)
		{
			Type = type;

			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			var allMethods = type.GetMethods(bindingFlags);

			var transactionalMethods = (from m in allMethods
			                            let attributes =
			                            	(NHTransactionAttribute[]) m.GetCustomAttributes(typeof (NHTransactionAttribute), true)
			                            where attributes.Length > 0
			                            select new {Method = m, Attribute = attributes.First()}).ToArray();

			if (transactionalMethods.Length == 0)
			{
				_attributeMap = EmptyAttributeMap;
				TransactionalMethods = EmptyMethods;
				TransactionAttributes = EmptyAttributes;
				return;
			}

			_attributeMap = new Dictionary<MethodInfo, NHTransactionAttribute>();
			foreach (var transactionalMethod in transactionalMethods)
			{
				_attributeMap.Add(transactionalMethod.Method, transactionalMethod.Attribute);
			}
			TransactionalMethods = transactionalMethods.Select(m => m.Method).ToArray();
			TransactionAttributes = transactionalMethods.Select(m => m.Attribute).ToArray();
		}

		public NHTransactionAttribute GetAttribute(MethodInfo method)
		{
			for (int index = 0; index < TransactionalMethods.Length; ++index)
			{
				var m = TransactionalMethods[index];
				if (AreMethodsEqual(m, method))
				{
					return TransactionAttributes[index];
				}
			}
			return null;
		}

		// http://ayende.com/blog/2658/method-equality
		private static bool AreMethodsEqual(MethodInfo left, MethodInfo right)
		{
			if (left.Equals(right))
				return true;
			if (left.Name != right.Name)
				return false;
			// GetHashCode calls to RuntimeMethodHandle.StripMethodInstantiation()
			// which is needed to fix issues with method equality from generic types.
			if (left.GetHashCode() != right.GetHashCode())
				return false;
			if (left.DeclaringType != right.DeclaringType)
				return false;
			ParameterInfo[] leftParams = left.GetParameters();
			ParameterInfo[] rightParams = right.GetParameters();
			if (leftParams.Length != rightParams.Length)
				return false;
			for (int i = 0; i < leftParams.Length; i++)
			{
				if (leftParams[i].ParameterType != rightParams[i].ParameterType)
					return false;
			}
			if (left.ReturnType != right.ReturnType)
				return false;
			return true;
		}
	}
}
