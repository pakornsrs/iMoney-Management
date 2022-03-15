using iMoney_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace iMoney_API
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<ConfigCodeEntity> AppConfigCodeData { get; set; }
        public DbSet<TransactionRecordEntity> AppTransactionRecord { get; set; }
    }
}