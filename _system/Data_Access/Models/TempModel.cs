using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access.Models
{
   public class TempModel
   {
      public DateTime date {  get; set; }
      public int temp_c { get; set; }

      public int temp_f => 32 + (int)(temp_c / 0.5556);

      public string summary
      {
         get
         {
            var rng = new Random();

            return Summaries[rng.Next( Summaries.Length )];

         }
      }

      private static readonly string[] Summaries = new[]
      {
         "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
      };
   }
}
