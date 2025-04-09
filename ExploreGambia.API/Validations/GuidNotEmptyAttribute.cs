using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Validations
{
    public class GuidNotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is Guid guidValue)
            {
                if (guidValue == Guid.Empty)
                {
                    return new ValidationResult("The TourId cannot be an empty GUID.");
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("The TourId must be a GUID.");
        }

            

            
    }   
}
