import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/Families/Cubits/SelectFamilyState.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class SelectFamilyCubit extends Cubit<SelectFamilyState> {
  final FamilyRepository _familyRepository;

  SelectFamilyCubit(this._familyRepository) : super(SelectFamilyInitial());

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

  Future<void> selectFamily(String familyId) async {
    emit(SelectFamilyLoading());

    try {
      final result = await _familyRepository.selectFamily(familyId);

      if (result.isSuccess) {
        emit(SelectFamilySuccess());
      } else {
        emit(SelectFamilyError(
            message: result.errorMessage ?? 'Failed to select family'));
        await Future.delayed(const Duration(milliseconds: 100));
        await loadFamilies();
      }
    } catch (e) {
      emit(SelectFamilyError(message: 'An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      await loadFamilies();
    }
  }

  Future<bool> deleteFamily(String familyId) async {
    final currentState = state;
    
    emit(SelectFamilyLoading());

    try {
      final result = await _familyRepository.deleteFamily(familyId);

      if (result.isSuccess) {
        await loadFamilies();
        return true;
      } else {
        emit(SelectFamilyError(
            message: result.errorMessage ?? 'Failed to delete family'));
        await Future.delayed(const Duration(milliseconds: 100));
        await loadFamilies();
        return false;
      }
    } catch (e) {
      emit(SelectFamilyError(message: 'An unexpected error occurred'));
      await Future.delayed(const Duration(milliseconds: 100));
      await loadFamilies();
      return false;
    }
  }
}
