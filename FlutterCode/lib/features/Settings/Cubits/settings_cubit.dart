import 'package:bloc/bloc.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/data/repos/user_repository.dart';
import 'package:famxpense/features/Settings/Cubits/settings_state.dart';

class SettingsCubit extends Cubit<SettingsState> {
  final UserRepository _userRepository;

  SettingsCubit(this._userRepository) : super(const SettingsInitial());

  /// Load user settings
  Future<void> loadSettings() async {
    try {
      emit(const SettingsLoading());
      AppLogger.info('SettingsCubit', 'Loading user settings...');

      final user = await _userRepository.getProfile();

      AppLogger.info('SettingsCubit', 'Settings loaded for: ${user.fullName}');
      emit(SettingsLoaded(user));
    } catch (e, stackTrace) {
      AppLogger.error(
        'SettingsCubit',
        'Failed to load settings',
        error: e,
        stackTrace: stackTrace,
      );
      emit(SettingsError(e.toString()));
    }
  }

  /// Update password
  Future<void> updatePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final currentState = state;

    if (currentState is! SettingsLoaded && currentState is! SettingsError) {
      AppLogger.error('SettingsCubit', 'Cannot update: user not loaded');
      return;
    }

    final currentUser = currentState is SettingsLoaded
        ? currentState.user
        : (currentState as SettingsError).user;

    if (currentUser == null) {
      emit(const SettingsError('User data not available'));
      return;
    }

    try {
      emit(SettingsUpdating(currentUser, 'Updating password...'));
      AppLogger.info('SettingsCubit', 'Updating password...');

      await _userRepository.updatePassword(
        currentPassword: currentPassword,
        newPassword: newPassword,
      );

      AppLogger.info('SettingsCubit', 'Password updated successfully');
      emit(SettingsUpdateSuccess(
        currentUser,
        'Password updated successfully',
      ));

      // Transition back to loaded state
      await Future.delayed(const Duration(milliseconds: 100));
      emit(SettingsLoaded(currentUser));
    } catch (e, stackTrace) {
      AppLogger.error(
        'SettingsCubit',
        'Failed to update password',
        error: e,
        stackTrace: stackTrace,
      );
      emit(SettingsError(e.toString(), user: currentUser));
    }
  }

  /// Update notification preferences
  Future<void> updateNotificationPreferences({
    required bool emailNotifications,
    required bool pushNotifications,
  }) async {
    final currentState = state;

    if (currentState is! SettingsLoaded && currentState is! SettingsError) {
      AppLogger.error('SettingsCubit', 'Cannot update: user not loaded');
      return;
    }

    final currentUser = currentState is SettingsLoaded
        ? currentState.user
        : (currentState as SettingsError).user;

    if (currentUser == null) {
      emit(const SettingsError('User data not available'));
      return;
    }

    try {
      emit(SettingsUpdating(currentUser, 'Updating preferences...'));
      AppLogger.info('SettingsCubit', 'Updating notification preferences...');

      await _userRepository.updateNotificationPreferences(
        emailNotifications: emailNotifications,
        pushNotifications: pushNotifications,
      );

      // Update local user model
      final updatedUser = currentUser.copyWith(
        emailNotifications: emailNotifications,
        pushNotifications: pushNotifications,
      );

      AppLogger.info('SettingsCubit', 'Preferences updated successfully');
      emit(SettingsUpdateSuccess(
        updatedUser,
        'Notification preferences updated',
      ));

      // Transition back to loaded state
      await Future.delayed(const Duration(milliseconds: 100));
      emit(SettingsLoaded(updatedUser));
    } catch (e, stackTrace) {
      AppLogger.error(
        'SettingsCubit',
        'Failed to update notification preferences',
        error: e,
        stackTrace: stackTrace,
      );
      emit(SettingsError(e.toString(), user: currentUser));
    }
  }

  /// Refresh settings
  Future<void> refreshSettings() async {
    final currentState = state;

    if (currentState is SettingsLoaded) {
      try {
        AppLogger.info('SettingsCubit', 'Refreshing settings...');
        final user = await _userRepository.getProfile();
        emit(SettingsLoaded(user));
      } catch (e, stackTrace) {
        AppLogger.error(
          'SettingsCubit',
          'Failed to refresh settings',
          error: e,
          stackTrace: stackTrace,
        );
        emit(SettingsError(e.toString(), user: currentState.user));
      }
    } else {
      await loadSettings();
    }
  }
}
