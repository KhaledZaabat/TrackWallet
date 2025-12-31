import 'package:uuid/v8.dart';

class User {
  final String id;
  final String username;
  final String fullName;
  final bool isMale;
  final String email;
  final DateTime dateOfBirth;
  final String profilePictureUrl;

  User._({
    required this.id,
    required this.username,
    required this.fullName,
    required this.isMale,
    required this.email,
    required this.dateOfBirth,
    required this.profilePictureUrl,
  });

  /// Factory used when creating a new user in the app.
  /// `hashedPassword` is still the *raw* password here – it will be hashed in the repo.
  static User create({
    required String username,
    required String fullName,
    required bool isMale,
    required String email,
    required DateTime dateOfBirth,
    required String profilePictureUrl,
    required String hashedPassword,
  }) {
    return User._(
      id: UuidV8().generate(),
      username: username,
      fullName: fullName,
      isMale: isMale,
      email: email,
      dateOfBirth: dateOfBirth,
      profilePictureUrl: profilePictureUrl,
    );
  }

  /// Factory used when we already have a persisted id+hashed password.
  static User fromId({
    required String id,
    required String username,
    required String fullName,
    required bool isMale,
    required String email,
    required DateTime dateOfBirth,
    required String profilePictureUrl,
    required String hashedPassword,
  }) {
    return User._(
      id: id,
      username: username,
      fullName: fullName,
      isMale: isMale,
      email: email,
      dateOfBirth: dateOfBirth,
      profilePictureUrl: profilePictureUrl,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      "id": id,
      "username": username,
      "fullName": fullName,
      "isMale": isMale ? 1 : 0,
      "email": email,
      "dateOfBirth": dateOfBirth.toIso8601String(),
      "profilePictureUrl": profilePictureUrl,
    };
  }

  static User fromJson(Map<String, dynamic> json) {
    final dynamic isMaleRaw = json["isMale"];
    final bool isMale = isMaleRaw is bool ? isMaleRaw : isMaleRaw == 1;

    // `username` was added later → fall back to email if missing.
    final String username =
        (json["username"] as String?) ?? (json["email"] as String);

    return User._(
        id: json["id"] as String,
        username: username,
        fullName: json["fullName"] as String,
        isMale: isMale,
        email: json["email"] as String,
        dateOfBirth: DateTime.parse(json["dateOfBirth"] as String),
        profilePictureUrl: json["profilePictureUrl"] as String);
  }
}
