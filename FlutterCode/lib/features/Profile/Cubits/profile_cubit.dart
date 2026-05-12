import 'dart:io';
import 'package:bloc/bloc.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/data/repos/user_repository.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/Profile/Cubits/profile_state.dart';

class ProfileCubit extends Cubit<ProfileState> {
  final UserRepository _userRepository;
  final DashboardCubit _dashboardCubit;
  final MyFamilyCubit _myFamilyCubit;

  ProfileCubit(this._userRepository, this._dashboardCubit, this._myFamilyCubit) : super(const ProfileInitial());

  Future<void> loadProfile() async {
    try {
      emit(const ProfileLoading());
      AppLogger.info('ProfileCubit', 'Loading user profile...');

      final user = await _userRepository.getProfile();

      AppLogger.info('ProfileCubit', 'Profile loaded: ${user.fullName}');
      emit(ProfileLoaded(user));
    } catch (e, stackTrace) {
      AppLogger.error(
        'ProfileCubit',
        'Failed to load profile',
        error: e,
        stackTrace: stackTrace,
      );
      emit(ProfileError(e.toString()));
    }
  }

  Future<void> updateProfile({
    required String fullName,
    required DateTime birthDate,
    required bool isMale,
    File? profileImage,
  }) async {
    final currentState = state;

    if (currentState is! ProfileLoaded && currentState is! ProfileError) {
      AppLogger.error('ProfileCubit', 'Cannot update: user not loaded');
      return;
    }

    final currentUser = currentState is ProfileLoaded
        ? currentState.user
        : (currentState as ProfileError).user;

    if (currentUser == null) {
      emit(const ProfileError('User data not available'));
      return;
    }

    try {
      emit(ProfileUpdating(currentUser));
      AppLogger.info('ProfileCubit', 'Updating profile...');

      await _userRepository.updateProfile(
        fullName: fullName,
        birthDate: birthDate,
        isMale: isMale,
        profileImage: profileImage,
      );

      final updatedUser = await _userRepository.getProfile();

      _dashboardCubit.refresh();
      _myFamilyCubit.loadFamilyDetails();

      AppLogger.info('ProfileCubit', 'Profile updated successfully');
      emit(ProfileUpdateSuccess(
        updatedUser,
        'Profile updated successfully',
      ));

      await Future.delayed(const Duration(milliseconds: 100));
      emit(ProfileLoaded(updatedUser));
    } catch (e, stackTrace) {
      AppLogger.error(
        'ProfileCubit',
        'Failed to update profile',
        error: e,
        stackTrace: stackTrace,
      );
      emit(ProfileError(e.toString(), user: currentUser));
    }
  }

  Future<void> refreshProfile() async {
    final currentState = state;

    if (currentState is ProfileLoaded) {
      try {
        AppLogger.info('ProfileCubit', 'Refreshing profile...');
        final user = await _userRepository.getProfile();
        emit(ProfileLoaded(user));
      } catch (e, stackTrace) {
        AppLogger.error(
          'ProfileCubit',
          'Failed to refresh profile',
          error: e,
          stackTrace: stackTrace,
        );
        emit(ProfileError(e.toString(), user: currentState.user));
      }
    } else {
      await loadProfile();
    }
  }
}
