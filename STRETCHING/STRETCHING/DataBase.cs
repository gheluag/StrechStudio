using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        public bool AddTask(TaskModel task)
        {
            try
            {
                OpenConnection();
                string query = @"
        INSERT INTO tasks 
            (title, description, task_date, deadline, admin_id, client_id, is_completed) 
        VALUES 
            (@title, @description, @taskDate, @deadline, @adminId, @clientId, @isCompleted)";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection());
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@description", task.Description);
                cmd.Parameters.AddWithValue("@taskDate", DateTime.Today);
                cmd.Parameters.AddWithValue("@deadline", task.Deadline);
                cmd.Parameters.AddWithValue("@adminId", task.AdminId);
                cmd.Parameters.AddWithValue("@clientId", task.ClientId.HasValue ? (object)task.ClientId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@isCompleted", task.IsCompleted);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении задачи: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        public bool UpdateTask(TaskModel task)
        {
            try
            {
                OpenConnection();
                string query = @"
        UPDATE tasks 
        SET 
            title = @title,
            description = @description,
            deadline = @deadline,
            is_completed = @isCompleted
        WHERE 
            task_id = @taskId";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection());
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@description", task.Description);
                cmd.Parameters.AddWithValue("@deadline", task.Deadline);
                cmd.Parameters.AddWithValue("@isCompleted", task.IsCompleted);
                cmd.Parameters.AddWithValue("@taskId", task.TaskId);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении задачи: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
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


        public List<Trainer> GetSearchTrainers(string searchText)
        {
            List<Trainer> trainers = new List<Trainer>();

            try
            {
                OpenConnection();
                string query = @"
            SELECT t.trainer_id, t.last_name, t.first_name, t.middle_name, t.date_of_birth,
                   GROUP_CONCAT(ts.specname SEPARATOR ', ') as specializations
            FROM trainers t
            LEFT JOIN trainer_specialization_mapping tsm ON t.trainer_id = tsm.trainer_id
            LEFT JOIN trainer_specializations ts ON tsm.specialization_id = ts.specialization_id
            WHERE CONCAT(t.last_name, ' ', t.first_name, ' ', t.middle_name) LIKE CONCAT('%', @search, '%') OR
                  ts.specname LIKE CONCAT('%', @search, '%')
            GROUP BY t.trainer_id";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection());
                // Правильный способ добавления параметра для LIKE
                cmd.Parameters.AddWithValue("@search", searchText);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        trainers.Add(new Trainer
                        {
                            TrainerId = reader.GetInt32("trainer_id"),
                            LastName = reader.GetString("last_name"),
                            FirstName = reader.GetString("first_name"),
                            MiddleName = reader.GetString("middle_name"),
                            DateOfBirth = reader.GetDateTime("date_of_birth"),
                            SpecializationNames = reader.IsDBNull(reader.GetOrdinal("specializations")) ?
                                              "Нет специализаций" : reader.GetString("specializations")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске тренеров: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return trainers;
        }

        public List<ClassScheduleItem> GetClassSchedule()
        {
            List<ClassScheduleItem> schedule = new List<ClassScheduleItem>();

            try
            {
                OpenConnection();
                string query = @"
        SELECT 
            c.class_id,
            c.class_date,
            c.start_time,
            c.end_time,
            d.name_d AS direction_name,
            CONCAT(t.last_name, ' ', t.first_name, ' ', t.middle_name) AS trainer_full_name,
            h.name_hall AS hall_name,
            COUNT(b.booking_id) AS booked_clients_count
        FROM 
            classes c
        JOIN 
            directions d ON c.direction_id = d.direction_id
        JOIN 
            trainers t ON c.trainer_id = t.trainer_id
        LEFT JOIN 
            bookings b ON c.class_id = b.class_id
        JOIN
            halls h ON b.id_hall = h.id_hall
        GROUP BY 
            c.class_id, c.class_date, c.start_time, c.end_time, 
            d.name_d, t.last_name, t.first_name, t.middle_name, h.name_hall
        ORDER BY 
            c.class_date, c.start_time";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection());

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add(new ClassScheduleItem
                        {
                            ClassId = reader.GetInt32("class_id"),
                            ClassDate = reader.GetDateTime("class_date"),
                            StartTime = reader.GetTimeSpan("start_time"),
                            EndTime = reader.GetTimeSpan("end_time"),
                            DirectionName = reader.GetString("direction_name"),
                            TrainerFullName = reader.GetString("trainer_full_name"),
                            HallName = reader.GetString("hall_name"),
                            BookedClientsCount = reader.GetInt32("booked_clients_count")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке расписания: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return schedule;
        }

        public bool DeleteClass(int classId)
        {
            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                // Сначала удаляем записи на это занятие
                var cmd = new MySqlCommand("DELETE FROM bookings WHERE class_id = @classId",
                                          GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@classId", classId);
                cmd.ExecuteNonQuery();

                // Затем удаляем само занятие
                cmd = new MySqlCommand("DELETE FROM classes WHERE class_id = @classId",
                                     GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@classId", classId);
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

        public List<TrainerSpecialization> GetSpecializations()
        {
            List<TrainerSpecialization> specializations = new List<TrainerSpecialization>();

            try
            {
                OpenConnection();
                string query = "SELECT specialization_id, specname, description FROM trainer_specializations";
                MySqlCommand cmd = new MySqlCommand(query, GetConnection());

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        specializations.Add(new TrainerSpecialization
                        {
                            SpecializationId = reader.GetInt32("specialization_id"),
                            Name = reader.GetString("specname"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ?
                                          string.Empty : reader.GetString("description")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке специализаций: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return specializations;
        }

        public int AddTrainer(Trainer trainer)
        {
            int trainerId = -1;
            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                // Добавляем тренера
                string query = @"INSERT INTO trainers 
                               (last_name, first_name, middle_name, date_of_birth) 
                               VALUES (@lastName, @firstName, @middleName, @dateOfBirth);
                               SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@lastName", trainer.LastName);
                cmd.Parameters.AddWithValue("@firstName", trainer.FirstName);
                cmd.Parameters.AddWithValue("@middleName", trainer.MiddleName);
                cmd.Parameters.AddWithValue("@dateOfBirth", trainer.DateOfBirth);

                trainerId = Convert.ToInt32(cmd.ExecuteScalar());

                // Добавляем специализации
                if (trainer.SpecializationIds != null && trainer.SpecializationIds.Count > 0)
                {
                    foreach (int specId in trainer.SpecializationIds)
                    {
                        query = "INSERT INTO trainer_specialization_mapping (trainer_id, specialization_id) VALUES (@trainerId, @specId)";
                        cmd = new MySqlCommand(query, GetConnection(), transaction);
                        cmd.Parameters.AddWithValue("@trainerId", trainerId);
                        cmd.Parameters.AddWithValue("@specId", specId);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return trainerId;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"Ошибка при добавлении тренера: {ex.Message}");
                return -1;
            }
            finally
            {
                CloseConnection();
            }
        }



        public List<Direction> GetAllDirections()
        {
            List<Direction> directions = new List<Direction>();

            try
            {
                OpenConnection();
                string query = "SELECT direction_id, name_d FROM directions ORDER BY name_d";
                MySqlCommand cmd = new MySqlCommand(query, GetConnection());

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        directions.Add(new Direction
                        {
                            DirectionId = reader.GetInt32("direction_id"),
                            Name_d = reader.GetString("name_d")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке направлений: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return directions;
        }

        public List<Hall> GetAllHalls()
        {
            List<Hall> halls = new List<Hall>();

            try
            {
                OpenConnection();
                string query = "SELECT id_hall, name_hall FROM halls ORDER BY name_hall";
                MySqlCommand cmd = new MySqlCommand(query, GetConnection());

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        halls.Add(new Hall
                        {
                            HallId = reader.GetInt32("id_hall"),
                            NameHall = reader.GetString("name_hall")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке залов: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return halls;
        }

        public Class GetClassById(int classId)
        {
            Class classObj = null;

            try
            {
                OpenConnection();
                string query = @"
        SELECT 
            class_id, 
            class_date, 
            start_time, 
            end_time, 
            direction_id, 
            trainer_id,
            (SELECT id_hall FROM bookings WHERE class_id = @classId LIMIT 1) as hall_id
        FROM 
            classes 
        WHERE 
            class_id = @classId";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection());
                cmd.Parameters.AddWithValue("@classId", classId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        classObj = new Class
                        {
                            ClassId = reader.GetInt32("class_id"),
                            ClassDate = reader.GetDateTime("class_date"),
                            StartTime = reader.GetTimeSpan("start_time"),
                            EndTime = reader.GetTimeSpan("end_time"),
                            DirectionId = reader.GetInt32("direction_id"),
                            TrainerId = reader.GetInt32("trainer_id"),
                            HallId = reader.IsDBNull(reader.GetOrdinal("hall_id")) ? 0 : reader.GetInt32("hall_id")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке занятия: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return classObj;
        }

        public bool AddClass(Class newClass)
        {
            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                // 1. Добавляем занятие в таблицу classes
                string query = @"
        INSERT INTO classes 
            (direction_id, trainer_id, class_date, start_time, end_time) 
        VALUES 
            (@directionId, @trainerId, @classDate, @startTime, @endTime);
        SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@directionId", newClass.DirectionId);
                cmd.Parameters.AddWithValue("@trainerId", newClass.TrainerId);
                cmd.Parameters.AddWithValue("@classDate", newClass.ClassDate);
                cmd.Parameters.AddWithValue("@startTime", newClass.StartTime);
                cmd.Parameters.AddWithValue("@endTime", newClass.EndTime);

                int classId = Convert.ToInt32(cmd.ExecuteScalar());

                // 2. Находим существующего клиента (например, администратора или тестового клиента)
                int defaultClientId = GetDefaultClientId(transaction);

                // 3. Добавляем запись в bookings для зала
                query = @"
        INSERT INTO bookings 
            (class_id, client_id, booking_date, status_id, id_hall) 
        VALUES 
            (@classId, @clientId, @bookingDate, @statusId, @hallId)";

                cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@classId", classId);
                cmd.Parameters.AddWithValue("@clientId", defaultClientId); // Используем реальный client_id
                cmd.Parameters.AddWithValue("@bookingDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@statusId", 1); // Предполагаем, что статус 1 существует
                cmd.Parameters.AddWithValue("@hallId", newClass.HallId);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Ошибка при добавлении занятия: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        private int GetDefaultClientId(MySqlTransaction transaction)
        {
            // Попробуем найти первого клиента в системе
            string query = "SELECT id_client FROM clients LIMIT 1";
            MySqlCommand cmd = new MySqlCommand(query, GetConnection(), transaction);

            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                return Convert.ToInt32(result);
            }

            // Если клиентов нет, создадим тестового клиента
            query = @"
    INSERT INTO clients 
        (last_name, first_name, middle_name, date_of_birth, phone_number, email, name_role) 
    VALUES 
        ('Test', 'Client', 'System', '2000-01-01', '0000000000', 'test@system.com', 1);
    SELECT LAST_INSERT_ID();";

            cmd = new MySqlCommand(query, GetConnection(), transaction);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool UpdateClass(Class updatedClass)
        {
            MySqlTransaction transaction = null;

            try
            {
                OpenConnection();
                transaction = GetConnection().BeginTransaction();

                // Обновляем занятие
                string query = @"
        UPDATE classes 
        SET 
            direction_id = @directionId, 
            trainer_id = @trainerId, 
            class_date = @classDate, 
            start_time = @startTime, 
            end_time = @endTime
        WHERE 
            class_id = @classId";

                MySqlCommand cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@directionId", updatedClass.DirectionId);
                cmd.Parameters.AddWithValue("@trainerId", updatedClass.TrainerId);
                cmd.Parameters.AddWithValue("@classDate", updatedClass.ClassDate);
                cmd.Parameters.AddWithValue("@startTime", updatedClass.StartTime);
                cmd.Parameters.AddWithValue("@endTime", updatedClass.EndTime);
                cmd.Parameters.AddWithValue("@classId", updatedClass.ClassId);
                cmd.ExecuteNonQuery();

                // Обновляем зал в bookings
                query = "UPDATE bookings SET id_hall = @hallId WHERE class_id = @classId LIMIT 1";

                cmd = new MySqlCommand(query, GetConnection(), transaction);
                cmd.Parameters.AddWithValue("@hallId", updatedClass.HallId);
                cmd.Parameters.AddWithValue("@classId", updatedClass.ClassId);
                cmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Console.WriteLine($"Ошибка при обновлении занятия: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }
    }


}


    

