using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pype.Validation.DataAnnotations
{
    /// <summary>
    /// Specifies that property is a complex object to validate.
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ObjectAttribute : ValidationAttribute
    {
        /// <summary>
        /// Returns true if object is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            var results = new List<ValidationResult>();
            var context = new ValidationContext(value, serviceProvider: null, items: null);

            if (!Validator.TryValidateObject(value, context, results, validateAllProperties: true))
            {
                return new AggregateValidationResult(validationContext?.DisplayName, results);
            }

            return ValidationResult.Success;
        }
    }
}
