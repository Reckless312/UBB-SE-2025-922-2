using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ServerAPI.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(DatabaseContext context)
        {
            var sql = File.ReadAllText("Data/seed_data.sql");
            context.Database.ExecuteSqlRaw(sql);
        }
    }
} 