using System.Collections.Generic;

namespace Autyan.Identity.Service.SignIn
{
    public class SignInResult
    {
        private static readonly SignInResult _success = new SignInResult
        {
            Succeed = true
        };

        public bool Succeed { get; protected set; }

        public List<SignInError> SignInErrors { get; protected set; } = new List<SignInError>();

        public static SignInResult Success() => _success;

        public static SignInResult Failed(SignInError[] errors)
        {
            var result = new SignInResult
            {
                Succeed = false
            };

            if (errors != null)
            {
                result.SignInErrors.AddRange(errors);
            }

            return result;
        }

        public static SignInResult Failed(SignInError error)
        {
            var result = new SignInResult
            {
                Succeed = false
            };

            if (error != null)
            {
                result.SignInErrors.Add(error);
            }

            return result;
        }

        public static SignInResult Failed(string description, SignInErrors errorCode)
        {
            var result = new SignInResult
            {
                Succeed = false
            };

            result.SignInErrors.Add(new SignInError
            {
                Description = description,
                ErrorCode = errorCode
            });

            return result;
        }
    }

    public class SignInError
    {
        public SignInErrors ErrorCode { get; set; }

        public string Description { get; set; }
    }
}
