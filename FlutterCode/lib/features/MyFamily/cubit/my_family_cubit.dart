import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_state.dart';

/// Cubit for managing MyFamily page feature
///
/// Handles all business logic for:
/// - Loading current family details with all members
/// - Displaying family information (name, bio, budget)
/// - Displaying family member list
/// - Kicking members (parents only)
/// - Updating family info (parents only)
class MyFamilyCubit extends Cubit<MyFamilyState> {
  final FamilyRepository _repository;
  final LocalStorage _localStorage;
  final DashboardCubit _dashboardCubit;

  MyFamilyCubit(this._repository, this._localStorage, this._dashboardCubit) : super(const MyFamilyInitial());

  /// Load current family details with all members
  Future<void> loadFamilyDetails() async {
    try {
      emit(const MyFamilyLoading());

      final result = await _repository.getFamilyDetails();
      final currentUserId = await _localStorage.getUserId();

      if (result.isSuccess && result.data != null) {
        final familyDetails = result.data!;
        
        // Check if current user is a parent
        final isParent = familyDetails.members.any(
          (m) => m.userId == currentUserId && m.isParent,
        );

        emit(MyFamilyLoaded(
          familyDetails: familyDetails,
          isCurrentUserParent: isParent,
        ));
      } else {
        emit(MyFamilyError(
          result.errorMessage ?? 'Failed to load family details',
        ));
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
    }
  }

  /// Kick a member from the family
  /// Only parents can kick non-parent members
  Future<void> kickMember(String userId) async {
    final currentState = state;
    if (currentState is! MyFamilyLoaded) return;

    try {
      // Set operation in progress
      emit(currentState.copyWith(operationInProgress: userId));

      final result = await _repository.kickMember(userId);

      if (result.isSuccess) {
        // Reload family details to get updated member list
        final refreshResult = await _repository.getFamilyDetails();
        final currentUserId = await _localStorage.getUserId();

        if (refreshResult.isSuccess && refreshResult.data != null) {
          final familyDetails = refreshResult.data!;
          final isParent = familyDetails.members.any(
            (m) => m.userId == currentUserId && m.isParent,
          );

          emit(MyFamilyOperationSuccess(
            message: 'Member removed successfully',
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));

          // Return to loaded state after showing success
          await Future.delayed(const Duration(milliseconds: 100));
          emit(MyFamilyLoaded(
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));
        }
      } else {
        emit(MyFamilyError(result.errorMessage ?? 'Failed to remove member'));
        // Return to loaded state after showing error
        await Future.delayed(const Duration(milliseconds: 100));
        emit(currentState.copyWith(clearOperation: true));
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(currentState.copyWith(clearOperation: true));
    }
  }

  /// Update family information (name and/or bio)
  /// Only parents can update family info
  Future<void> updateFamilyInfo({String? name, String? bio}) async {
    final currentState = state;
    if (currentState is! MyFamilyLoaded) return;

    try {
      emit(const MyFamilyLoading());

      final result = await _repository.updateFamily(name: name, bio: bio);

      if (result.isSuccess) {
        // Reload family details to get updated info
        final refreshResult = await _repository.getFamilyDetails();
        final currentUserId = await _localStorage.getUserId();

        if (refreshResult.isSuccess && refreshResult.data != null) {
          final familyDetails = refreshResult.data!;
          final isParent = familyDetails.members.any(
            (m) => m.userId == currentUserId && m.isParent,
          );

          // Refresh dashboard to update family name across app
          _dashboardCubit.refresh();

          emit(MyFamilyOperationSuccess(
            message: 'Family updated successfully',
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));

          // Return to loaded state after showing success
          await Future.delayed(const Duration(milliseconds: 100));
          emit(MyFamilyLoaded(
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));
        }
      } else {
      emit(MyFamilyError(result.errorMessage ?? 'Failed to update family'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(currentState);
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(currentState);
    }
  }

  /// Leave the current family
  /// After leaving, navigates back to select family page
  Future<bool> leaveFamily() async {
    final currentState = state;
    if (currentState is! MyFamilyLoaded) return false;

    try {
      emit(const MyFamilyLoading());

      final result = await _repository.leaveFamily();

      if (result.isSuccess) {
        emit(MyFamilyOperationSuccess(
          message: 'You have left the family',
          familyDetails: currentState.familyDetails,
          isCurrentUserParent: currentState.isCurrentUserParent,
        ));
        return true;
      } else {
        emit(MyFamilyError(result.errorMessage ?? 'Failed to leave family'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(currentState);
        return false;
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(currentState);
      return false;
    }
  }
}
