namespace ODataFilterPropertiesByRole.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.OData.Builder;
    using System.Web.OData.Query;

    /// <summary>
    /// The entity set configuration helper.
    /// </summary>
    public static class EntitySetConfigurationHelper
    {
        /// <summary>
        /// The entity filter container.
        /// </summary>
        private static EntityPropertyFilterConfig entityFilterContainer = new EntityPropertyFilterConfig();

        public static IDictionary<Type, EntityPropertyFilterConfig.PropertySet> GetFilterMap(
            this ODataQueryOptions queryOptions, 
            HttpRequestMessage message)
        {
            return entityFilterContainer.GetAvailableProperties(message);
        }

        /// <summary>
        /// The add filter.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="TEntityType">
        /// </typeparam>
        /// <returns>
        /// The <see cref="EntityPropertyFilterConfig.EntityPropertySelector{TEntityType}"/>.
        /// </returns>
        public static EntityPropertyFilterConfig.EntityPropertySelector<TEntityType> AddFilter<TEntityType>(
            this EntitySetConfiguration<TEntityType> config,
            string key)
            where TEntityType : class
        {
            entityFilterContainer.SetupEntity(config.EntityType);
            return entityFilterContainer.AddFilter<TEntityType>(key);
        }

        /// <summary>
        /// The add filter.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="filterExpression">
        /// The filter expression.
        /// </param>
        /// <typeparam name="TEntityType">
        /// </typeparam>
        /// <returns>
        /// The <see cref="EntityPropertySelector"/>.
        /// </returns>
        public static EntityPropertyFilterConfig.EntityPropertySelector<TEntityType> AddFilter<TEntityType>(
            this EntitySetConfiguration<TEntityType> config,
            string key,
            Expression<Func<HttpRequestMessage, bool>> filterExpression)
            where TEntityType : class
        {
            entityFilterContainer.SetupEntity(config.EntityType);
            return entityFilterContainer.AddFilter<TEntityType>(key, filterExpression);
        }
    }
}