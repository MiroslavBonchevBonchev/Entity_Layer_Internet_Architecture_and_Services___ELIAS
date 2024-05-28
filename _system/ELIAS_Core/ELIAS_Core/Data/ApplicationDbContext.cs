using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ELIAS_Core.Data
{
   public class ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : IdentityDbContext<ApplicationUser>( options )
   {
   }
}
