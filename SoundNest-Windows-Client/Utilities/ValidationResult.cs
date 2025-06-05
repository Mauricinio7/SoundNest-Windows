using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
        public class ValidationResult
        {
            public bool Result { get; init; }
            public string? Message { get; init; }

            public static ValidationResult Success() => new() { Result = true };
            public static ValidationResult Failure(string message) => new() { Result = false, Message = message };
    }
}
