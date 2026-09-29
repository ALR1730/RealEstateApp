using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Interfaces
{
    /// <summary>
    /// Contrato de Unidad de Trabajo (Unit of Work) para orquestar la atomicidad
    /// transaccional entre múltiples repositorios y operaciones del dominio.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Persiste los cambios acumulados en el contexto actual.
        /// </summary>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicia una transacción explícita.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirma la transacción explícita actual y persiste los cambios.
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revierte la transacción explícita actual descartando los cambios no confirmados.
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una acción dentro de una transacción atómica con rollback automático ante cualquier fallo.
        /// </summary>
        Task ExecuteTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una función con retorno dentro de una transacción atómica con rollback automático ante cualquier fallo.
        /// </summary>
        Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
    }
}
