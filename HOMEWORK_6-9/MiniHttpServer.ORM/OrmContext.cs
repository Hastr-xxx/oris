using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiniHttpServer.ORM
{
    public sealed class OrmContext : IOrnContext
    {
        private readonly string _connectionString;

        public OrmContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Строка подключения не задана.", nameof(connectionString));
            _connectionString = connectionString;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, new()
        {
            string safeTableName = EscapeIdentifier(tableName);
            string sql = $"SELECT * FROM {safeTableName}";
            var result = new List<T>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true).ToArray();

            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++) columns[reader.GetName(i)] = i;

            while (await reader.ReadAsync(cancellationToken))
            {
                T entity = new();
                foreach (var prop in properties)
                {
                    if (!columns.TryGetValue(prop.Name, out int ordinal)) continue;
                    if (await reader.IsDBNullAsync(ordinal, cancellationToken)) continue;

                    object value = reader.GetValue(ordinal);
                    Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object converted = targetType.IsInstanceOfType(value) ? value
                                     : targetType.IsEnum ? Enum.ToObject(targetType, value)
                                     : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                    prop.SetValue(entity, converted);
                }
                result.Add(entity);
            }
            return result;
        }

        public async Task<T?> GetByIdAsync<T>(string tableName, int id, CancellationToken cancellationToken = default) where T : class, new()
        {
            string safeTableName = EscapeIdentifier(tableName);
            string sql = $"SELECT * FROM {safeTableName} WHERE Id = @id";
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id); 

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null; 

            T entity = new();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var prop = properties.FirstOrDefault(p => p.Name.Equals(reader.GetName(i), StringComparison.OrdinalIgnoreCase));
                if (prop != null && !await reader.IsDBNullAsync(i, cancellationToken))
                {
                    Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object value = Convert.ChangeType(reader.GetValue(i), targetType, CultureInfo.InvariantCulture);
                    prop.SetValue(entity, value);
                }
            }
            return entity;
        }

        public async Task<int> InsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class
        {
            string safeTableName = EscapeIdentifier(tableName);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) 
                .ToArray();

            string columns = string.Join(", ", props.Select(p => p.Name));
            string parameters = string.Join(", ", props.Select(p => "@" + p.Name));
            string sql = $"INSERT INTO {safeTableName} ({columns}) OUTPUT INSERTED.Id VALUES ({parameters})";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);

            foreach (var prop in props)
                command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> UpdateAsync<T>(string tableName, int id, T entity, CancellationToken cancellationToken = default) where T : class
        {
            string safeTableName = EscapeIdentifier(tableName);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)) 
                .ToArray();

            string setClause = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));
            string sql = $"UPDATE {safeTableName} SET {setClause} WHERE Id = @Id";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@Id", id);
            foreach (var prop in props)
                command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> DeleteAsync(string tableName, int id, CancellationToken cancellationToken = default)
        {
            string safeTableName = EscapeIdentifier(tableName);
            string sql = $"DELETE FROM {safeTableName} WHERE Id = @Id";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static string EscapeIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) || !Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new ArgumentException("Недопустимое имя SQL-идентификатора.", nameof(identifier));
            return $"[{identifier}]";
        }
    }
}