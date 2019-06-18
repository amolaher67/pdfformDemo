using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using PdfForm.Models;
using PdfForm.ResponseModels;

namespace PdfForm
{
    public static class PdfHelper
    {
        public static List<PdfFormfields> ReadPdfFormfieldses(string pdfPath)
        {
            var FieldList = new List<PdfFormfields>();
            try
            {
                using (var reader = new PdfReader(pdfPath))
                {
                    var fields = reader.AcroFields.Fields;

                    foreach (var key in fields.Keys)
                    {
                        var value = reader.AcroFields.GetField(key);
                        Console.WriteLine(key + " : " + value);
                        FieldList.Add(new PdfFormfields()
                        {
                            Key = key,
                            Value = value
                        });
                    }
                }
                return FieldList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string GeneratePdfFromExisingValues(List<PdfFormfields> fieldlist)
        {
            try
            {
                var pdfTemplate = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Download", "form.pdf");
                var newFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", $"form_{Guid.NewGuid()}.pdf");
                using (PdfReader pdfReader = new PdfReader(pdfTemplate))
                {
                    using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create)))
                    {
                        AcroFields pdfFormFields = pdfStamper.AcroFields;

                        foreach (var field in fieldlist)
                        {
                            pdfFormFields.SetField(field.Key, field.Value);
                        }
                        AddFieldToPDF(pdfStamper);

                        pdfStamper.Close();
                    }
                    pdfReader.Close();
                }

                return newFile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void AddFieldToPDF(PdfStamper pdfStamper)
        {
            TextField moreText = new TextField(pdfStamper.Writer,
                new iTextSharp.text.Rectangle(20, 20, 590, 780), "moreText");

            moreText.Visibility = TextField.VISIBLE_BUT_DOES_NOT_PRINT;
            moreText.Text = "Use this space for any additional information";
            moreText.Options = (TextField.MULTILINE);

            PdfFormField Fieldtxt = moreText.GetTextField();

            pdfStamper.AddAnnotation(Fieldtxt, 1);
        }


        public static List<PdfFormfields> GetTemplatefields(InitialLoadModel model)
        {
            return new List<PdfFormfields>()
            {
                    new PdfFormfields(){Key = "txt_Initial",Value = model.Initials},
                    new PdfFormfields(){Key = "txt_Surname",Value = model.Surname},
                    new PdfFormfields(){Key = "txt_SouthAfricanID",Value = model.IDNumber},
                    new PdfFormfields(){Key = "txt_EmailAddres",Value = model.EmailAddress},
                    new PdfFormfields(){Key = "txt_ContactNumber",Value = model.ContactNumber},
                    new PdfFormfields(){Key = "txt_grossMonthlyIncome",Value = model.GrossMonthlyIncome},
                    new PdfFormfields(){Key = "rb_LegalQuestion.1",Value = model.ApplicantCreditEvaluation11?.ToString()},
                    new PdfFormfields(){Key = "rb_LegalQuestion.2",Value = model.ApplicantCreditEvaluation2?.ToString()},
                    new PdfFormfields(){Key = "rb_LegalQuestion.3",Value = model.ApplicantCreditEvaluation3?.ToString()},
                    new PdfFormfields(){Key = "rb_LegalQuestion.4",Value = model.ApplicantCreditEvaluation5?.ToString()},
            };
        }

    }
}
