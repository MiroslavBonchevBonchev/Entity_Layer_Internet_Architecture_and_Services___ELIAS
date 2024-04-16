﻿using MassTransit;
using Data_Messages;

namespace Query_Service.Services
{
   public sealed class TestEventRabbitMQCaptureResponse : IConsumer< MessageTestResponse >
   {
      public Task Consume( ConsumeContext< MessageTestResponse > context )
      {
         Console.WriteLine( $"MessageTestResponse: Message id = {context.Message.Id}, Response time = {context.Message.ResponseTime_UTC }." );

         return Task.CompletedTask;
      }
   }
}
