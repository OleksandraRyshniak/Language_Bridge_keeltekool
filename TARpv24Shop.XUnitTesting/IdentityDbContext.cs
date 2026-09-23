using Keeltekooli.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Text;

namespace TARpv24Keeltekooli.XUnitTesting
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
    public class AdaptedDbContext : IdentityDbContext<ApplicationUser>
    {
        public AdaptedDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {

        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Keelekursus> Keelekursus { get; set; }
        public DbSet<Opetaja> Opetaja { get; set; }
        public DbSet<Koolitus> Koolitus { get; set; }
        public DbSet<Registreerimine> Registreerimine { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Koolitus>();
        }
    }
}
    
