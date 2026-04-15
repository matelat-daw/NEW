<?php

namespace FuturePrograms\Clients\Models;

use FuturePrograms\Clients\Config\Database;
use Exception;

class User
{
    private $pdo;

    public function __construct()
    {
        $this->pdo = Database::getClientConnection();
    }

    public function getAllUsers($page = 0, $size = 10, $excludeUserId = null)
    {
        $offset = $page * $size;

        $excludeClause = '';
        if ($excludeUserId !== null) {
            $excludeClause = 'WHERE u.id <> :excludeUserId';
        }

        $stmt = $this->pdo->prepare('
            SELECT
                u.id,
                u.nick,
                u.name,
                u.surname1,
                u.surname2,
                u.phone,
                u.gender,
                u.bday,
                u.email,
                u.profile_img AS profileImg,
                u.active,
                u.email_verified AS emailVerified,
                u.created_at AS createdAt,
                u.updated_at AS updatedAt,
                COALESCE(UPPER(r.name), "USER") AS role
            FROM users u
            LEFT JOIN user_roles ur ON ur.user_id = u.id
            LEFT JOIN roles r ON r.id = ur.role_id
            ' . $excludeClause . '
            ORDER BY u.id DESC
            LIMIT :limit OFFSET :offset
        ');

        if ($excludeUserId !== null) {
            $stmt->bindValue(':excludeUserId', (int)$excludeUserId, \PDO::PARAM_INT);
        }
        $stmt->bindValue(':limit', (int)$size, \PDO::PARAM_INT);
        $stmt->bindValue(':offset', (int)$offset, \PDO::PARAM_INT);
        $stmt->execute();

        $results = $stmt->fetchAll();
        if ($excludeUserId !== null) {
            $countStmt = $this->pdo->prepare('SELECT COUNT(*) AS total FROM users WHERE id <> ?');
            $countStmt->execute([(int)$excludeUserId]);
        } else {
            $countStmt = $this->pdo->query('SELECT COUNT(*) AS total FROM users');
        }
        $total = (int)$countStmt->fetch()['total'];

        return [
            'users' => $results,
            'total' => $total,
        ];
    }

    public function getUserById($id)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                u.id,
                u.nick,
                u.name,
                u.surname1,
                u.surname2,
                u.phone,
                u.gender,
                u.bday,
                u.email,
                u.password,
                u.profile_img AS profileImg,
                u.active,
                u.email_verified AS emailVerified,
                u.created_at AS createdAt,
                u.updated_at AS updatedAt,
                COALESCE(UPPER(r.name), "USER") AS role
            FROM users u
            LEFT JOIN user_roles ur ON ur.user_id = u.id
            LEFT JOIN roles r ON r.id = ur.role_id
            WHERE u.id = ?
            LIMIT 1
        ');

        $stmt->execute([$id]);
        return $stmt->fetch();
    }

    public function getUserByEmail($email)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                u.id,
                u.nick,
                u.name,
                u.surname1,
                u.surname2,
                u.phone,
                u.gender,
                u.bday,
                u.email,
                u.password,
                u.profile_img AS profileImg,
                u.active,
                u.email_verified AS emailVerified,
                u.created_at AS createdAt,
                u.updated_at AS updatedAt,
                COALESCE(UPPER(r.name), "USER") AS role
            FROM users u
            LEFT JOIN user_roles ur ON ur.user_id = u.id
            LEFT JOIN roles r ON r.id = ur.role_id
            WHERE u.email = ?
            LIMIT 1
        ');

        $stmt->execute([$email]);
        return $stmt->fetch();
    }

    public function getUserByNick($nick)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                u.id,
                u.nick,
                u.name,
                u.surname1,
                u.surname2,
                u.phone,
                u.gender,
                u.bday,
                u.email,
                u.profile_img AS profileImg,
                u.active,
                u.email_verified AS emailVerified,
                u.created_at AS createdAt,
                u.updated_at AS updatedAt,
                COALESCE(UPPER(r.name), "USER") AS role
            FROM users u
            LEFT JOIN user_roles ur ON ur.user_id = u.id
            LEFT JOIN roles r ON r.id = ur.role_id
            WHERE u.nick = ?
            LIMIT 1
        ');

        $stmt->execute([$nick]);
        return $stmt->fetch();
    }

    public function validateCredentials($email, $password)
    {
        $user = $this->getUserByEmail($email);
        return $user ? password_verify($password, $user['password']) : false;
    }

    public function registerUser($nick, $name, $surname1, $surname2, $email, $phone, $password, $gender, $bday = null)
    {
        if ($this->getUserByEmail($email)) {
            throw new Exception('Email already registered');
        }

        if ($this->getUserByNick($nick)) {
            throw new Exception('Nick already taken');
        }

        $hashedPassword = password_hash($password, PASSWORD_BCRYPT);
        $verificationToken = bin2hex(random_bytes(32));
        $verificationTokenExpiry = date('Y-m-d H:i:s', strtotime('+24 hours'));
        $now = date('Y-m-d H:i:s');

        $stmt = $this->pdo->prepare('
            INSERT INTO users
                (nick, name, surname1, surname2, email, phone, password, gender, bday,
                 email_verified, verification_token, verification_token_expiry, created_at, updated_at)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ');

        $stmt->execute([
            $nick,
            $name,
            $surname1,
            $surname2,
            $email,
            $phone,
            $hashedPassword,
            strtoupper((string)$gender),
            $bday,
            0,
            $verificationToken,
            $verificationTokenExpiry,
            $now,
            $now,
        ]);

        $userId = (int)$this->pdo->lastInsertId();
        $this->assignRole($userId, 'USER');

        return $userId;
    }

    private function assignRole($userId, $roleName)
    {
        $roleStmt = $this->pdo->prepare('SELECT id FROM roles WHERE UPPER(name) = UPPER(?) LIMIT 1');
        $roleStmt->execute([$roleName]);
        $role = $roleStmt->fetch();

        if (!$role) {
            throw new Exception('Role not found: ' . $roleName);
        }

        $deleteStmt = $this->pdo->prepare('DELETE FROM user_roles WHERE user_id = ?');
        $deleteStmt->execute([$userId]);

        $insertStmt = $this->pdo->prepare('INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES (?, ?, ?)');
        $insertStmt->execute([$userId, $role['id'], date('Y-m-d H:i:s')]);
    }

    public function verifyEmail($token)
    {
        $stmt = $this->pdo->prepare('
            SELECT id
            FROM users
            WHERE verification_token = ?
              AND (verification_token_expiry IS NULL OR verification_token_expiry >= NOW())
            LIMIT 1
        ');
        $stmt->execute([$token]);
        $user = $stmt->fetch();

        if (!$user) {
            return false;
        }

        $update = $this->pdo->prepare('
            UPDATE users
            SET email_verified = 1,
                verification_token = NULL,
                verification_token_expiry = NULL,
                updated_at = ?
            WHERE id = ?
        ');

        $update->execute([date('Y-m-d H:i:s'), $user['id']]);
        return true;
    }

    public function updateProfileImage($userId, $fileName)
    {
        $stmt = $this->pdo->prepare('
            UPDATE users
            SET profile_img = ?, updated_at = ?
            WHERE id = ?
        ');

        $stmt->execute([
            $userId . '/' . $fileName,
            date('Y-m-d H:i:s'),
            $userId,
        ]);

        return $this->getUserById($userId);
    }

    public function updateProfile($userId, $name, $surname1, $surname2, $phone)
    {
        $stmt = $this->pdo->prepare('
            UPDATE users
            SET name = ?, surname1 = ?, surname2 = ?, phone = ?, updated_at = ?
            WHERE id = ?
        ');

        $stmt->execute([$name, $surname1, $surname2, $phone, date('Y-m-d H:i:s'), $userId]);
        return $this->getUserById($userId);
    }

    public function updatePassword($userId, $newPassword)
    {
        $hashedPassword = password_hash($newPassword, PASSWORD_BCRYPT);
        $stmt = $this->pdo->prepare('
            UPDATE users
            SET password = ?, updated_at = ?
            WHERE id = ?
        ');

        $stmt->execute([$hashedPassword, date('Y-m-d H:i:s'), $userId]);
        return true;
    }

    public function deleteUser($userId)
    {
        $deleteRoles = $this->pdo->prepare('DELETE FROM user_roles WHERE user_id = ?');
        $deleteRoles->execute([$userId]);

        $stmt = $this->pdo->prepare('DELETE FROM users WHERE id = ?');
        $stmt->execute([$userId]);
        return true;
    }

    public static function toDto($user)
    {
        $profileImg = $user['profileImg'] ?? null;
        if (!$profileImg || trim((string)$profileImg) === '') {
            $profileImg = self::getDefaultProfileImagePath($user['gender'] ?? null);
        }

        return [
            'id' => (int)$user['id'],
            'nick' => $user['nick'],
            'name' => $user['name'],
            'surname1' => $user['surname1'],
            'surname2' => $user['surname2'] ?? '',
            'email' => $user['email'],
            'phone' => $user['phone'],
            'gender' => $user['gender'] ?? 'OTHER',
            'bday' => $user['bday'] ?? null,
            'profileImg' => $profileImg,
            'active' => (bool)($user['active'] ?? true),
            'emailVerified' => (bool)($user['emailVerified'] ?? false),
            'role' => strtoupper((string)($user['role'] ?? 'USER')),
            'createdAt' => $user['createdAt'] ?? null,
            'updatedAt' => $user['updatedAt'] ?? null,
        ];
    }

    private static function getDefaultProfileImagePath($gender)
    {
        $normalized = strtoupper(trim((string)$gender));

        if ($normalized === 'MALE') {
            return 'default/male.png';
        }

        if ($normalized === 'FEMALE') {
            return 'default/female.png';
        }

        return 'default/other.png';
    }
}
