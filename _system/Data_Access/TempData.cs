using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data_Access.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using ELIAS_Core.Extensions;

namespace Data_Access
{
   public interface ITemperatureData
   {
      public Task< Tuple< IEnumerable< TempModel >, bool > > GetTemps();
      public Task< TempModel? > GetTemp( DateTime date );
      public Task< int > InsertTemp( TempModel tm );
   }

   public class TemperatureData : ITemperatureData
   {
      private readonly IDBExecute _db_execute;
      private readonly IDistributedCache _cache;

      public TemperatureData( IDBExecute db_execute, IDistributedCache cache )
      {
         _db_execute = db_execute;
         _cache = cache;
      }

      public async Task< Tuple< IEnumerable< TempModel >, bool > > GetTemps()
      {
         var forecasts = await _db_execute.LoadAsync< TempModel, dynamic >( Stored_procedure.Get_all_temperatures, new { } );

         return forecasts;
      }
      public async Task< TempModel? > GetTemp( DateTime date )
      {
         var results = await _db_execute.LoadAsync< TempModel, dynamic >( Stored_procedure.Get_date_temperature, new { date = date } );

         return results.Item1.FirstOrDefault();
      }

      public async Task< int > InsertTemp( TempModel tm )
      {
         return await _db_execute.SaveAsync( Stored_procedure.Insert_temperature, new { date = tm.date, temp_c = tm.temp_c } );
      }
   }
}
