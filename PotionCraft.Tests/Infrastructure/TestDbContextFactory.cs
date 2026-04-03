using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PotionCraft.Repository;

namespace PotionCraft.Tests.Infrastructure;

/// <summary>
/// Фабрика для создания тестового контекста БД на основе SQLite in-memory.
/// Поддерживает транзакции в отличие от EF InMemory провайдера.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Создаёт новый экземпляр PotionCraftDbContext с SQLite in-memory соединением.
    /// Вызывающий код должен закрыть соединение после завершения теста.
    /// </summary>
    public static (PotionCraftDbContext context, SqliteConnection connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<PotionCraftDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new PotionCraftDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
