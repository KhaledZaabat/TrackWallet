import 'package:equatable/equatable.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';

abstract class InvitationsState extends Equatable {
  const InvitationsState();

  @override
  List<Object?> get props => [];
}

class InvitationsInitial extends InvitationsState {
  const InvitationsInitial();
}

class InvitationsLoading extends InvitationsState {
  const InvitationsLoading();
}

class InvitationsLoaded extends InvitationsState {
  final List<Invitation> receivedInvitations;
  final List<Invitation> sentInvitations;
  final int selectedTab;
  final String? loadingInvitationId;

  const InvitationsLoaded({
    required this.receivedInvitations,
    required this.sentInvitations,
    this.selectedTab = 0,
    this.loadingInvitationId,
  });

  InvitationsLoaded copyWith({
    List<Invitation>? receivedInvitations,
    List<Invitation>? sentInvitations,
    int? selectedTab,
    String? loadingInvitationId,
  }) {
    return InvitationsLoaded(
      receivedInvitations: receivedInvitations ?? this.receivedInvitations,
      sentInvitations: sentInvitations ?? this.sentInvitations,
      selectedTab: selectedTab ?? this.selectedTab,
      loadingInvitationId: loadingInvitationId ?? this.loadingInvitationId,
    );
  }

  @override
  List<Object?> get props => [
        receivedInvitations,
        sentInvitations,
        selectedTab,
        loadingInvitationId,
      ];
}

class InvitationsError extends InvitationsState {
  final String message;

  const InvitationsError(this.message);

  @override
  List<Object?> get props => [message];
}
