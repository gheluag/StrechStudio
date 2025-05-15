drop database if exists stretching;
create database if not exists stretching;

use stretching;

-- создание таблицы "Роль клиента"
CREATE TABLE role_clients (
    id_role INT AUTO_INCREMENT PRIMARY KEY,
    name_role VARCHAR(50) NOT NULL,
    UNIQUE INDEX idx_name_role (name_role)  -- Добавлен уникальный индекс
);

-- создание таблицы "Залы"
CREATE TABLE halls (
    id_hall INT AUTO_INCREMENT PRIMARY KEY,
    name_hall VARCHAR(100) NOT NULL,
    description_hall TEXT NOT NULL,
    capacity INT NOT NULL CHECK (capacity > 0),  -- Добавлена проверка
    INDEX idx_hall_name (name_hall)  -- Добавлен индекс
);

-- создание таблицы "Тренера"
CREATE TABLE trainers (
    trainer_id INT AUTO_INCREMENT PRIMARY KEY,
    last_name VARCHAR(50) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50) NOT NULL,
    date_of_birth DATE NOT NULL,
    work_features TEXT NOT NULL,
    INDEX idx_trainer_name (last_name, first_name)  -- Добавлен составной индекс
);

-- создание таблицы "Направления"
CREATE TABLE directions (
    direction_id INT AUTO_INCREMENT PRIMARY KEY,
    name_d VARCHAR(100) NOT NULL,
    description_d TEXT NOT NULL,
    contraindications TEXT NOT NULL,
    class_features TEXT NOT NULL,
    UNIQUE INDEX idx_direction_name (name_d)  -- Уникальный индекс
);

-- создание таблицы "Абонементы"
CREATE TABLE subscriptions (
    subscription_id INT AUTO_INCREMENT PRIMARY KEY,
    subscription_name VARCHAR(100) NOT NULL,
    description_s TEXT NOT NULL,
    initial_discount DECIMAL(5, 2) NOT NULL CHECK (initial_discount >= 0 AND initial_discount <= 100),
    final_discount DECIMAL(5, 2) NOT NULL CHECK (final_discount >= 0 AND final_discount <= 100),
    full_price DECIMAL(10, 2) NOT NULL CHECK (full_price > 0),
    duration_months INT NOT NULL CHECK (duration_months >= 0),
    INDEX idx_subscription_name (subscription_name)  -- Добавлен индекс
);

-- создание таблицы "Роль администратора"
CREATE TABLE role_admin (
    role_admin_id INT AUTO_INCREMENT PRIMARY KEY,
    name_role_admin VARCHAR(50) NOT NULL,
    description_admin TEXT NOT NULL,
    UNIQUE INDEX idx_role_admin_name (name_role_admin)  -- Уникальный индекс
);

-- создание таблицы "Администратор"
CREATE TABLE administrators (
    admin_id INT AUTO_INCREMENT PRIMARY KEY,
    last_name VARCHAR(50) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50) NOT NULL,
    date_of_birth DATE NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL,
    role_admin INT NOT NULL,
    FOREIGN KEY(role_admin) REFERENCES role_admin(role_admin_id),
    login VARCHAR(20) NOT NULL,
    pasw_admin VARCHAR(255) NOT NULL,  -- Увеличена длина для хэша пароля
    UNIQUE INDEX idx_admin_login (login),  -- Уникальный индекс
    UNIQUE INDEX idx_admin_email (email),  -- Уникальный индекс
    INDEX idx_admin_name (last_name, first_name)  -- Составной индекс
);

-- создание таблицы "Методы оплаты"
CREATE TABLE payment_method (
    id_method INT AUTO_INCREMENT PRIMARY KEY,
    name_method VARCHAR(50) NOT NULL,
    UNIQUE INDEX idx_payment_method (name_method)  -- Уникальный индекс
);

-- создание таблицы "Платежи"
CREATE TABLE payments (
    payment_id INT AUTO_INCREMENT PRIMARY KEY,
    total_amount DECIMAL(10, 2) NOT NULL CHECK (total_amount > 0),
    amount_paid DECIMAL(10, 2) NOT NULL CHECK (amount_paid >= 0),
    amount_remaining DECIMAL(10, 2) NOT NULL CHECK (amount_remaining >= 0),
    last_payment_date DATE NOT NULL,
    promised_payment_date DATE,
    payment_method INT NOT NULL,
    FOREIGN KEY (payment_method) REFERENCES payment_method(id_method),
    comment_pay TEXT,
    INDEX idx_payment_date (last_payment_date),  -- Индекс по дате
    INDEX idx_payment_amount (total_amount)  -- Индекс по сумме
);

-- создание таблицы "Клиенты"
CREATE TABLE clients (
    id_client INT AUTO_INCREMENT PRIMARY KEY,
    last_name VARCHAR(50) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    middle_name VARCHAR(50) NOT NULL,
    date_of_birth DATE NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    email VARCHAR(100) NOT NULL,
    health_conditions TEXT,
    name_role INT NOT NULL,
    FOREIGN KEY (name_role) REFERENCES role_clients(id_role),
    UNIQUE INDEX idx_client_email (email),  -- Уникальный индекс
    INDEX idx_client_phone (phone_number),  -- Индекс по телефону
    INDEX idx_client_name (last_name, first_name),  -- Составной индекс
    INDEX idx_client_role (name_role)  -- Индекс по роли
);

-- создание таблицы "Абонемент клиента"
CREATE TABLE client_subscriptions (
    client_subscription_id INT AUTO_INCREMENT PRIMARY KEY,
    subscription_id int not null,
    FOREIGN KEY (subscription_id) REFERENCES subscriptions(subscription_id),
    payment_id INT NOT NULL,
    total_classes INT NOT NULL CHECK (total_classes >= 0),
    remaining_classes INT NOT NULL CHECK (remaining_classes >= 0),
    subscription_start_date DATE NOT NULL,
    subscription_end_date DATE NOT NULL,
    FOREIGN KEY (payment_id) REFERENCES payments(payment_id),
    assigned_admin INT NOT NULL,
    FOREIGN KEY (assigned_admin) REFERENCES administrators(admin_id),
    client_id INT NOT NULL,
    FOREIGN KEY (client_id) REFERENCES clients(id_client),
    INDEX idx_subscription_dates (subscription_start_date, subscription_end_date),
    INDEX idx_subscription_client (client_id),
    INDEX idx_subscription_admin (assigned_admin),
    
    -- Проверка дат (теперь без ссылки на другие столбцы)
    CONSTRAINT chk_dates CHECK (subscription_end_date >= subscription_start_date)
);

-- создание таблицы "Статус занятия"
CREATE TABLE class_status (
    status_id INT AUTO_INCREMENT PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL,
    UNIQUE INDEX idx_status_name (status_name)  -- Уникальный индекс
);

-- создание таблицы "Занятия"
CREATE TABLE classes (
    class_id INT AUTO_INCREMENT PRIMARY KEY,
    direction_id INT NOT NULL,
    trainer_id INT NOT NULL,
    class_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    FOREIGN KEY (direction_id) REFERENCES directions(direction_id),
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id),
    INDEX idx_class_date (class_date),  -- Индекс по дате
    INDEX idx_class_trainer (trainer_id),  -- Индекс по тренеру
    INDEX idx_class_direction (direction_id)  -- Индекс по направлению
);

-- создание таблицы "Записи"
CREATE TABLE bookings (
    booking_id INT AUTO_INCREMENT PRIMARY KEY,
    class_id INT NOT NULL,
    client_id INT NOT NULL,
    booking_date DATE NOT NULL,
    status_id INT NOT NULL,
    FOREIGN KEY (class_id) REFERENCES classes(class_id),
    FOREIGN KEY (client_id) REFERENCES clients(id_client),
    FOREIGN KEY (status_id) REFERENCES class_status(status_id),
    id_hall INT NOT NULL,
    FOREIGN KEY (id_hall) REFERENCES halls(id_hall),
    INDEX idx_booking_class (class_id),  -- Индекс по занятию
    INDEX idx_booking_client (client_id),  -- Индекс по клиенту
    INDEX idx_booking_date (booking_date),  -- Индекс по дате записи
    INDEX idx_booking_hall (id_hall)  -- Индекс по залу
);