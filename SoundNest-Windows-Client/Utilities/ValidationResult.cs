using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
    public enum ValidationErrorType
    {
        IncompleteData,
        InvalidData,
        GeneralError
    }

    public class ValidationResult
    {
        public bool Result { get; init; }
        public string? Message { get; init; }
        public string? Tittle { get; init; }

        public static ValidationResult Success() => new() { Result = true };

        public static ValidationResult Failure(string message, ValidationErrorType errorType)
        {
            return new ValidationResult
            {
                Result = false,
                Message = message,
                Tittle = GetTitleFromErrorType(errorType)
            };
        }

        private static string GetTitleFromErrorType(ValidationErrorType errorType)
        {
            return errorType switch
            {
                ValidationErrorType.IncompleteData => "Campos incompletos",
                ValidationErrorType.InvalidData => "Datos inválidos",
                ValidationErrorType.GeneralError => "Error",
                _ => "Error"
            };
        }
    }
}
