import 'package:equatable/equatable.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';

/// Abstract base class for all invitations states
abstract class InvitationsState extends Equatable {
  const InvitationsState();

  @override
  List<Object?> get props => [];
}

/// Initial state - no data loaded yet
class InvitationsInitial extends InvitationsState {
  const InvitationsInitial();
}

/// Loading state - fetching invitations from API
class InvitationsLoading extends InvitationsState {
  const InvitationsLoading();
}

/// Loaded state - both received and sent invitations are available
/// 
/// This state holds:
/// - receivedInvitations: List of invitations received by current user
/// - sentInvitations: List of invitations sent by current family
/// - selectedTab: Current tab index (0 = Received, 1 = Sent)
/// - loadingInvitationId: Optional UUID of invitation being acted upon
///   - When set, enables granular loading state on specific card
///   - Allows user to interact with other invitations while one is loading
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

  /// Create a copy of this state with some fields replaced
  /// Useful for updating individual fields without recreating entire state
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

/// Error state - failure during any operation
/// 
/// This state is emitted when:
/// - Initial load fails
/// - Send invitation fails
/// - Accept/decline/cancel action fails
/// 
/// Message contains user-friendly error text to display in snackbar
class InvitationsError extends InvitationsState {
  final String message;

  const InvitationsError(this.message);

  @override
  List<Object?> get props => [message];
}
