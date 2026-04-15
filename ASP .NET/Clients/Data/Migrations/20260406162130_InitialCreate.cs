using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clients.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usar SQL directo para ser idempotente (CREATE TABLE IF NOT EXISTS)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `users` (
                    `id` bigint NOT NULL AUTO_INCREMENT,
                    `nick` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
                    `name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `surname1` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `surname2` varchar(100) CHARACTER SET utf8mb4 NULL,
                    `email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `phone` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
                    `password` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `gender` varchar(10) CHARACTER SET utf8mb4 NULL,
                    `bday` datetime(6) NULL,
                    `profile_picture` varchar(255) CHARACTER SET utf8mb4 NULL,
                    `role` int NOT NULL DEFAULT 1,
                    `verified` tinyint(1) NOT NULL DEFAULT 0,
                    `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    `updated_at` datetime(6) NULL,
                    `last_login` datetime(6) NULL,
                    `active` tinyint(1) NOT NULL DEFAULT 1,
                    `email_verified` tinyint(1) NOT NULL DEFAULT 0,
                    `verification_token` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `verification_token_expiry` datetime(6) NULL,
                    `password_reset_token` varchar(500) CHARACTER SET utf8mb4 NULL,
                    `password_reset_token_expiry` datetime(6) NULL,
                    PRIMARY KEY (`id`),
                    UNIQUE KEY `IX_users_email` (`email`),
                    UNIQUE KEY `IX_users_nick` (`nick`)
                ) CHARACTER SET=utf8mb4
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
