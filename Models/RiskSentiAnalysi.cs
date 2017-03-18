namespace Sentimeter.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RiskSentiAnalysi
    {
        public List<RiskSentiAnalysi> RiskSentiAnalysis;

        public RiskSentiAnalysi()
        {

            RiskSentiAnalysis = new List<RiskSentiAnalysi>();
          
        }
        public int id { get; set; }

        
        [Column(TypeName = "text")]
        public string Comment { get; set; }

        public int AssetId { get; set; }

        [StringLength(20)]
        public string CurrentStatus { get; set; }

        public DateTime? updateddate { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? positivity { get; set; }

        public int? AnalysisCode { get; set; }
    }
}
