using System.Collections.Generic;

namespace Autyan.Identity.Service
{
    public class BusinessResult
    {
        protected static readonly BusinessResult SuccessResult = new BusinessResult
        {
            Succeed = true
        };

        public bool Succeed { get; protected set; }

        public IList<string> ErrorMessages { get; } = new List<string>();

        public static BusinessResult Failed(string errorMessage)
        {
            var result = new BusinessResult
            {
                Succeed = false
            };
            result.ErrorMessages.Add(errorMessage);

            return result;
        }

        protected void AddErrorMessage(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
        }
    }
}
