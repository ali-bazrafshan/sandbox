using System.Reflection;

namespace Demos.AttributeDemo
{
    internal class Handler
    {
        private readonly DeleteAccountService _accountService;

        public Handler(DeleteAccountService accountService)
        {
            _accountService = accountService;
        }

        public void Run()
        {
            var method = _accountService.GetType().GetMethod("DeleteAccount");
            var attribute = method?.GetCustomAttribute<NeedsAdminAttribute>();
            if (attribute is null)
            {
                _accountService.DeleteAccount(1);
            }
            else
            {
                Console.WriteLine(attribute?.Message);
            }
        }
    }
}
