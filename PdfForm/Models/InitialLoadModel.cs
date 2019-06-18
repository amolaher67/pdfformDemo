using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfForm.Models
{
    public class InitialLoadModel
    {
        public int SelectedSubProductId { get; set; }
        public string Initials { get; set; }
        public string Surname { get; set; }
        public string IDNumber { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public string GrossMonthlyIncome { get; set; }
        public bool? ApplicantCreditEvaluation2 { get; set; }
        public bool? ApplicantCreditEvaluation11 { get; set; }
        public bool? ApplicantCreditEvaluation3 { get; set; }
        public bool? ApplicantCreditEvaluation5 { get; set; }
    }
}
