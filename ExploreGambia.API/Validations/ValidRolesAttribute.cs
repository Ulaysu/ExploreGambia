using System.ComponentModel.DataAnnotations;

namespace ExploreGambia.API.Validations
{
    public class ValidRolesAttribute : ValidationAttribute
    {
        private readonly string[] allowedRoles;

        public ValidRolesAttribute(string[] allowedRoles)
        {
            this.allowedRoles = allowedRoles;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("The Roles field is required."); // Return an error if Roles is null
            }

            if (value is string[] roles)
            {
                if (roles.Any(string.IsNullOrEmpty)) // Check for null or empty strings
                {
                    return new ValidationResult("The Roles array cannot contain empty strings.");
                }

                if (roles.All(r => allowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"One or more roles are invalid. Allowed roles: {string.Join(", ", allowedRoles)}.");
                }
            }


            return new ValidationResult("The Roles field must be an array of strings.");
        }
    }
}
