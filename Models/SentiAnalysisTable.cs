namespace Sentimeter.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SentiAnalysisTable : DbContext
    {
        public SentiAnalysisTable()
            : base("name=SentiAnalysisTable")
        {
        }

        public virtual DbSet<SentiAnalysi> SentiAnalysis { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SentiAnalysi>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<SentiAnalysi>()
                .Property(e => e.positivity)
                .HasPrecision(18, 0);
        }
    }
}
