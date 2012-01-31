## Creating the Container ##

To include the NHibernate facility in your program, you must add it programatically. There is no XML configuration option at present. All of the classes and interfaces you need for normal operation are in the `Castle.Facilities.NH` namespace:

	var container = new WindsorContainer();
	container.AddFacility<NHibernateFacility>();

There is no need to add any other facilities for the `NHibernateFacility` to work. You can, of course, add whatever other Castle facilities you need for your program.

## NHibernate configuration builders ##

The next thing you need to do is to register one or more classes that implement the `IConfigurationBuilder` interface. The `IConfigurationBuilder` interface looks like this:

	public interface IConfigurationBuilder 
	{
		bool IsDefault { get; }
		string Alias { get; }
		Configuration BuildConfiguration();
		void Registered(ISessionFactory factory, Configuration configuration);
	}

The purpose of classes that implement this interface is that they know how to build the NHibernate `Configuration` objects that are in turn used to create `ISessionFactory` objects, which are used to create `ISession` objects. (If you are new to NHibernate, now is probably a good time to read the NHibernate documentation, especially the section on configuration).

A quick run through of the `IConfigurationBuilder` members:

* `IsDefault` is used to indicate whether this is the default NHibernate configuration for access. The majority of applications only use one database, and would only have one `IConfigurationBuilder`, and this property would always return true. It is an error to have more than one `IConfigurationBuilder` instance have `IsDefault` return `true`.
* `Alias` is a string property that uniquely identifies the NHibernate configuration. It is an error for two or more `IConfigurationBuilder` instances to have the same alias.
* The `BuildConfiguration` method will create an NHibernate `Configuration` object. The implementation is free to use whatever API it chooses (e.g. `FluentNHibernate`, the new loquacious API in NHibernate 3.2, or the classic API with mappings in `*.hbm.xml` files). This method will be called at most once for each `IConfigurationBuilder`, and will be called only when needed. (i.e., the `BuildConfiguration` method will be called the first time the program actually attempts to connect to the database).
* The `Registered` method is called by the NHibernate facility shortly after the `BuildConfiguration` method has been called. It offers the configuration builder the opportunity to perform some action as soon as the NHibernate database configuration has been initialised. One use of this method is in unit tests, where the test database schema can be initialised as soon as the test database has been created.

An example of an NHibernate configuration builder can be found in the unit test assembly, class [TestConfigurationBuilder](https://github.com/jjeffery/C2/blob/master/src/C2.Facilities.NH.Tests/Support/TestConfigurationBuilder.cs). (TODO: this example is not the simplest example possible, it would be better to reference a more 'standard' configuration builder as an example).

Your NHibernate configuration builders need to be registered with the container, e.g.:

	container.Register(
		Component.For<IConfigurationBuilder>()
		ImplementedBy<MyDatabaseConfigurationBuilder>()
		);

## Using `ISessionManager` ##

Once your database configuration is setup using your NHibernate configuration builders, you are ready to access your database(s) using NHibernate. The NHibernate facility offers the `ISessionManager` interface to simplify the task of obtaining an `ISession` object.

To use `ISessionManager`, simply use constructor injection or property injection to obtain an instance:

	class MyDatabaseAccessClass 
	{
		public ISessionManager SessionManager { get; set; }

		public PerformSomeDatabaseAccess() 
		{
			using (ISession session = SessionManager.OpenSession())
			{
				// Do something with the session, for example perform a query
				IList<Invoice> list = session.QueryOver<Invoice>()
										.Where(invoice => invoice.IsCurrent)
										.List();
			}
		}

In the example above, `SessionManager.OpenSession()` returns an `ISession` for the default database configuration. If your application access more than one database, you would make use of aliases, e.g.:

	using (ISession session = SessionManager.OpenSession("databaseOne")
	{
		// do something with session for database one
	}

	using (ISession session = SessionManager.OpenSession("databaseTwo"))
	{
		// do something with session for database two
	}

	using (ISession session = SessionManager.OpenSession())
	{
		// do something with session for the default database
	}
	   




