namespace RealEstateApp.Core.Application.ViewModels.Improvement
{
    /// <summary>
    /// ViewModel de lectura para mejoras.
    /// </summary>
    public class ImprovementViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
