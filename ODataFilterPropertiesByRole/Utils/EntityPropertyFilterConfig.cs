namespace ODataFilterPropertiesByRole.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.OData.Builder;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class EntityPropertyFilterConfig
    {
        public EntityPropertyFilterConfig()
        {
            this.Filters = new List<EntityPropertyFilter>();
        }

        /// <summary>
        /// The property sets.
        /// </summary>
        private Dictionary<Type, PropertySet> propertySets = new Dictionary<Type, PropertySet>();

        /// <summary>
        /// Gets the filters.
        /// </summary>
        private List<EntityPropertyFilter> Filters { get; }

        /// <summary>
        /// This adds all the fields in the OData model to a dictionary which we will use
        /// to filter fields in the ODataEntityQueryFilterAttribute
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <typeparam name="TEntityType">
        /// </typeparam>
        /// <exception cref="Exception">
        /// </exception>
        public void SetupEntity<TEntityType>(EntityTypeConfiguration<TEntityType> config) where TEntityType : class
        {
            var type = typeof(TEntityType);
            if (this.propertySets.ContainsKey(type))
            {
                return;
            }

            var navigationProperties = config.NavigationProperties
                .Where(prop => !prop.NotNavigable)
                .ToList();
            var properties = config.Properties
                .Where(prop => !prop.NotNavigable)
                .ToList();

            if (!properties.Any() && !navigationProperties.Any())
            {
                throw new Exception("You must call GetEdmModel on the builder before adding filters.");
            }

            var apiNameByPropertyName = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                var attribute = property.PropertyInfo.GetCustomAttributes(true)
                    .FirstOrDefault(attr => attr is JsonPropertyAttribute) as JsonPropertyAttribute;
                apiNameByPropertyName.Add(
                    property.PropertyInfo.Name,
                    attribute != null ? attribute.PropertyName : property.Name);
            }

            this.propertySets.Add(
                type,
                new PropertySet
                    {
                        ApiNameByPropertyName = apiNameByPropertyName,
                        NavigationProperties = navigationProperties.Select(prop => prop.Name).ToList(),
                        Properties = properties.Select(prop => prop.Name).ToList(),
                    });
        }

        public EntityPropertySelector<TEntityType> AddFilter<TEntityType>(string key)
            where TEntityType : class
        {
            return this.AddFilter<TEntityType>(key, request => true);
        }

        public EntityPropertySelector<TEntityType> AddFilter<TEntityType>(
            string key,
            Expression<Func<HttpRequestMessage, bool>> filterExpression)
            where TEntityType : class
        {
            var existingFilter = this.GetForKey<TEntityType>(key);

            return existingFilter != null ? 
                new EntityPropertySelector<TEntityType>(existingFilter) : 
                new EntityPropertySelector<TEntityType>(this, key, filterExpression.Compile());
        }

        public EntityPropertyFilter GetForKey<TEntityType>(string key)
        {
            return this.Filters.FirstOrDefault(f => f.Key == key && f.EntityType == typeof(TEntityType));
        }

        public IDictionary<Type, PropertySet> GetAvailableProperties(HttpRequestMessage message)
        {
            var groupedFilters = this.Filters
                .Where(f => f.Filter(message))
                .GroupBy(g => g.EntityType);

            var output = new Dictionary<Type, PropertySet>();
            foreach (var groupedFilter in groupedFilters)
            {
                var propsForGroup = groupedFilter.SelectMany(t => t.Properties);
                var filterPerEntityCount = groupedFilter.GroupBy(g => g.Key).Count();

                var existingPropertySet = this.propertySets[groupedFilter.Key];
                var propsToRemove =
                    propsForGroup.GroupBy(p => p)
                        .Where(g => g.Any() && g.Count() == filterPerEntityCount)
                        .Select(g => g.FirstOrDefault())
                        .Where(prop => prop != null)
                        .Select(prop => existingPropertySet.ApiNameByPropertyName[prop]);

                output.Add(
                    groupedFilter.Key, 
                    new PropertySet
                        {
                            ApiNameByPropertyName = existingPropertySet.ApiNameByPropertyName,
                            NavigationProperties = existingPropertySet.NavigationProperties.Where(prop => !propsToRemove.Contains(prop)).ToList(),
                            Properties = existingPropertySet.Properties.Where(prop => !propsToRemove.Contains(prop)).ToList()
                    });
            }

            return output;
        }

        public class EntityPropertySelector<TEntityType>
            where TEntityType : class
        {
            private readonly EntityPropertyFilter filter;

            public EntityPropertySelector(EntityPropertyFilterConfig parent, string key, Func<HttpRequestMessage, bool> filterExpression)
            {
                this.filter = new EntityPropertyFilter(typeof(TEntityType), key, filterExpression);

                parent.Filters.Add(this.filter);
            }

            public EntityPropertySelector(EntityPropertyFilter filter)
            {
                this.filter = filter;
            }

            public EntityPropertySelector<TEntityType> RemoveProperty<TProperty>(
                Expression<Func<TEntityType, TProperty>> selector)
            {
                var propInfo = GetPropertyInfo(selector);
                this.filter.Properties.Add(propInfo.Name);
                return this;
            }

            private static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TEntityType, TProperty>> selector)
            {
                var type = typeof(TEntityType);

                var member = selector.Body as MemberExpression;
                if (member == null)
                {
                    throw new ArgumentException($"Expression '{selector}' refers to a method, not a property.");
                }

                var propInfo = member.Member as PropertyInfo;
                if (propInfo == null)
                {
                    throw new ArgumentException($"Expression '{selector}' refers to a field, not a property.");
                }

                if (type != propInfo.ReflectedType && (propInfo.ReflectedType == null
                                                       || !type.IsSubclassOf(propInfo.ReflectedType)))
                {
                    throw new ArgumentException(
                        $"Expresion '{selector}' refers to a property that is not from type {type}.");
                }

                return propInfo;
            }
        }

        public class PropertySet
        {
            public PropertySet()
            {
                this.ApiNameByPropertyName = new Dictionary<string, string>();
                this.Properties = new List<string>();
                this.NavigationProperties = new List<string>();
            }

            public Dictionary<string, string> ApiNameByPropertyName { get; set; }

            public List<string> Properties { get; set; }
            public List<string> NavigationProperties { get; set; }
        }

        public class EntityPropertyFilter
        {
            public EntityPropertyFilter(Type entityType, string key, Func<HttpRequestMessage, bool> filter)
            {
                this.EntityType = entityType;
                this.Key = key;
                this.Filter = filter;

                this.Properties = new List<string>();
            }

            public List<string> Properties { get; set; }

            public Type EntityType { get; set; }

            public string Key { get; }

            public Func<HttpRequestMessage, bool> Filter { get; }
        }
    }
}