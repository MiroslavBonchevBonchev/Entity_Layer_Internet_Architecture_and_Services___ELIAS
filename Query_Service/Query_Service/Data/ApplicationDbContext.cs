using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Query_Service.Data
{
   public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>( options )
   {
   }
}
