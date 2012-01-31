using NHibernate.Cfg;

namespace Castle.Facilities.NH
{
	/// <summary>
	/// Registers an object which optionally contributes to the NHibernate <see cref="Configuration"/>,
	/// which has just been created by an <see cref="IConfigurationBuilder"/>.
	/// </summary>
	/// <remarks>
	/// The NHibernate facility creates NHibernate configurations by resolving all components that
	/// implement the <see cref="IConfigurationBuilder"/> interface. After each <see cref="Configuration"/>
	/// object is created, the NHibernate facility calls each component registered to implement this
	/// interface. This provides an opportunity to add additional class mappings, modify the configuration,
	/// add interceptors and more.
	/// </remarks>
	public interface IConfigurationContributor
	{
		/// <summary>
		/// Contribute to the NHibernate <see cref="Configuration"/>.
		/// </summary>
		/// <param name="alias">
		/// The alias of the database configuration being created. This value
		/// comes from the <see cref="IConfigurationBuilder.Alias"/> property.
		/// </param>
		/// <param name="isDefault">Is this the default configuration.</param>
		/// <param name="configuration"></param>
		void Contribute(string alias, bool isDefault, Configuration configuration);
	}
}