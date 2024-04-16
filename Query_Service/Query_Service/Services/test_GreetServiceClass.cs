using Grpc.Core;
using Query_Service.Protos;

namespace Query_Service.Services
{
   public class test_GreetServiceClass : test_greeter.test_greeterBase
   {
      private readonly ILogger<test_GreetServiceClass> _logger;
      public test_GreetServiceClass( ILogger<test_GreetServiceClass> logger )
      {
         _logger = logger;
      }

      public override Task<test_HelloReply> SayHello( test_HelloRequest request, ServerCallContext context )
      {
         return Task.FromResult( new test_HelloReply
         {
            Message = "Hello " + request.Name
         } );
      }
   }
}
