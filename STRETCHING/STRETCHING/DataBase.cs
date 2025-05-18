using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public MySqlConnection GetConnection() => connection;

        public void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }

        public void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

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
            finally
            {
                connection.Close();
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

            finally
            {
                connection.Close();
            }

            return clientslst;
        }


        public ObservableCollection<TaskItem> GetTasksForAdmin(int adminId)
        {
            ObservableCollection<TaskItem> tasks = new();
            try
            {
                connection.Open();

                string query = "SELECT task_id, title, description, task_date, deadline, is_completed FROM tasks WHERE admin_id = @adminId ORDER BY deadline";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@adminId", adminId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(new TaskItem
                    {
                        Id = reader.GetInt32("task_id"),
                        Title = reader.GetString("title"),
                        Description = reader.GetString("description"),
                        TaskDate = reader.GetDateTime("task_date"),
                        Deadline = reader.GetDateTime("deadline"),
                        IsCompleted = reader.GetBoolean("is_completed")
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }
            


            return tasks;
        }

        public void UpdateTaskCompletion(int taskId, bool isCompleted)
        {
            try
            {
                connection.Open();

                string query = "UPDATE tasks SET is_completed = @isCompleted WHERE task_id = @id";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@isCompleted", isCompleted);
                cmd.Parameters.AddWithValue("@id", taskId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }

        }
        public void GenerateTasksForAdmin(int adminId)
        {
            try
            {
                connection.Open();
                using var cmd = new MySqlCommand("GenerateTasksForAdmin", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@admin_id", adminId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }

           
        }


        public List<TaskModel> GetTasksForAdminMain(int adminId)
        {
            var tasks = new List<TaskModel>();

            try
            {
                connection.Open();

                string query = @"
                    SELECT task_id, title, description, task_date, deadline, is_completed
                    FROM tasks
                    WHERE admin_id = @adminId
                      AND (is_completed = 0 OR deadline > CURDATE())
                    ORDER BY deadline ASC";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminId", adminId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskModel
                            {
                                TaskId = reader.GetInt32("task_id"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                TaskDate = reader.GetDateTime("task_date"),
                                Deadline = reader.GetDateTime("deadline"),
                                IsCompleted = reader.GetBoolean("is_completed")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }

            return tasks;
        }



        public List<ScheduleItem> GetTodaySchedule()
        {
            List<ScheduleItem> schedule = new List<ScheduleItem>();

            string query = @"
        SELECT 
            c.class_id,
            c.class_date,
            c.start_time,
            c.end_time,
            d.name_d as direction,
            t.last_name,
            t.first_name,
            h.name_hall as hall
        FROM classes c
        JOIN directions d ON c.direction_id = d.direction_id
        JOIN trainers t ON c.trainer_id = t.trainer_id
        JOIN bookings b ON c.class_id = b.class_id
        JOIN halls h ON b.id_hall = h.id_hall
        WHERE c.class_date = CURDATE()
        GROUP BY c.class_id, c.class_date, c.start_time, c.end_time, d.name_d, t.last_name, t.first_name, h.name_hall
        ORDER BY c.start_time;";

            try
            {
                OpenConnection();
                var cmd = new MySqlCommand(query, GetConnection());
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var item = new ScheduleItem
                    {
                        ClassDate = reader.GetDateTime("class_date"),
                        StartTime = TimeSpan.Parse(reader["start_time"].ToString()),
                        EndTime = TimeSpan.Parse(reader["end_time"].ToString()),
                        Direction = reader["direction"].ToString(),
                        Trainer = $"{reader["last_name"]} {reader["first_name"].ToString()[0]}.",
                        Hall = reader["hall"].ToString()
                    };
                    schedule.Add(item);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                CloseConnection();
            }

            return schedule;
        }

        // Получение списка всех специализаций
        public List<TrainerSpecialization> GetAllSpecializations()
        {
            List<TrainerSpecialization> specializations = new List<TrainerSpecialization>();

            string query = "SELECT * FROM trainer_specializations ORDER BY specname";

            try
            {
                OpenConnection();
                var cmd = new MySqlCommand(query, GetConnection());
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    specializations.Add(new TrainerSpecialization
                    {
                        SpecializationId = reader.GetInt32("specialization_id"),
                        Name = reader.GetString("specname"),
                        Description = reader.IsDBNull(reader.GetOrdinal("description"))
                            ? null : reader.GetString("description")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                CloseConnection();
            }

            return specializations;
        }

        // Получение всех тренеров с их специализациями
        public List<Trainer> GetAllTrainers()
        {
            List<Trainer> trainers = new List<Trainer>();

            string query = @"
        SELECT 
            t.trainer_id,
            t.last_name,
            t.first_name,
            t.middle_name,
            t.date_of_birth,
            GROUP_CONCAT(ts.specname SEPARATOR ', ') AS specializations,
            GROUP_CONCAT(ts.specialization_id SEPARATOR ',') AS specialization_ids
        FROM 
            trainers t
        LEFT JOIN 
            trainer_specialization_mapping tsm ON t.trainer_id = tsm.trainer_id
        LEFT JOIN 
            trainer_specializations ts ON tsm.specialization_id = ts.specialization_id
        GROUP BY 
            t.trainer_id
        ORDER BY 
            t.last_name, t.first_name";

            try
            {
                OpenConnection();
                var cmd = new MySqlCommand(query, GetConnection());
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var trainer = new Trainer
                    {
                        TrainerId = reader.GetInt32("trainer_id"),
                        LastName = reader.GetString("last_name"),
                        FirstName = reader.GetString("first_name"),
                        MiddleName = reader.GetString("middle_name")
                    };

                    if (!reader.IsDBNull(reader.GetOrdinal("specializations")))
                    {
                        trainer.SpecializationNames = reader.GetString("specializations");
                        var ids = reader.GetString("specialization_ids").Split(',');
                        trainer.SpecializationIds = ids.Where(id => !string.IsNullOrEmpty(id))
                                                      .Select(int.Parse).ToList();
                    }

                    trainers.Add(trainer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                CloseConnection();
            }

            return trainers;
        }

        public int AddTrainer(Trainer trainer)
        {
            string query = @"INSERT INTO trainers 
                    (last_name, first_name, middle_name, date_of_birth) 
                    VALUES 
                    (@lastName, @firstName, @middleName, @dateOfBirth);
                    SELECT LAST_INSERT_ID();";

            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                var cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@lastName", trainer.LastName);
                cmd.Parameters.AddWithValue("@firstName", trainer.FirstName);
                cmd.Parameters.AddWithValue("@middleName", trainer.MiddleName);
                cmd.Parameters.AddWithValue("@dateOfBirth", trainer.DateOfBirth);

                int trainerId = Convert.ToInt32(cmd.ExecuteScalar());

                // Добавляем специализации
                if (trainer.SpecializationIds != null && trainer.SpecializationIds.Any())
                {
                    AddTrainerSpecializations(trainerId, trainer.SpecializationIds, transaction); // Передаем transaction
                }

                transaction.Commit();
                return trainerId;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine(ex);
                return -1;
            }
            finally
            {
                CloseConnection();
            }
        }

        private void AddTrainerSpecializations(int trainerId, List<int> specializationIds, MySqlTransaction transaction)
        {
            string query = "INSERT INTO trainer_specialization_mapping (trainer_id, specialization_id) VALUES ";

            var values = new List<string>();
            foreach (var id in specializationIds)
            {
                values.Add($"({trainerId}, {id})");
            }

            query += string.Join(",", values);

            try
            {
                var cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        // Обновление данных тренера
        public bool UpdateTrainer(Trainer trainer)
        {
            MySqlTransaction transaction = null;
            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                Console.WriteLine($"Updating trainer {trainer.TrainerId}");
                Console.WriteLine($"Specializations: {string.Join(",", trainer.SpecializationIds ?? new List<int>())}");

                // 1. Обновляем основные данные тренера
                string updateQuery = @"UPDATE trainers SET 
                             last_name = @lastName, 
                             first_name = @firstName, 
                             middle_name = @middleName, 
                             date_of_birth = @dateOfBirth 
                             WHERE trainer_id = @trainerId";

                var cmd = new MySqlCommand(updateQuery, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@lastName", trainer.LastName);
                cmd.Parameters.AddWithValue("@firstName", trainer.FirstName);
                cmd.Parameters.AddWithValue("@middleName", trainer.MiddleName);
                cmd.Parameters.AddWithValue("@dateOfBirth", trainer.DateOfBirth);
                cmd.Parameters.AddWithValue("@trainerId", trainer.TrainerId);
                cmd.ExecuteNonQuery();

                // 2. Удаляем все существующие специализации тренера
                string deleteQuery = "DELETE FROM trainer_specialization_mapping WHERE trainer_id = @trainerId";
                cmd = new MySqlCommand(deleteQuery, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@trainerId", trainer.TrainerId);
                cmd.ExecuteNonQuery();

                // 3. Добавляем новые специализации
                if (trainer.SpecializationIds != null && trainer.SpecializationIds.Any())
                {
                    string insertQuery = "INSERT INTO trainer_specialization_mapping (trainer_id, specialization_id) VALUES ";
                    var values = trainer.SpecializationIds.Select(id => $"({trainer.TrainerId}, {id})");
                    insertQuery += string.Join(",", values);

                    cmd = new MySqlCommand(insertQuery, GetConnection(), transaction);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Ошибка при обновлении тренера: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Удаление тренера
        public bool DeleteTrainer(int trainerId)
        {
            string query = "DELETE FROM trainers WHERE trainer_id = @trainerId";
            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                // Сначала удаляем связи со специализациями
                var cmd = new MySqlCommand("DELETE FROM trainer_specialization_mapping WHERE trainer_id = @trainerId",
                                          GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@trainerId", trainerId);
                cmd.ExecuteNonQuery();

                // Затем удаляем самого тренера
                cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@trainerId", trainerId);
                int affectedRows = cmd.ExecuteNonQuery();

                transaction.Commit();
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine(ex);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

    }


    }

