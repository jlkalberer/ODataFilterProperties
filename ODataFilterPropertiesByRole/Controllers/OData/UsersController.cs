namespace ODataFilterPropertiesByRole.Controllers.OData
{
    using System.Linq;
    using System.Web.Http;
    using System.Web.OData;

    using ODataFilterPropertiesByRole.Filters;
    using ODataFilterPropertiesByRole.Models;

    [RoutePrefix("odata")]
    public class UsersController : ODataController
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [ODataEntityQueryFilter]
        public IQueryable<ApplicationUser> Get()
        {
            return this.db.Users.ToList().AsQueryable();
        }

        [ODataEntityQueryFilter]
        public SingleResult<ApplicationUser> Get([FromODataUri] string key)
        {
            var result = this.db.Users.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}
