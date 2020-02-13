using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DbFiller
{
     public class CrownContext : DbContext
    {
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=exchange.db");
    }

    public class ExchangeRate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid     Id { get; set; }
        
        [Required]
        public Decimal  Rate { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public string   Currency { get; set; }
    }    
}