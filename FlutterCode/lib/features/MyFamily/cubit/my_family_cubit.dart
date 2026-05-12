import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_state.dart';

class MyFamilyCubit extends Cubit<MyFamilyState> {
  final FamilyRepository _repository;
  final LocalStorage _localStorage;
  final DashboardCubit _dashboardCubit;

  MyFamilyCubit(this._repository, this._localStorage, this._dashboardCubit) : super(const MyFamilyInitial());

  Future<void> loadFamilyDetails() async {
    try {
      emit(const MyFamilyLoading());

      final result = await _repository.getFamilyDetails();
      final currentUserId = await _localStorage.getUserId();

      if (result.isSuccess && result.data != null) {
        final familyDetails = result.data!;
        
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

  Future<void> kickMember(String userId) async {
    final currentState = state;
    if (currentState is! MyFamilyLoaded) return;

    try {
      emit(currentState.copyWith(operationInProgress: userId));

      final result = await _repository.kickMember(userId);

      if (result.isSuccess) {
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

          await Future.delayed(const Duration(milliseconds: 100));
          emit(MyFamilyLoaded(
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));
        }
      } else {
        emit(MyFamilyError(result.errorMessage ?? 'Failed to remove member'));
        await Future.delayed(const Duration(milliseconds: 100));
        emit(currentState.copyWith(clearOperation: true));
      }
    } catch (e) {
      emit(MyFamilyError(e.toString()));
      await Future.delayed(const Duration(milliseconds: 100));
      emit(currentState.copyWith(clearOperation: true));
    }
  }

  Future<void> updateFamilyInfo({String? name, String? bio}) async {
    final currentState = state;
    if (currentState is! MyFamilyLoaded) return;

    try {
      emit(const MyFamilyLoading());

      final result = await _repository.updateFamily(name: name, bio: bio);

      if (result.isSuccess) {
        final refreshResult = await _repository.getFamilyDetails();
        final currentUserId = await _localStorage.getUserId();

        if (refreshResult.isSuccess && refreshResult.data != null) {
          final familyDetails = refreshResult.data!;
          final isParent = familyDetails.members.any(
            (m) => m.userId == currentUserId && m.isParent,
          );

          _dashboardCubit.refresh();

          emit(MyFamilyOperationSuccess(
            message: 'Family updated successfully',
            familyDetails: familyDetails,
            isCurrentUserParent: isParent,
          ));

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
