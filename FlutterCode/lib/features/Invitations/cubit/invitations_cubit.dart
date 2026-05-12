import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:famxpense/data/repos/invitations_repository.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_state.dart';


class InvitationsCubit extends Cubit<InvitationsState> {
  final InvitationsRepository _repository;

  InvitationsCubit(this._repository) : super(const InvitationsInitial());

  Future<void> loadAll() async {
    try {
      emit(const InvitationsLoading());

      final receivedResult = await _repository.getReceivedInvitations();
      
      final sentResult = await _repository.getSentInvitations();

      if (receivedResult.isSuccess && sentResult.isSuccess) {
        emit(InvitationsLoaded(
          receivedInvitations: receivedResult.data ?? [],
          sentInvitations: sentResult.data ?? [],
          selectedTab: 0,
        ));
      } else if (receivedResult.isSuccess && !sentResult.isSuccess) {
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

  Future<void> sendInvitation(String email, bool isParent) async {
    try {
      if (email.isEmpty) {
        emit(const InvitationsError('Email cannot be empty'));
        return;
      }

      final result = await _repository.sendInvitation(
        email: email,
        isParent: isParent,
      );

      if (result.isSuccess) {
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to send invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  Future<void> acceptInvitation(String id) async {
    try {
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      emit(currentState.copyWith(loadingInvitationId: id));

      final result = await _repository.acceptInvitation(id);

      if (result.isSuccess) {
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to accept invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  Future<void> declineInvitation(String id) async {
    try {
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      emit(currentState.copyWith(loadingInvitationId: id));

      final result = await _repository.declineInvitation(id);

      if (result.isSuccess) {
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to decline invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  Future<void> cancelInvitation(String id) async {
    try {
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      emit(currentState.copyWith(loadingInvitationId: id));

      final result = await _repository.cancelInvitation(id);

      if (result.isSuccess) {
        await loadAll();
      } else {
        emit(InvitationsError(result.errorMessage ?? 'Failed to cancel invitation'));
      }
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }

  Future<void> switchTab(int index) async {
    try {
      final currentState = state;
      if (currentState is! InvitationsLoaded) return;

      emit(currentState.copyWith(selectedTab: index));
    } catch (e) {
      emit(InvitationsError(e.toString()));
    }
  }
}
