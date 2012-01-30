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

using NHibernate;

namespace Castle.Facilities.NH.Internal.Interfaces
{
	public interface ISessionFactoryResolver
	{
		/// <summary>
		/// Returns the <see cref="ISessionFactory"/> associated with the specified alias.
		/// </summary>
		/// <param name="alias">Session factory alias</param>
		/// <returns>
		/// An instance of <see cref="ISessionFactory"/> associated with the alias. The
		/// session factory will be created if required.
		/// </returns>
		/// <exception cref="NHibernateFacilityException">
		/// Unable to create a session factory with the specified alias.
		/// </exception>
		ISessionFactory GetSessionFactory(string alias);

		/// <summary>
		/// Is the specified alias defined.
		/// </summary>
		/// <param name="alias">Alias</param>
		bool IsAliasDefined(string alias);

		/// <summary>
		/// Returns the default alias.
		/// </summary>
		string DefaultAlias { get; }
	}
}
