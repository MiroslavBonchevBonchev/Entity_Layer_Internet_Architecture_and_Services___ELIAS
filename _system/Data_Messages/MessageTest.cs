using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Messages
{
   public record MessageTestForward
   {
      public Guid Id { get; set; }
      public DateTime SendTime_UTC { get; set; }
   }

   public record MessageTestResponse
   {
      public Guid Id { get; set; }
      public DateTime ResponseTime_UTC { get; set; }
   }
}
