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
using Castle.Facilities.NH.Internal.Interfaces;

namespace Castle.Facilities.NH.Internal.Implementation
{
	public class TransactionMetaInfoStore : ITransactionalInfoStore
	{
		private readonly Dictionary<Type, TransactionMetaInfo> _map = new Dictionary<Type, TransactionMetaInfo>();

		public TransactionMetaInfo GetTransactionalInfo(Type type)
		{
			TransactionMetaInfo transactionalInfo;
			if (!_map.TryGetValue(type, out transactionalInfo))
			{
				transactionalInfo = new TransactionMetaInfo(type);
				_map.Add(type, transactionalInfo);
			}

			return transactionalInfo;
		}
	}
}
