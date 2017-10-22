using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataFilterPropertiesByRole.Filters
{
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData;
    using System.Web.OData.Extensions;
    using System.Web.OData.Query;

    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;

    using ODataFilterPropertiesByRole.Utils;

    public class ODataEntityQueryFilterAttribute : EnableQueryAttribute
    {
        public ODataEntityQueryFilterAttribute()
        {
            this.PageSize = 100;
            this.AllowedQueryOptions = AllowedQueryOptions.All;
        }

        public override object ApplyQuery(object entity, ODataQueryOptions queryOptions)
        {
            var result = base.ApplyQuery(entity, queryOptions);
            this.UpdateQueryFilter(queryOptions, entity.GetType());
            return result;
        }

        public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            var result = base.ApplyQuery(queryable, queryOptions);
            this.UpdateQueryFilter(queryOptions, queryable.ElementType);
            return result;
        }

        private void UpdateQueryFilter(ODataQueryOptions queryOptions, Type queryableElementType)
        {
            var filteredPropertiesByType = queryOptions.GetFilterMap(queryOptions.Request);
            var originalRequest = queryOptions.Request;
            var oDataProperties = originalRequest.ODataProperties();

            Func<SelectExpandClause, ExpandedNavigationSelectItem, Type, SelectExpandClause> fillTypesRecursive = null;
            fillTypesRecursive = (selectExpandClause, parent, type) =>
                {
                    if (selectExpandClause == null)
                    {
                        return null;
                    }

                    var propertySet = filteredPropertiesByType.ContainsKey(type) ? filteredPropertiesByType[type] : null;
                    if (parent != null && selectExpandClause.AllSelected)
                    {
                        selectExpandClause = GetSelectExpandClause(
                            GetEdmType(parent.NavigationSource.Type),
                            queryOptions,
                            propertySet,
                            oDataProperties,
                            selectExpandClause.SelectedItems.Where(s => s is ExpandedNavigationSelectItem).Cast<ExpandedNavigationSelectItem>()) ?? selectExpandClause;
                    }

                    var selectItems = new List<SelectItem>();
                    foreach (var selectItem in selectExpandClause.SelectedItems)
                    {
                        var pathItem = selectItem as PathSelectItem;
                        if (pathItem != null && propertySet != null && !propertySet.Properties.Contains(pathItem.SelectedPath.FirstSegment.Identifier))
                        {
                            continue;
                        }

                        var expandedItem = selectItem as ExpandedNavigationSelectItem;
                        if (expandedItem != null)
                        {
                            if (propertySet != null && !propertySet.NavigationProperties.Contains(
                                    expandedItem.PathToNavigationProperty.FirstSegment.Identifier))
                            {
                                continue;
                            }

                            var edmType = GetEdmType(expandedItem.NavigationSource.Type);
                            var clrTypeAnnotation = queryOptions.Context.Model.GetAnnotationValue<ClrTypeAnnotation>(edmType);
                            expandedItem = new ExpandedNavigationSelectItem(
                                expandedItem.PathToNavigationProperty,
                                expandedItem.NavigationSource,
                                fillTypesRecursive(expandedItem.SelectAndExpand, expandedItem, clrTypeAnnotation.ClrType),
                                expandedItem.FilterOption,
                                expandedItem.OrderByOption,
                                expandedItem.TopOption,
                                expandedItem.SkipOption,
                                expandedItem.CountOption,
                                expandedItem.SearchOption,
                                expandedItem.LevelsOption);
                            selectItems.Add(expandedItem);
                        }
                        else
                        {
                            selectItems.Add(selectItem);
                        }
                    }

                    return new SelectExpandClause(selectItems, selectExpandClause.AllSelected);
                };


            var clause = oDataProperties.SelectExpandClause;
            if (clause == null)
            {
                var propertySet = filteredPropertiesByType[queryableElementType];
                oDataProperties.SelectExpandClause = GetSelectExpandClause(
                    oDataProperties.Path.EdmType.AsElementType(),
                    queryOptions, 
                    propertySet,
                    oDataProperties);
            }
            else
            {
                originalRequest.ODataProperties().SelectExpandClause =
                    fillTypesRecursive(clause, null, queryableElementType);
            }
        }

        private static IEdmType GetEdmType(IEdmType edmType)
        {
            if (edmType is EdmEntityType)
            {
                edmType = ((EdmEntityType)edmType).AsActualType();
            }
            else if (edmType is EdmCollectionType)
            {
                edmType = ((EdmCollectionType)edmType).ElementType.Definition.AsActualType();
            }
            return edmType;
        }

        private static SelectExpandClause GetSelectExpandClause(
            IEdmType edmType, 
            ODataQueryOptions queryOptions, 
            EntityPropertyFilterConfig.PropertySet propertySet, 
            HttpRequestMessageProperties oDataProperties, 
            IEnumerable<ExpandedNavigationSelectItem> selectItems = null)
        {
            if (propertySet == null)
            {
                return null;
            }

            var selectExpandDictionary =
                new Dictionary<string, string> { { "$select", string.Join(", ", propertySet.Properties) }, };
            if (selectItems != null)
            {
                selectExpandDictionary.Add("$expand", string.Join(", ", selectItems.Select(s => s.PathToNavigationProperty.FirstSegment.Identifier)));
            }

            var parser = new ODataQueryOptionParser(
                queryOptions.Context.Model,
                edmType,
                oDataProperties.Path.NavigationSource,
                selectExpandDictionary);
            return parser.ParseSelectAndExpand();
        }
    }
}