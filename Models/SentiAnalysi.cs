namespace Sentimeter.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SentiAnalysi
    {
        public int id { get; set; }

        [Column(TypeName = "text")]
        public string Comment { get; set; }

        [Column(TypeName = "text")]
        public string Assetid { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? positivity { get; set; }

        public int? AnalysisCode { get; set; }
    }
}
