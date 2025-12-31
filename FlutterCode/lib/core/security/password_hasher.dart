import 'dart:convert';
import 'package:crypto/crypto.dart';

class PasswordHasher {
  /// Hashes a plain password using SHA-256
  static String hash(String password) {
    final bytes = utf8.encode(password);
    final digest = sha256.convert(bytes);
    return digest.toString();
  }

  /// Verifies a plain password against the stored hash
  static bool verify(String plain, String hashed) {
    return hash(plain) == hashed;
  }
}
