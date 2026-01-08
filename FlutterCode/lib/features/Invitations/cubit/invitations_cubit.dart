import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:famxpense/data/repos/invitations_repository.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';

/// Cubit for managing invitations feature
/// 
/// Handles all business logic for:
/// - Loading received and sent invitations
/// - Sending new invitations
/// - Accepting/declining received invitations
/// - Cancelling sent invitations
/// - Tab switching
class InvitationsCubit extends Cubit<InvitationsState> {
  final InvitationsRepository _repository;

  InvitationsCubit(this._repository) : super(const InvitationsInitial());

  /// Load all invitations (both received and sent)
  /// 
  /// Fetches:
  /// - Received invitations: GET /api/invitations/received (always)
  /// - Sent invitations: GET /api/invitations/sent (only if family selected)
  /// 
  /// Emits: InvitationsLoading → InvitationsLoaded or InvitationsError
  Future<void> loadAll() async {
    try {
      emit(const InvitationsLoading());

      final receivedResult = await _repository.getReceivedInvitations();
      
      // Only load sent invitations if family is selected
      // Check if family is selected by trying to load sent invitations
      final sentResult = await _repository.getSentInvitations();

      if (receivedResult.isSuccess && sentResult.isSuccess) {
        emit(InvitationsLoaded(
          receivedInvitations: receivedResult.data ?? [],
          sentInvitations: sentResult.data ?? [],
          selectedTab: 0,
        ));
      } else if (receivedResult.isSuccess && !sentResult.isSuccess) {
        // If sent invitations failed (likely no family selected), 
        // still show received invitations
        emit(InvitationsLoaded(
          receivedInvitations: receivedResult.data ?? [],
          sentInvitations: [],
          selectedTab: 0,
        ));
      } else {
        final errorMessage = receivedResult.errorMessage ??
            sentResult.errorMessage ??
            'Failed to load invitations';
        emit(InvitationsError(errorMessage));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  /// Send invitation to user by email
  /// 
  /// Parameters:
  /// - email: Email address of user to invite
  /// - isParent: Whether to invite as parent (true) or member (false)
  /// 
  /// Flow:
  /// 1. Validate email and parameters
  /// 2. Call repository sendInvitation()
  /// 3. On success: call loadAll() to refresh both lists
  /// 4. On error: emit InvitationsError
  /// 
  /// Emits: InvitationsError on validation failure or API error
  /// On success: loadAll() refreshes state
  Future<void> sendInvitation(String email, bool isParent) async {
    try {
      // Validate email is not empty
      if (email.isEmpty) {
        emit(const InvitationsError('Email cannot be empty'));
        return;
      }

      // Call repository
      final result = await _repository.sendInvitation(
        email: email,
        isParent: isParent,
      );

      if (result.isSuccess) {
        // Refresh all invitations after successful send
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to send invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  /// Accept a received invitation
  /// 
  /// Parameters:
  /// - id: Invitation ID to accept
  /// 
  /// Flow:
  /// 1. Emit current Loaded state with loadingInvitationId set (disables button)
  /// 2. Call repository acceptInvitation()
  /// 3. On success: call loadAll() to refresh lists and clear loadingInvitationId
  /// 4. On error: emit InvitationsError
  /// 
  /// Emits: InvitationsError on failure
  /// On success: new InvitationsLoaded state with refreshed data
  Future<void> acceptInvitation(String id) async {
    try {
      // Get current state to preserve data
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      // Emit state with loadingInvitationId to show loading on card
      emit(currentState.copyWith(loadingInvitationId: id));

      // Call repository
      final result = await _repository.acceptInvitation(id);

      if (result.isSuccess) {
        // Refresh all invitations after successful action
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to accept invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  /// Decline a received invitation
  /// 
  /// Parameters:
  /// - id: Invitation ID to decline
  /// 
  /// Flow:
  /// 1. Emit current Loaded state with loadingInvitationId set (disables button)
  /// 2. Call repository declineInvitation()
  /// 3. On success: call loadAll() to refresh lists and clear loadingInvitationId
  /// 4. On error: emit InvitationsError
  /// 
  /// Emits: InvitationsError on failure
  /// On success: new InvitationsLoaded state with refreshed data
  Future<void> declineInvitation(String id) async {
    try {
      // Get current state to preserve data
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      // Emit state with loadingInvitationId to show loading on card
      emit(currentState.copyWith(loadingInvitationId: id));

      // Call repository
      final result = await _repository.declineInvitation(id);

      if (result.isSuccess) {
        // Refresh all invitations after successful action
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to decline invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  /// Cancel a sent invitation (parents only)
  /// 
  /// Parameters:
  /// - id: Invitation ID to cancel
  /// 
  /// Flow:
  /// 1. Emit current Loaded state with loadingInvitationId set (disables button)
  /// 2. Call repository cancelInvitation()
  /// 3. On success: call loadAll() to refresh lists and clear loadingInvitationId
  /// 4. On error: emit InvitationsError (includes 403 Forbidden for non-parents)
  /// 
  /// Emits: InvitationsError on failure or if user is not a parent
  /// On success: new InvitationsLoaded state with refreshed data
  Future<void> cancelInvitation(String id) async {
    try {
      // Get current state to preserve data
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      // Emit state with loadingInvitationId to show loading on card
      emit(currentState.copyWith(loadingInvitationId: id));

      // Call repository
      final result = await _repository.cancelInvitation(id);

      if (result.isSuccess) {
        // Refresh all invitations after successful action
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to cancel invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  /// Switch between Received (0) and Sent (1) tabs
  /// 
  /// Parameters:
  /// - index: Tab index (0 = Received, 1 = Sent)
  /// 
  /// Flow:
  /// 1. Get current Loaded state
  /// 2. Emit new Loaded state with updated selectedTab
  /// 3. Does NOT reload data - both lists already cached in state
  /// 
  /// Emits: InvitationsLoaded with updated selectedTab
  Future<void> switchTab(int index) async {
    try {
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      // Update selectedTab without reloading data
      emit(currentState.copyWith(selectedTab: index));
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }
}
