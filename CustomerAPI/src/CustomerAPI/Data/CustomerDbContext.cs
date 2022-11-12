using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CustomerAPI.Models;

namespace CustomerAPI.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext (DbContextOptions<CustomerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customer { get; set; } = default!;
    }
}
