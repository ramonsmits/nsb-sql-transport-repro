namespace Service
{
    public class MyCommand
    {
        public MyCommand(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}