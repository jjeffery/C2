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

using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Castle.Facilities.NH.Tests.Model
{
	/// <summary>
	/// The ubiquitous sample blog data model
	/// </summary>
	public class Blog
	{
			public Blog()
			{
				// ReSharper disable DoNotCallOverridableMethodsInConstructor
				Items = new List<BlogItem>();
				// ReSharper restore DoNotCallOverridableMethodsInConstructor
			}

			public virtual int Id { get; set; }

			public virtual string Name { get; set; }

			public virtual IList<BlogItem> Items { get; set; }
	}

	namespace Mappers
	{
		public class BlogMapping : ClassMapping<Blog>
		{
			public BlogMapping()
			{
				Table("Blogs");
				DynamicInsert(true);
				DynamicUpdate(true);
				Id(x => x.Id, m => m.Generator(Generators.Identity));
				Property(x => x.Name, m => m.Length(64));
				Bag(x => x.Items, m =>
				                  	{
				                  		m.Inverse(true);
				                  		m.OrderBy(x => x.ItemDate);
				                  		m.Cascade(Cascade.All);
				                  		m.Key(k => k.Column("BlogId"));
				                  	},
				    m => m.OneToMany(b => b.Class(typeof(BlogItem))));
			}
		}
	}
}
