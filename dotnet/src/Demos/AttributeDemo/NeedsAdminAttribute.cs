namespace Demos.AttributeDemo
{
    [System.AttributeUsage(AttributeTargets.Method)]
    internal class NeedsAdminAttribute : Attribute
    {
        public string Message { get; }

        public NeedsAdminAttribute(string message)
        {
            Message = message;
        }
    }
}
