namespace RealEstateApp.Core.Domain.Common
{
    public interface IHasConcurrencyToken
    {
        int RowVersion { get; }
        void IncrementRowVersion();
    }
}
