namespace Sentimeter.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class RiskSenti : DbContext
    {
        public RiskSenti()
            : base("name=RiskSentiAnalysis")
        {
        }

        public virtual DbSet<RiskSentiAnalysi> RiskSentiAnalysis { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RiskSentiAnalysi>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<RiskSentiAnalysi>()
                .Property(e => e.CurrentStatus)
                .IsUnicode(false);

            modelBuilder.Entity<RiskSentiAnalysi>()
                .Property(e => e.positivity)
                .HasPrecision(18, 15);
        }
    }
}
