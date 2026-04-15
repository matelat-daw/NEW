-- phpMyAdmin SQL Dump
-- version 5.2.3
-- https://www.phpmyadmin.net/
--
-- Servidor: localhost
-- Tiempo de generación: 14-04-2026 a las 22:22:09
-- Versión del servidor: 12.2.2-MariaDB
-- Versión de PHP: 8.5.4

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `clients`
--
CREATE DATABASE IF NOT EXISTS `clients` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;
USE `clients`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `roles`
--

CREATE TABLE `roles` (
  `id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `roles`
--

INSERT INTO `roles` (`id`, `name`, `description`, `created_at`) VALUES
(1, 'USER', 'Usuario regular con funcionalidades básicas', '2026-03-30 17:25:15'),
(2, 'PREMIUM', 'Usuario premium con funcionalidades avanzadas', '2026-03-30 17:25:15'),
(3, 'ADMIN', 'Administrador del sistema con acceso total', '2026-03-30 17:25:15');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `users`
--

CREATE TABLE `users` (
  `id` bigint(20) NOT NULL,
  `nick` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `surname1` varchar(255) NOT NULL,
  `surname2` varchar(255) DEFAULT NULL,
  `bday` date DEFAULT NULL,
  `phone` varchar(255) NOT NULL,
  `gender` enum('MALE','FEMALE','OTHER') NOT NULL,
  `email` varchar(100) NOT NULL,
  `password` varchar(255) NOT NULL,
  `profile_img` varchar(255) DEFAULT NULL,
  `active` tinyint(1) DEFAULT 0,
  `email_verified` tinyint(1) DEFAULT 0,
  `verification_token` varchar(255) DEFAULT NULL,
  `verification_token_expiry` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT current_timestamp(),
  `updated_at` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `users`
--

INSERT INTO `users` (`id`, `nick`, `name`, `surname1`, `surname2`, `bday`, `phone`, `gender`, `email`, `password`, `profile_img`, `active`, `email_verified`, `verification_token`, `verification_token_expiry`, `created_at`, `updated_at`) VALUES
(1, 'ADMIN', 'César Osvaldo', 'Matelat', 'Borneo', '1968-04-05', '664774821', 'MALE', 'cesarmatelat@gmail.com', '$2a$12$XVPo2FFGNpyk3pgudQlwvuK30uflP0jvfSfNY3YBYCEkXQ7IauHgW', '1/profile.jpg', 1, 1, NULL, NULL, '2026-04-01 10:11:02', '2026-04-13 09:48:06'),
(2, 'Orions68', 'César Osvaldo', 'Matelat', 'Borneo', '1968-04-05', '664774821', 'MALE', 'orions68@gmail.com', '$2a$12$8R3q5bcSE6k6w4g9/PhINu/taDY0x6v1JcoEiOvXTTYwwV.X2tPNG', '2/profile.jpeg', 1, 1, NULL, NULL, '2026-04-01 14:22:20', '2026-04-11 22:28:52'),
(3, 'Secondo', 'Second', 'Segundo', NULL, '1970-01-01', '622222222', 'MALE', 'matelat@outlook.es', '$2a$12$rCC6MkzzHo4W9avjsbD8E.N3XaaiRLVKPI7PuZ0W5ulV4dl6EGCkK', '3/profile.png', 1, 1, NULL, NULL, '2026-04-11 23:22:21', '2026-04-12 23:07:47');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `user_roles`
--

CREATE TABLE `user_roles` (
  `id` bigint(20) NOT NULL,
  `user_id` bigint(20) NOT NULL,
  `role_id` int(11) NOT NULL,
  `assigned_at` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Volcado de datos para la tabla `user_roles`
--

INSERT INTO `user_roles` (`id`, `user_id`, `role_id`, `assigned_at`) VALUES
(1, 1, 3, '2026-04-01 11:11:02'),
(2, 2, 2, '2026-04-01 15:22:20'),
(4, 3, 1, '2026-04-12 00:22:21');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `__efmigrationshistory`
--

CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

--
-- Volcado de datos para la tabla `__efmigrationshistory`
--

INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
('20260406162130_InitialCreate', '9.0.0');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `name` (`name`),
  ADD KEY `idx_name` (`name`);

--
-- Indices de la tabla `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nick` (`nick`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_nick` (`nick`),
  ADD KEY `idx_verification_token` (`verification_token`);

--
-- Indices de la tabla `user_roles`
--
ALTER TABLE `user_roles`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unique_user_role` (`user_id`,`role_id`),
  ADD KEY `idx_user_id` (`user_id`),
  ADD KEY `idx_role_id` (`role_id`);

--
-- Indices de la tabla `__efmigrationshistory`
--
ALTER TABLE `__efmigrationshistory`
  ADD PRIMARY KEY (`MigrationId`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `roles`
--
ALTER TABLE `roles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `users`
--
ALTER TABLE `users`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `user_roles`
--
ALTER TABLE `user_roles`
  MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `user_roles`
--
ALTER TABLE `user_roles`
  ADD CONSTRAINT `1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `2` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
