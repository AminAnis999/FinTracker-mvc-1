using FinTracker.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FinTracker.Repositories;

// ADO.NET repository. No repository interface or dependency injection is used.
public class TransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<FinanceTransaction>> GetAllAsync(string userId)
    {
        var items = new List<FinanceTransaction>();
        const string sql = @"SELECT Id, Title, Type, Amount, Category, TransactionDate, Note
                             FROM FinanceTransactions WHERE UserId = @UserId
                             ORDER BY TransactionDate DESC, Id DESC";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId;
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) items.Add(Map(reader));
        return items;
    }

    public async Task<FinanceTransaction?> GetByIdAsync(int id, string userId)
    {
        const string sql = @"SELECT Id, Title, Type, Amount, Category, TransactionDate, Note
                             FROM FinanceTransactions WHERE Id=@Id AND UserId=@UserId";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId;
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task AddAsync(FinanceTransaction item, string userId)
    {
        const string sql = @"INSERT INTO FinanceTransactions
            (Title, Type, Amount, Category, TransactionDate, Note, UserId)
            VALUES (@Title,@Type,@Amount,@Category,@Date,@Note,@UserId)";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, item, userId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(FinanceTransaction item, string userId)
    {
        const string sql = @"UPDATE FinanceTransactions SET Title=@Title, Type=@Type,
            Amount=@Amount, Category=@Category, TransactionDate=@Date, Note=@Note
            WHERE Id=@Id AND UserId=@UserId";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, item, userId);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        const string sql = "DELETE FROM FinanceTransactions WHERE Id=@Id AND UserId=@UserId";
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static void AddParameters(SqlCommand command, FinanceTransaction item, string userId)
    {
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 100).Value = item.Title;
        command.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = item.Type;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = item.Amount;
        command.Parameters["@Amount"].Precision = 18;
        command.Parameters["@Amount"].Scale = 2;
        command.Parameters.Add("@Category", SqlDbType.NVarChar, 60).Value = item.Category;
        command.Parameters.Add("@Date", SqlDbType.Date).Value = item.TransactionDate.Date;
        command.Parameters.Add("@Note", SqlDbType.NVarChar, 300).Value = (object?)item.Note ?? DBNull.Value;
        command.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId;
    }

    private static FinanceTransaction Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        Type = reader.GetString(2),
        Amount = reader.GetDecimal(3),
        Category = reader.GetString(4),
        TransactionDate = reader.GetDateTime(5),
        Note = reader.IsDBNull(6) ? null : reader.GetString(6)
    };
}
