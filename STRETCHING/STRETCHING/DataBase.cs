using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using STRETCHING.Entities;

namespace STRETCHING
{
    public class DataBase
    {
        MySqlConnection connection = new("user=root; password=1234; server=localhost; database=stretching");

        public Administrators Authorize(string login, string password)
        {
            Administrators admin = null;

            using (connection)
            {
                try
                {
                    connection.Open();

                    string query = @"
                    SELECT admin_id, login, pasw_admin, last_name, first_name, middle_name, role_admin
                    FROM administrators
                    WHERE login = @login AND pasw_admin = @password
                    LIMIT 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                admin = new Administrators
                                {
                                    Id = reader.GetInt32("admin_id"),
                                    Login = reader.GetString("login"),
                                    Password = reader.GetString("pasw_admin"),
                                    Last_name = reader.GetString("last_name"),
                                    First_name = reader.GetString("first_name"),
                                    Middle_name = reader.GetString("middle_name"),
                                    Role = reader.GetInt32("role_admin")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // лог или сообщение об ошибке
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
                }
            }

            return admin;
        }


        public List<Clients> GetClientsByAdmin(int adminId)
        {
            List<Clients> clientslst = new();

            string query = "SELECT c.id_client, c.last_name, c.first_name, c.middle_name, " +
                "c.phone_number, c.email, " +
                "c.date_of_birth, " +
                "rc.name_role AS role_name, " + 
                "s.subscription_name, " +
                "c.health_conditions, " +
                "cs.subscription_start_date, " +
                "cs.subscription_end_date, " +
                "c.name_role AS role_id " + 
                "FROM client_subscriptions cs " +
                "JOIN clients c ON cs.client_id = c.id_client " +
                "JOIN subscriptions s ON cs.subscription_id = s.subscription_id " +
                "JOIN administrators a ON cs.assigned_admin = a.admin_id " +
                "JOIN role_clients rc ON c.name_role = rc.id_role " +
                "WHERE cs.assigned_admin = @adminId";

            try
            {
                connection.Open();

                MySqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@adminId", adminId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var client = new Clients
                        {
                            IdClient = reader.GetInt32("id_client"),
                            LastName = reader.GetString("last_name"),
                            FirstName = reader.GetString("first_name"),
                            MiddleName = reader.GetString("middle_name"),
                            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime("date_of_birth")),
                            PhoneNumber = reader.GetString("phone_number"),
                            Email = reader.GetString("email"),
                            HealthConditions = reader.IsDBNull(reader.GetOrdinal("health_conditions"))
                                ? null
                                : reader.GetString("health_conditions"),
                            RoleId = reader.GetInt32("role_id"), 
                            RoleName = reader.GetString("role_name") 
                        };

                        clientslst.Add(client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            

            return clientslst;
        }


        public List<Clients> SearchClients(string txt, int adminId)
        {
            List<Clients> clientslst = new();

            string query = "SELECT c.id_client, c.last_name, c.first_name, c.middle_name, " +
                "c.phone_number, c.email, " +
                "c.date_of_birth, " +
                "rc.name_role AS role_name, " +
                "s.subscription_name, " +
                "c.health_conditions, " +
                "cs.subscription_start_date, " +
                "cs.subscription_end_date, " +
                "c.name_role AS role_id " +
                "FROM client_subscriptions cs " +
                "JOIN clients c ON cs.client_id = c.id_client " +
                "JOIN subscriptions s ON cs.subscription_id = s.subscription_id " +
                "JOIN administrators a ON cs.assigned_admin = a.admin_id " +
                "JOIN role_clients rc ON c.name_role = rc.id_role " +
                "WHERE (c.last_name like @txt " +
                "or c.first_name like @txt " +
                "or c.middle_name like @txt) " +
                "and cs.assigned_admin = @adminId";

            try
            {
                connection.Open();

                MySqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@txt", "%" + txt + "%");
                cmd.Parameters.AddWithValue("@adminId", adminId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var client = new Clients
                        {
                            IdClient = reader.GetInt32("id_client"),
                            LastName = reader.GetString("last_name"),
                            FirstName = reader.GetString("first_name"),
                            MiddleName = reader.GetString("middle_name"),
                            DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime("date_of_birth")),
                            PhoneNumber = reader.GetString("phone_number"),
                            Email = reader.GetString("email"),
                            HealthConditions = reader.IsDBNull(reader.GetOrdinal("health_conditions"))
                                ? null
                                : reader.GetString("health_conditions"),
                            RoleId = reader.GetInt32("role_id"),
                            RoleName = reader.GetString("role_name")
                        };

                        clientslst.Add(client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            return clientslst;
        }


    }


    }

