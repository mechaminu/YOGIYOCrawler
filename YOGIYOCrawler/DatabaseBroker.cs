using System;
using Microsoft.EntityFrameworkCore;

namespace YOGIYOCrawler
{
    class DatabaseBroker : DbContext
    {
        
        public DbSet<Product> Products { get; set; }
        
       
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(@"server=minuuoo97.ddns.net:52022;uid=admin;pwd=admin123;");    // Connection String
    }
}
