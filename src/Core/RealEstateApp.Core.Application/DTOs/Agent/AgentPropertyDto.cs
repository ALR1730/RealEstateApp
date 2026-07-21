namespace RealEstateApp.Core.Application.DTOs.Agent
{
    /// <summary>
    /// DTO resumido de propiedad vinculada a un agente.
    /// Usado en el endpoint de detalle del agente.
    /// </summary>
    public class AgentPropertyDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal SizeInMeters { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
