using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Data
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ExploreGambiaDbContext context;

        public EfUnitOfWork(ExploreGambiaDbContext context)
        {
            this.context = context;
        }

        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        {
            var executionStrategy = context.Database.CreateExecutionStrategy();

            return executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            });
        }
    }
}
