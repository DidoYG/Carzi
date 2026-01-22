using Microsoft.EntityFrameworkCore;
using Carzi.Models;

namespace Carzi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<Vignette> Vignettes { get; set; }
        public DbSet<TplInsurance> TplInsurances { get; set; }
        public DbSet<AnnualInspection> AnnualInspections { get; set; }
        public DbSet<Trip> Trips { get; set; }
    }
}
