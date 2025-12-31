import 'package:famxpense/models/Family/FamilyInfo.dart';

class AuthData {
  final String userId;
  final String email;
  final String fullName;
  final String? profileImageUrl;
  final List<FamilyInfo> families;

  AuthData({
    required this.userId,
    required this.email,
    required this.fullName,
    this.profileImageUrl,
    required this.families,
  });
}
