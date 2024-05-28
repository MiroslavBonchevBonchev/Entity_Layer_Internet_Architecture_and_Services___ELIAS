using Dapper;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Data_Access.Models;
using Microsoft.Extensions.Caching.Distributed;
using ELIAS_Core.Extensions;
using System.Collections.Generic;

namespace Data_Access
{
   public interface IDBExecute
   {
      Task< Tuple< IEnumerable< TempModel >, bool > > LoadAsync< T, U >( Stored_procedure stored_procedure, U parameters );
      Task< int > SaveAsync< T >( Stored_procedure stored_procedure, T parameters );

      //Task< List< T > > LoadAsync< T, U >( string sql, U parameters );
      //Task< int > SaveAsync< T >( string sql, T parameters );

   }

   public class DBExecute : IDBExecute
   {
      private readonly string _connection_string;
      private readonly IDistributedCache _cache;

      public DBExecute( IConfiguration config, IDistributedCache cache )
      {
         _connection_string = config.GetConnectionString( "QS_WorkDB_Connection" ) ?? throw new Exception( "Unknown connection string \"QS_WorkDB_Connection\"." );
         _cache = cache;
      }

      public async Task< Tuple< IEnumerable< TempModel >, bool > > LoadAsync< T, U >( Stored_procedure stored_procedure, U parameters )
      {
         bool is_from_db = false;
         string recordKey = "elias_QS_" + DateTime.Now.ToString( "yyyyMMdd_hhmm" );

         IEnumerable< TempModel > forecasts = await _cache.GetRecordAsync< IEnumerable< TempModel > >( recordKey );

         if( forecasts is null )
         {
            using( IDbConnection connection = new MySqlConnection( _connection_string ) )
            {
               try
               {
                  forecasts = (IEnumerable< TempModel >)await connection.QueryAsync< T >( stored_procedure.ToString(), parameters, null, null, CommandType.StoredProcedure );
               }
               catch( Exception ex )
               {
                  Console.WriteLine( ex.ToString() );
               }
            }

            await _cache.SetRecordAsync( recordKey, forecasts );

            is_from_db = true;
         }

         return new Tuple<IEnumerable<TempModel>, bool>( forecasts ?? [], is_from_db );
      }

      public async Task< int > SaveAsync< T >( Stored_procedure stored_procedure, T parameters )
      {
         using( IDbConnection connection = new MySqlConnection( _connection_string ) )
         {
            return await connection.ExecuteAsync( stored_procedure.ToString(), parameters, null, null, CommandType.StoredProcedure );
         }
      }

      private async Task< List< T > > LoadAsync< T, U >( string sql, U parameters )
      {
         using( IDbConnection connection = new MySqlConnection( _connection_string ) )
         {
            var res = await connection.QueryAsync< T >( sql, parameters );
            
            return res.ToList();
         }
      }

      private async Task< int > SaveAsync< T >( string sql, T parameters )
      {
         using( IDbConnection connection = new MySqlConnection( _connection_string ) )
         {
            return await connection.ExecuteAsync( sql, parameters );
         }
      }
   }
}
