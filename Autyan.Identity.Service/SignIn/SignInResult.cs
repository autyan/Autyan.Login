namespace Autyan.Identity.Service.SignIn
{
    public class SignInResult : BusinessResult
    {
        protected new static readonly SignInResult SuccessResult = new SignInResult
        {
            Succeed = true
        };

        protected SignInResult(BusinessResult result) : this()
        {
            Succeed = result.Succeed;
        }

        public SignInResult()
        {

        }

        public SignInErrors ErrorCode { get; set; }

        public static SignInResult Success()
        {
            return SuccessResult;
        }

        public static SignInResult Failed(SignInErrors errorCode)
        {
            return new SignInResult
            {
                Succeed = false,
                ErrorCode = errorCode
            };
        }

        public static SignInResult Failed(string errorMessage, SignInErrors errorCode)
        {
            var result = new SignInResult
            {
                Succeed = false,
                ErrorCode = errorCode
            };
            result.AddErrorMessage(errorMessage);
            return result;
        }
    }
}
