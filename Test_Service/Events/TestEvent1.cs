using MassTransit;
using Data_Messages;
using MassTransit.Transports;

namespace Test_Service.Events
{
   public sealed class TestEvent1Consumer : IConsumer< MessageTestForward >
   {
      private IPublishEndpoint _publish_endpoint;
       
      public TestEvent1Consumer( IPublishEndpoint publish_endpoint )
      {
         _publish_endpoint = publish_endpoint;
      }

      public Task Consume( ConsumeContext< MessageTestForward > context )
      {
         Console.WriteLine( $"MessageTestForward: Message id = {context.Message.Id}, Send time = {context.Message.SendTime_UTC }." );

         _publish_endpoint.Publish( new MessageTestResponse { Id = Guid.NewGuid(), ResponseTime_UTC = DateTime.UtcNow } );

         return Task.CompletedTask;
      }
   }
}
