import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/Families/Cubits/SelectFamilyState.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class SelectFamilyCubit extends Cubit<SelectFamilyState> {
  final FamilyRepository _familyRepository;

  SelectFamilyCubit(this._familyRepository) : super(SelectFamilyInitial());

  /// Load user's families
  Future<void> loadFamilies() async {
    emit(SelectFamilyLoading());

    try {
      final result = await _familyRepository.getUserFamilies();

      if (result.isSuccess) {
        emit(SelectFamilyFamiliesLoaded(families: result.families ?? []));
      } else {
        emit(SelectFamilyError(
            message: result.errorMessage ?? 'Failed to load families'));
      }
    } catch (e) {
      emit(SelectFamilyError(message: 'An unexpected error occurred'));
    }
  }

  /// Select a family - just saves the selection, doesn't load dashboard
  Future<void> selectFamily(String familyId) async {
    emit(SelectFamilyLoading());

    try {
      final result = await _familyRepository.selectFamily(familyId);

      if (result.isSuccess) {
        // Emit simple success - dashboard will be loaded separately
        emit(SelectFamilySuccess());
      } else {
        emit(SelectFamilyError(
            message: result.errorMessage ?? 'Failed to select family'));
        // Return to families loaded state
        await Future.delayed(const Duration(milliseconds: 100));
        await loadFamilies();
      }
    } catch (e) {
      emit(SelectFamilyError(message: 'An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      await loadFamilies();
    }
  }
}
