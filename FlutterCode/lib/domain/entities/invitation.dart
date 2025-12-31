import 'package:uuid/v8.dart';

enum InvitationStatus {
  pending,
  accepted,
  declined,
}

class Invitation {
  final String id;
  final String inviteeUserId;
  final String inviterUserId;
  final String familyId;
  final bool isParent;
  final DateTime sentAt;
  final InvitationStatus status;

  Invitation._({
    required this.id,
    required this.inviteeUserId,
    required this.inviterUserId,
    required this.familyId,
    required this.isParent,
    required this.sentAt,
    required this.status,
  });

  static final UuidV8 _uuidGenerator = UuidV8();

  static Invitation create({
    required String inviteeUserId,
    required String inviterUserId,
    required String familyId,
    required bool isParent,
  }) {
    return Invitation._(
      id: _uuidGenerator.generate(),
      inviteeUserId: inviteeUserId,
      inviterUserId: inviterUserId,
      familyId: familyId,
      isParent: isParent,
      sentAt: DateTime.now(),
      status: InvitationStatus.pending,
    );
  }

  Invitation accept() {
    if (status == InvitationStatus.accepted) {
      throw StateError("Invitation already accepted.");
    }
    if (status == InvitationStatus.declined) {
      throw StateError("Invitation was declined and cannot be accepted.");
    }

    return Invitation._(
      id: id,
      inviteeUserId: inviteeUserId,
      inviterUserId: inviterUserId,
      familyId: familyId,
      isParent: isParent,
      sentAt: sentAt,
      status: InvitationStatus.accepted,
    );
  }

  Invitation decline() {
    if (status == InvitationStatus.declined) {
      throw StateError("Invitation already declined.");
    }
    if (status == InvitationStatus.accepted) {
      throw StateError("Invitation was accepted and cannot be declined.");
    }

    return Invitation._(
      id: id,
      inviteeUserId: inviteeUserId,
      inviterUserId: inviterUserId,
      familyId: familyId,
      isParent: isParent,
      sentAt: sentAt,
      status: InvitationStatus.declined,
    );
  }

  Invitation cancel({required String requesterUserId}) {
    if (requesterUserId != inviterUserId) {
      throw StateError("Only the inviter can cancel this invitation.");
    }
    if (status != InvitationStatus.pending) {
      throw StateError("Only pending invitations can be canceled.");
    }

    return Invitation._(
      id: id,
      inviteeUserId: inviteeUserId,
      inviterUserId: inviterUserId,
      familyId: familyId,
      isParent: isParent,
      sentAt: sentAt,
      status: InvitationStatus.declined,
    );
  }

  static Invitation fromId({
    required String id,
    required String inviteeUserId,
    required String inviterUserId,
    required String familyId,
    required bool isParent,
    required DateTime sentAt,
    required InvitationStatus status,
  }) {
    return Invitation._(
      id: id,
      inviteeUserId: inviteeUserId,
      inviterUserId: inviterUserId,
      familyId: familyId,
      isParent: isParent,
      sentAt: sentAt,
      status: status,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      "id": id,
      "inviteeUserId": inviteeUserId,
      "inviterUserId": inviterUserId,
      "familyId": familyId,
      "isParent": isParent,
      "sentAt": sentAt.toIso8601String(),
      "status": status.name,
    };
  }

  static Invitation fromJson(Map<String, dynamic> json) {
    return Invitation._(
      id: json["id"],
      inviteeUserId: json["inviteeUserId"],
      inviterUserId: json["inviterUserId"],
      familyId: json["familyId"],
      isParent: json["isParent"],
      sentAt: DateTime.parse(json["sentAt"]),
      status: InvitationStatus.values.byName(json["status"]),
    );
  }
}
