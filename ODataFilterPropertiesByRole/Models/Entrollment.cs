using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODataFilterPropertiesByRole.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public enum Grade
    {
        A, B, C, D, F
    }

    public class Enrollment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrollmentID { get; set; }

        public Grade? Grade { get; set; }

        public virtual Course Course { get; set; }

        public virtual ApplicationUser Student { get; set; }
    }
}