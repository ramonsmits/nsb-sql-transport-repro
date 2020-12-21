using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Service
{
    public class MyCommandHandler : IHandleMessages<MyCommand>
    {
        public Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Handled command. Message = {message.Message}");

            return Task.CompletedTask;
        }
    }
}