using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataFilterPropertiesByRole.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Course
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseID { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}