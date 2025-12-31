import 'package:famxpense/core/security/password_hasher.dart';
import 'package:famxpense/data/database/repositories/abstractions/i_user_repository.dart';
import 'package:famxpense/data/database/repositories/concrete/session_repository.dart';
import 'package:famxpense/domain/entities/user.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'user_state.dart';

class UserCubit extends Cubit<UserState> {
  final IUserRepository _userRepository;
  final SessionRepository _sessionRepository;

  UserCubit(this._userRepository, this._sessionRepository)
      : super(const UserState());

  Future<void> loadCurrentUser() async {
    emit(state.copyWith(
      isLoading: true,
      error: null,
      successMessage: null,
    ));

    try {
      final String? userId = await _sessionRepository.getCurrentUser();
      if (userId == null) {
        emit(state.copyWith(
          isLoading: false,
          error: 'No user is currently logged in',
        ));
        return;
      }

      final User? user = await _userRepository.getById(userId);
      if (user == null) {
        emit(state.copyWith(
          isLoading: false,
          error: 'Failed to load user',
        ));
        return;
      }

      emit(state.copyWith(
        user: user,
        isLoading: false,
        error: null,
        successMessage: null,
      ));
    } catch (_) {
      emit(state.copyWith(
        isLoading: false,
        error: 'Failed to load user',
        successMessage: null,
      ));
    }
  }

  Future<void> updateProfile({
    required String fullName,
    required bool isMale,
    required DateTime dateOfBirth,
    required String email,
    String? password,
    String? profilePictureUrl,
  }) async {
    final User? existingUser = state.user;

    if (existingUser == null) {
      emit(state.copyWith(
        error: 'No user loaded',
        isSaving: false,
        successMessage: null,
      ));
      return;
    }

    emit(state.copyWith(
      isSaving: true,
      error: null,
      successMessage: null,
    ));

    try {
      final bool shouldUpdatePassword =
          password != null && password.trim().isNotEmpty;

      final User updatedUser = User.fromId(
        id: existingUser.id,
        username: existingUser.username,
        fullName: fullName,
        isMale: isMale,
        email: email,
        dateOfBirth: dateOfBirth,
        profilePictureUrl: profilePictureUrl ?? existingUser.profilePictureUrl,
        hashedPassword: shouldUpdatePassword
            ? PasswordHasher.hash(password.trim())
            : existingUser.hashedPassword,
      );

      await _userRepository.update(updatedUser);

      emit(state.copyWith(
        user: updatedUser,
        isSaving: false,
        error: null,
        successMessage: 'Profile updated successfully',
      ));
    } catch (_) {
      emit(state.copyWith(
        isSaving: false,
        error: 'Failed to update profile',
        successMessage: null,
      ));
    }
  }
}
