using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STRETCHING.Entities
{
    public class ClientRepository
    {
        private readonly DataBase _database;

        public ClientRepository(DataBase database)
        {
            _database = database;
        }

        // Добавление нового клиента
        public int AddClient(STRETCHING.Entities.Clients client, int subscriptionId, int adminId, Payment payment)
        {
            _database.OpenConnection();

            using (var transaction = _database.GetConnection().BeginTransaction())
            {
                try
                {
                    // 1. Вставляем платеж
                    var paymentId = InsertPayment(payment);

                    // 2. Вставляем клиента
                    var clientId = InsertClient(client);

                    // 3. Создаем абонемент клиента
                    InsertClientSubscription(clientId, subscriptionId, paymentId, adminId);

                    transaction.Commit();
                    return clientId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    _database.CloseConnection();
                }
            }
        }

        private int InsertClient(STRETCHING.Entities.Clients client)
        {
            var query = @"INSERT INTO clients (last_name, first_name, middle_name, date_of_birth, 
                    phone_number, email, health_conditions, name_role)
                  VALUES (@lastName, @firstName, @middleName, @birthDate, @phone, @email, @health, @role);
                  SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@lastName", client.LastName);
                cmd.Parameters.AddWithValue("@firstName", client.FirstName);
                cmd.Parameters.AddWithValue("@middleName", client.MiddleName);
                cmd.Parameters.Add("@birthDate", MySqlDbType.Date).Value = client.DateOfBirth.ToString("yyyy-MM-dd");
                cmd.Parameters.AddWithValue("@phone", client.PhoneNumber);
                cmd.Parameters.AddWithValue("@email", client.Email);
                cmd.Parameters.AddWithValue("@health", client.HealthConditions ?? string.Empty);
                cmd.Parameters.AddWithValue("@role", client.RoleId);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        private int InsertPayment(Payment payment)
        {
            var query = @"INSERT INTO payments (total_amount, amount_paid, amount_remaining, last_payment_date, 
                        promised_payment_date, payment_method, comment_pay)
                      VALUES (@total, @paid, @remaining, @lastDate, @promisedDate, @method, @comment);
                      SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@total", payment.TotalAmount);
                cmd.Parameters.AddWithValue("@paid", payment.AmountPaid);
                cmd.Parameters.AddWithValue("@remaining", payment.AmountRemaining);
                cmd.Parameters.AddWithValue("@lastDate", payment.LastPaymentDate);
                cmd.Parameters.AddWithValue("@promisedDate", payment.PromisedPaymentDate.HasValue ?
                    (object)payment.PromisedPaymentDate.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@method", payment.PaymentMethodId);
                cmd.Parameters.AddWithValue("@comment", payment.Comment ?? string.Empty);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private void InsertClientSubscription(int clientId, int subscriptionId, int paymentId, int adminId)
        {
            // Получаем данные об абонементе
            var subscription = GetSubscriptionById(subscriptionId);
            var startDate = DateTime.Today;
            var endDate = startDate.AddMonths(subscription.DurationMonths);

            var query = @"INSERT INTO client_subscriptions 
                        (subscription_id, payment_id, total_classes, remaining_classes, 
                         subscription_start_date, subscription_end_date, assigned_admin, client_id)
                      VALUES (@subId, @payId, @totalClasses, @remainingClasses, @startDate, @endDate, @adminId, @clientId)";

            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@subId", subscriptionId);
                cmd.Parameters.AddWithValue("@payId", paymentId);
                cmd.Parameters.AddWithValue("@totalClasses", subscription.TotalClasses);
                cmd.Parameters.AddWithValue("@remainingClasses", subscription.TotalClasses);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);
                cmd.Parameters.AddWithValue("@adminId", adminId);
                cmd.Parameters.AddWithValue("@clientId", clientId);

                cmd.ExecuteNonQuery();
            }
        }

        public List<ClientRole> GetClientRoles()
        {
            var query = "SELECT id_role, name_role FROM role_clients";
            var roles = new List<ClientRole>();

            _database.OpenConnection();
            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new ClientRole
                        {
                            Id = reader.GetInt32("id_role"),
                            Name = reader.GetString("name_role")
                        });
                    }
                }
            }
            _database.CloseConnection();

            return roles;
        }

        public List<Subscription> GetSubscriptions()
        {
            var query = "SELECT subscription_id, subscription_name, full_price, duration_months FROM subscriptions";
            var subscriptions = new List<Subscription>();

            _database.OpenConnection();
            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subscriptions.Add(new Subscription
                        {
                            Id = reader.GetInt32("subscription_id"),
                            Name = reader.GetString("subscription_name"),
                            FullPrice = reader.GetDecimal("full_price"),
                            DurationMonths = reader.GetInt32("duration_months")
                        });
                    }
                }
            }
            _database.CloseConnection();

            return subscriptions;
        }

        private Subscription GetSubscriptionById(int id)
        {
            var query = "SELECT * FROM subscriptions WHERE subscription_id = @id";

            _database.OpenConnection();
            using (var cmd = new MySqlCommand(query, _database.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Subscription
                        {
                            Id = reader.GetInt32("subscription_id"),
                            Name = reader.GetString("subscription_name"),
                            FullPrice = reader.GetDecimal("full_price"),
                            DurationMonths = reader.GetInt32("duration_months"),
                            TotalClasses = CalculateTotalClasses(reader.GetInt32("duration_months"))
                        };
                    }
                }
            }
            _database.CloseConnection();

            throw new Exception("Абонемент не найден");
        }

        private int CalculateTotalClasses(int durationMonths)
        {
            // Предположим, что в месяц в среднем 8 занятий
            return durationMonths * 8;
        }
    }
}
