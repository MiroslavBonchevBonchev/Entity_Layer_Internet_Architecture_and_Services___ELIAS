using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Access
{
   public enum Stored_procedure
   {
      Get_all_temperatures,
      Get_date_temperature,
      Insert_temperature
   }

   public enum DB_provider
   {
      MySQL,
      MSSQL,
      PostGRE
   };
}
