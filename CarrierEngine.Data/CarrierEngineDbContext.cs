﻿// <auto-generated>
// ReSharper disable All
#pragma warning disable 1591    //  Ignore "Missing XML Comment" warning

using CarrierEngine.Data.Configuration;
using CarrierEngine.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CarrierEngine.Data;

public class CarrierEngineDbContext : DbContext
{
    public CarrierEngineDbContext()
    {
    }

    public CarrierEngineDbContext(DbContextOptions<CarrierEngineDbContext> options)
    : base(options)
    {
    }

    public DbSet<Carrier> Carriers { get; set; } // Carrier
    public DbSet<CarrierTrackingCodeMap> CarrierTrackingCodeMaps { get; set; } // CarrierTrackingCodeMap

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Data Source=SHAWK02;Initial Catalog=CarrierEngine;Integrated Security=True;MultipleActiveResultSets=True");
        }
    }
     

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CarrierConfiguration());
        modelBuilder.ApplyConfiguration(new CarrierTrackingCodeMapConfiguration());
    }

}