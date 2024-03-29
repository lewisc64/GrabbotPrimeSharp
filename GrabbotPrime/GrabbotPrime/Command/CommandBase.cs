﻿using System.Threading.Tasks;

namespace GrabbotPrime.Command
{
    public abstract class CommandBase : ICommand
    {
        public Core Core { get; set; }

        public abstract bool Recognise(string message);

        public abstract Task Run(string message, ICommandContext context);

        protected async Task<string> Ask(string question, ICommandContext context)
        {
            await context.SendMessage(question);
            return await context.WaitForMessage();
        }
    }
}
