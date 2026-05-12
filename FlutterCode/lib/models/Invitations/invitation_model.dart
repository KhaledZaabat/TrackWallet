import 'package:equatable/equatable.dart';
import 'package:intl/intl.dart';

/// Invitation status enum (case-sensitive, must match backend exactly)
enum InvitationStatus {
  pending('Pending'),
  accepted('Accepted'),
  declined('Declined'),
  cancelled('Cancelled');

  final String displayName;
  const InvitationStatus(this.displayName);
}

class Invitation extends Equatable {
  final String invitationId;
  final String inviteeUserId;
  final String inviteeEmail;
  final String inviterUserId;
  final String inviterName;
  final String familyId;
  final String familyName;
  final bool isParent;
  final InvitationStatus status;
  final DateTime sentAtUtc;

  const Invitation({
    required this.invitationId,
    required this.inviteeUserId,
    required this.inviteeEmail,
    required this.inviterUserId,
    required this.inviterName,
    required this.familyId,
    required this.familyName,
    required this.isParent,
    required this.status,
    required this.sentAtUtc,
  });

  /// Create Invitation from API JSON response
  /// API returns ISO8601 datetime string for sentAtUtc
  /// Status enum must match exactly (case-sensitive)
  factory Invitation.fromJson(Map<String, dynamic> json) {
    return Invitation(
      invitationId: json['invitationId'] as String,
      inviteeUserId: json['inviteeUserId'] as String,
      inviteeEmail: json['inviteeEmail'] as String? ?? '',
      inviterUserId: json['inviterUserId'] as String,
      inviterName: json['inviterName'] as String? ?? 'Unknown',
      familyId: json['familyId'] as String,
      familyName: json['familyName'] as String? ?? 'Unknown',
      isParent: json['isParent'] as bool? ?? false,
      status: _parseStatus(json['status'] as String),
      sentAtUtc: DateTime.parse(json['sentAtUtc'] as String),
    );
  }

  /// Parse status string to enum (case-sensitive, must match backend)
  static InvitationStatus _parseStatus(String statusString) {
    try {
      return InvitationStatus.values.firstWhere(
        (status) => status.name == statusString.toLowerCase(),
      );
    } catch (e) {
      // Fallback to pending if status not found
      return InvitationStatus.pending;
    }
  }

  Map<String, dynamic> toJson() {
    return {
      'invitationId': invitationId,
      'inviteeUserId': inviteeUserId,
      'inviteeEmail': inviteeEmail,
      'inviterUserId': inviterUserId,
      'inviterName': inviterName,
      'familyId': familyId,
      'familyName': familyName,
      'isParent': isParent,
      'status': status.displayName,
      'sentAtUtc': sentAtUtc.toIso8601String(),
    };
  }

  String getFormattedDate() {
    return DateFormat('MMM dd, yyyy').format(sentAtUtc.toLocal());
  }

  String getRoleDisplay() {
    return isParent ? 'parent' : 'member';
  }

  Invitation copyWith({
    String? invitationId,
    String? inviteeUserId,
    String? inviteeEmail,
    String? inviterUserId,
    String? inviterName,
    String? familyId,
    String? familyName,
    bool? isParent,
    InvitationStatus? status,
    DateTime? sentAtUtc,
  }) {
    return Invitation(
      invitationId: invitationId ?? this.invitationId,
      inviteeUserId: inviteeUserId ?? this.inviteeUserId,
      inviteeEmail: inviteeEmail ?? this.inviteeEmail,
      inviterUserId: inviterUserId ?? this.inviterUserId,
      inviterName: inviterName ?? this.inviterName,
      familyId: familyId ?? this.familyId,
      familyName: familyName ?? this.familyName,
      isParent: isParent ?? this.isParent,
      status: status ?? this.status,
      sentAtUtc: sentAtUtc ?? this.sentAtUtc,
    );
  }

  @override
  List<Object?> get props => [
        invitationId,
        inviteeUserId,
        inviteeEmail,
        inviterUserId,
        inviterName,
        familyId,
        familyName,
        isParent,
        status,
        sentAtUtc,
      ];
}
