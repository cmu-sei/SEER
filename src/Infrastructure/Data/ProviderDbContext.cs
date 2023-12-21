// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using Microsoft.EntityFrameworkCore;

namespace Seer.Infrastructure.Data
{
    public class ApplicationDbContextInMemory : ApplicationDbContext
    {
        public ApplicationDbContextInMemory(DbContextOptions<ApplicationDbContextInMemory> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }

    public class ApplicationDbContextSqlLite : ApplicationDbContext
    {
        public ApplicationDbContextSqlLite(DbContextOptions<ApplicationDbContextInMemory> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }

    public class ApplicationDbContextSqlServer : ApplicationDbContext
    {
        public ApplicationDbContextSqlServer(DbContextOptions<ApplicationDbContextSqlServer> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }

    public class ApplicationDbContextPostgreSQL : ApplicationDbContext
    {
        public ApplicationDbContextPostgreSQL(DbContextOptions<ApplicationDbContextPostgreSQL> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}