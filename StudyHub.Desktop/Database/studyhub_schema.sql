CREATE DATABASE IF NOT EXISTS studyhub_db;
USE studyhub_db;

-- 1. Users Table (Supports Single-Table Inheritance for Student and Administrator)
CREATE TABLE IF NOT EXISTS users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role ENUM('Student', 'Administrator') NOT NULL DEFAULT 'Student',
    course VARCHAR(100) NULL,
    year_level VARCHAR(50) NULL
);

-- 2. Resources Table
CREATE TABLE IF NOT EXISTS resources (
    resource_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id INT NOT NULL,
    title VARCHAR(150) NOT NULL,
    description TEXT NOT NULL,
    subject VARCHAR(100) NOT NULL,
    author_name VARCHAR(100) NOT NULL,
    resource_type ENUM('PDF', 'Link') NOT NULL,
    file_path_or_url VARCHAR(255) NOT NULL,
    date_uploaded DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 3. Comments Table
CREATE TABLE IF NOT EXISTS comments (
    comment_id INT AUTO_INCREMENT PRIMARY KEY,
    resource_id INT NOT NULL,
    student_id INT NOT NULL,
    content TEXT NOT NULL,
    date_posted DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (resource_id) REFERENCES resources(resource_id) ON DELETE CASCADE,
    FOREIGN KEY (student_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 4. Bookmarks Table (Resolves Many-to-Many between Students and Resources)
CREATE TABLE IF NOT EXISTS bookmarks (
    bookmark_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id INT NOT NULL,
    resource_id INT NOT NULL,
    date_bookmarked DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_bookmark (student_id, resource_id),
    FOREIGN KEY (student_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (resource_id) REFERENCES resources(resource_id) ON DELETE CASCADE
);

-- 5. Recently Viewed Table
CREATE TABLE IF NOT EXISTS recently_viewed (
    view_id INT AUTO_INCREMENT PRIMARY KEY,
    student_id INT NOT NULL,
    resource_id INT NOT NULL,
    viewed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_view (student_id, resource_id),
    FOREIGN KEY (student_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (resource_id) REFERENCES resources(resource_id) ON DELETE CASCADE
);

-- 6. Reports Table
CREATE TABLE IF NOT EXISTS reports (
    report_id INT AUTO_INCREMENT PRIMARY KEY,
    reporter_id INT NOT NULL,
    target_id INT NOT NULL,
    target_type ENUM('Resource', 'Comment') NOT NULL,
    reason TEXT NOT NULL,
    status ENUM('Pending', 'Resolved', 'Dismissed') NOT NULL DEFAULT 'Pending',
    date_reported DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (reporter_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Seed Default Accounts
INSERT IGNORE INTO users (user_id, full_name, email, password, role, course, year_level) VALUES
(1, 'System Admin', 'admin@studyhub.edu', 'admin123', 'Administrator', NULL, NULL),
(2, 'Juan Dela Cruz', 'juan@student.edu', 'pass123', 'Student', 'BSCS', '2nd Year');

-- Seed Sample Resources
INSERT IGNORE INTO resources (resource_id, student_id, title, description, subject, author_name, resource_type, file_path_or_url) VALUES
(1, 2, 'OOP Principles in C#', 'Beginner guide to Encapsulation, Inheritance, and Polymorphism.', 'Computer Science', 'Juan Dela Cruz', 'PDF', 'docs/oop_csharp.pdf'),
(2, 2, 'Database Normalization Notes', '1NF, 2NF, and 3NF examples for relational databases.', 'Information Management', 'Juan Dela Cruz', 'Link', 'https://example.edu/db-notes');