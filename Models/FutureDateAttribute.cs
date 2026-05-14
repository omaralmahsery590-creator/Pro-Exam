using System.ComponentModel.DataAnnotations;

namespace pro_exam.Models
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date.Date > DateTime.Today)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "التاريخ يجب أن يكون مستقبلي");
        }
    }
}
