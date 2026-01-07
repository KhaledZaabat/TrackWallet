class User {
  final String id;
  final String fullName;
  final String userName;
  final String email;
  final DateTime birthDate;
  final bool isMale;
  final String profileImageUrl;
  final bool emailNotifications;
  final bool pushNotifications;

  User({
    required this.id,
    required this.fullName,
    required this.userName,
    required this.email,
    required this.birthDate,
    required this.isMale,
    required this.profileImageUrl,
    required this.emailNotifications,
    required this.pushNotifications,
  });

  User copyWith({
    String? id,
    String? fullName,
    String? userName,
    String? email,
    DateTime? birthDate,
    bool? isMale,
    String? profileImageUrl,
    bool? emailNotifications,
    bool? pushNotifications,
  }) {
    return User(
      id: id ?? this.id,
      fullName: fullName ?? this.fullName,
      userName: userName ?? this.userName,
      email: email ?? this.email,
      birthDate: birthDate ?? this.birthDate,
      isMale: isMale ?? this.isMale,
      profileImageUrl: profileImageUrl ?? this.profileImageUrl,
      emailNotifications: emailNotifications ?? this.emailNotifications,
      pushNotifications: pushNotifications ?? this.pushNotifications,
    );
  }

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] as String? ?? '',
      fullName: json['fullName'] as String? ?? '',
      userName: json['userName'] as String? ?? '',
      email: json['email'] as String? ?? '',
      birthDate: json['birthDate'] != null
          ? DateTime.parse(json['birthDate'] as String)
          : DateTime.now(),
      isMale: json['isMale'] as bool? ?? true,
      profileImageUrl: json['profileImageUrl'] as String? ?? '',
      emailNotifications: json['emailNotifications'] as bool? ?? true,
      pushNotifications: json['pushNotifications'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'fullName': fullName,
      'userName': userName,
      'email': email,
      'birthDate': birthDate.toIso8601String(),
      'isMale': isMale,
      'profileImageUrl': profileImageUrl,
      'emailNotifications': emailNotifications,
      'pushNotifications': pushNotifications,
    };
  }
}
