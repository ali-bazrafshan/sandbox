namespace Demos.AttributeDemo
{
    internal class DeleteAccountService
    {
        [NeedsAdmin("You are not authorized.")]
        public void DeleteAccount(int id)
        {
            Console.WriteLine($"Account #{id} deleted.");
        }
    }
}
