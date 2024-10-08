﻿using Npgsql;
using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly string _connectionString;

        public DiscountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public int AddDiscount(Discount discount)
        {
            if (discount == null) throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");

            string insertSql = @"INSERT INTO discounts (name, description, discountpercent, active)
                                VALUES (@Name, @Description, @Discountpercent, @Active)
                                RETURNING id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", discount.Name);
                    command.Parameters.AddWithValue("@Description", discount.Description);
                    command.Parameters.AddWithValue("@Discountpercent", discount.DiscountPercent);
                    command.Parameters.AddWithValue("@Active", discount.IsActive);

                    object? result = command.ExecuteScalar();
                    int? newId = result as int?;

                    if (newId == null)
                    {
                        throw new InvalidOperationException("Failed to insert record or retrieve the new ID.");
                    }

                    return newId.Value;
                }
            }
        }

        public bool DeleteDiscount(int id)
        {
            string deleteSql = @"DELETE FROM discounts WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public Discount? GetDiscountById(int id)
        {
            string selectSql = @"SELECT id, name, description, discountpercent, active WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var discount = new Discount
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Description = reader.GetString(reader.GetOrdinal("description")),
                                DiscountPercent = reader.GetDecimal(reader.GetOrdinal("discountpercent")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("active"))
                            };

                            return discount;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public bool UpdateDiscount(Discount discount)
        {
            if (discount == null) throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");

            string updateSql = @"UPDATE discounts
                                SET name = @Name,
                                    description = @Description,
                                    discountpercent = @Discountpercent,
                                    active = @Active
                                WHERE id = @Id;";
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", discount.Name);
                    command.Parameters.AddWithValue("@Description", discount.Description);
                    command.Parameters.AddWithValue("@Discountpercent", discount.DiscountPercent);
                    command.Parameters.AddWithValue("@Active", discount.IsActive);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
