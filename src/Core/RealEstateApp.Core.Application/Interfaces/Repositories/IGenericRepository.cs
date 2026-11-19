namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> : Core.Domain.Interfaces.IGenericRepository<T>
        where T : class
    {
    }
}
