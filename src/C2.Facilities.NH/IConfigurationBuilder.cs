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
using NHibernate.Cfg;

namespace Castle.Facilities.NH
{
	/// <summary>
	/// Installer used to create an NHibernate configuration for a database.
	/// </summary>
	/// <remarks>
	/// Register one of these for each database used by the application.
	/// </remarks>
	public interface IConfigurationBuilder
	{
		/// <summary>
		/// 	Is this the default session factory
		/// </summary>
		bool IsDefault { get; }

		/// <summary>
		/// 	A short alias string for the database. This value must be unique for the registered
		/// 	NHibernate installers.
		/// </summary>
		string Alias { get; }

		/// <summary>
		/// 	Build an NHibernate <see cref="Configuration"/> for this database.
		/// </summary>
		/// <returns>A non null <see cref="Configuration"/> instance that can
		/// 	be used to create an <see cref="ISessionFactory"/>, or to further 
		///		configure NHibernate
		/// </returns>
		Configuration BuildConfiguration();

		/// <summary>
		/// 	Call-back to the installer, when the factory is registered
		/// 	and correctly set up in Windsor..
		/// </summary>
		/// <param name = "factory">Session factory</param>
		/// <param name="configuration">Configuration used to create the sesion factory</param>
		void Registered(ISessionFactory factory, Configuration configuration);
	}
}