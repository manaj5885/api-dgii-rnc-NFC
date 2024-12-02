namespace DGII_New.Models
{
    public class ConsultaRequest
    {
        public string? RNC { get; set; }
        public string? NCF { get; set; }
        public string? RncComprador { get; set; }
        public string? CodigoSeguridad { get; set; }
    }

    public class ConsultaResponse
    {
        public string? RncEmisor { get; set; }
        public string? RncComprador { get; set; }
        public string? ENCF { get; set; }
        public string? CodigoSeguridad { get; set; }
        public string? Estado { get; set; }
        public string? MontoTotal { get; set; }
        public string? TotalItbis { get; set; }
        public string? FechaEmision { get; set; }
        public string? FechaFirma { get; set; }
    }


}
