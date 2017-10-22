using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;

namespace ODataFilterPropertiesByRole
{
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;

    using ODataFilterPropertiesByRole.Models;
    using ODataFilterPropertiesByRole.Utils;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            var builder = new ODataConventionModelBuilder();
            builder.EnableLowerCamelCase();
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            var userEntitySet = builder.EntitySet<ApplicationUser>("Users");
            var enrollmentEntitySet = builder.EntitySet<Enrollment>("Enrollments");
            var courseEntitySet = builder.EntitySet<Course>("Courses");

            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: "odata",
                model: builder.GetEdmModel());

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            // For the default role, we want to hide the email address
            userEntitySet.AddFilter("Default")
                .RemoveProperty(p => p.Email)
                .RemoveProperty(p => p.EmailConfirmed)
                .RemoveProperty(p => p.AccessFailedCount)
                .RemoveProperty(p => p.LockoutEnabled)
                .RemoveProperty(p => p.PasswordHash)
                .RemoveProperty(p => p.Roles)
                .RemoveProperty(p => p.SecurityStamp)
                .RemoveProperty(p => p.TwoFactorEnabled)
                .RemoveProperty(p => p.PhoneNumberConfirmed)
                .RemoveProperty(p => p.Logins)
                .RemoveProperty(p => p.LockoutEndDateUtc);

            // If the user has an admin role they won't filter anything
            userEntitySet.AddFilter("Administrator", message => CheckForAdminAccount(message))
                .RemoveProperty(p => p.EmailConfirmed)
                .RemoveProperty(p => p.AccessFailedCount)
                .RemoveProperty(p => p.LockoutEnabled)
                .RemoveProperty(p => p.PasswordHash)
                .RemoveProperty(p => p.SecurityStamp)
                .RemoveProperty(p => p.TwoFactorEnabled)
                .RemoveProperty(p => p.PhoneNumberConfirmed)
                .RemoveProperty(p => p.LockoutEndDateUtc);

            // We allow admin to see the Grade property
            enrollmentEntitySet.AddFilter("Default").RemoveProperty(p => p.Grade);
            enrollmentEntitySet.AddFilter("Administrator", message => CheckForAdminAccount(message));

            
            courseEntitySet.AddFilter("Default");
        }

        private static bool CheckForAdminAccount(HttpRequestMessage message)
        {
            var user = message.GetOwinContext().Authentication.User;
            if (user == null)
            {
                return false;
            }

            return user.IsInRole("Administrator");
        }
    }
}
