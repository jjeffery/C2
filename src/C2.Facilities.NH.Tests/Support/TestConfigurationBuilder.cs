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
using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;
using Environment = NHibernate.Cfg.Environment;

namespace Castle.Facilities.NH.Tests.Support
{
	public class TestConfigurationBuilder : IConfigurationBuilder, IDisposable
	{
		public TestConfigurationBuilder()
		{
			Alias = "testdb";
			IsDefault = true;
			ConnectionString = "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1;";
			CreateDatabaseSchema = true;
		}

		public bool IsDefault { get; set; }
		public string Alias { get; set; }
		public string ConnectionString { get; set; }
		public bool CreateDatabaseSchema { get; set; }
		public bool DatabaseSchemaCreated { get; private set; }
		public Configuration Configuration { get; private set; }
		public ISessionFactory SessionFactory { get; private set; }

		public Configuration BuildConfiguration()
		{
			var cfg = new Configuration();
			cfg.SetProperty(Environment.ConnectionProvider, typeof (DriverConnectionProvider).AssemblyQualifiedName);
			cfg.SetProperty(Environment.Dialect, typeof (SQLiteDialect).AssemblyQualifiedName);
			cfg.SetProperty(Environment.ConnectionDriver, typeof(SQLite20Driver).AssemblyQualifiedName);
			cfg.SetProperty(Environment.ConnectionString, ConnectionString);

			var modelMapper = new ModelMapper();
			modelMapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());
			var mapping = modelMapper.CompileMappingForAllExplicitlyAddedEntities();
			cfg.AddMapping(mapping);

			return cfg;
		}

		public void Registered(ISessionFactory factory, Configuration cfg)
		{
			Configuration = cfg;
			SessionFactory = factory;
			if (CreateDatabaseSchema)
			{
				var sessionFactoryImplementor = (ISessionFactoryImplementor) factory;

				string[] lines = cfg.GenerateSchemaCreationScript(sessionFactoryImplementor.Dialect);

				using (var session = factory.OpenSession())
				{
					foreach (var line in lines)
					{
						var cmd = session.Connection.CreateCommand();
						cmd.CommandText = line;
						cmd.ExecuteNonQuery();
					}
				}

				DatabaseSchemaCreated = true;
			}
		}

		public void Dispose()
		{
			if (DatabaseSchemaCreated)
			{
				var sessionFactoryImplementor = (ISessionFactoryImplementor)SessionFactory;

				string[] lines = Configuration.GenerateDropSchemaScript(sessionFactoryImplementor.Dialect);

				using (var session = SessionFactory.OpenSession())
				{
					foreach (var line in lines)
					{
						var cmd = session.Connection.CreateCommand();
						cmd.CommandText = line;
						cmd.ExecuteNonQuery();
					}
				}
			}
		}
	}
}
