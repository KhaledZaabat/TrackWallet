import 'package:uuid/v8.dart';

class FamilyUser {
  final String id;
  final String familyId;
  final String userId;
  final bool isParent;
  final String invitedByID;
  final DateTime joinedAt;

  FamilyUser._({
    required this.id,
    required this.familyId,
    required this.userId,
    required this.isParent,
    required this.invitedByID,
    required this.joinedAt,
  });

  static final UuidV8 _uuidGenerator = UuidV8();

  static FamilyUser create({
    required String familyId,
    required String userId,
    required bool isParent,
    required String invitedByID,
  }) {
    return FamilyUser._(
      id: _uuidGenerator.generate(),
      familyId: familyId,
      userId: userId,
      isParent: isParent,
      invitedByID: invitedByID,
      joinedAt: DateTime.now(),
    );
  }

  static FamilyUser fromId({
    required String id,
    required String familyId,
    required String userId,
    required bool isParent,
    required String invitedByID,
    required DateTime joinedAt,
  }) {
    return FamilyUser._(
      id: id,
      familyId: familyId,
      userId: userId,
      isParent: isParent,
      invitedByID: invitedByID,
      joinedAt: joinedAt,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      "id": id,
      "familyId": familyId,
      "userId": userId,
      "isParent": isParent,
      "invitedByID": invitedByID,
      "joinedAt": joinedAt.toIso8601String(),
    };
  }

  static FamilyUser fromJson(Map<String, dynamic> json) {
    return FamilyUser._(
      id: json["id"],
      familyId: json["familyId"],
      userId: json["userId"],
      isParent: json["isParent"],
      invitedByID: json["invitedByID"],
      joinedAt: DateTime.parse(json["joinedAt"]),
    );
  }
}
