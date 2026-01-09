

import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/features/Families/Cubits/create_family_state.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class CreateFamilyCubit extends Cubit<CreateFamilyState> {
  final FamilyRepository _familyRepository;

  CreateFamilyCubit(this._familyRepository) : super(CreateFamilyInitial());

  /// Create a new family
  Future<void> createFamily({
    required String name,
    required double initialBudget,
    String? familyBio,
  }) async {
    if (name.trim().isEmpty) {
      emit(CreateFamilyError(message: 'Family name is required'));
      return;
    }

    if (initialBudget < 0) {
      emit(CreateFamilyError(message: 'Budget cannot be negative'));
      return;
    }

    emit(CreateFamilyLoading());

    try {
      // Log before API call
      AppLogger.info(
        'CreateFamilyCubit',
        'Creating family: $name, budget: $initialBudget',
      );

      final result = await _familyRepository.createFamily(
        name: name.trim(),
        initialBudget: initialBudget,
        familyBio: familyBio?.trim(),
      );

      if (result.isSuccess && result.family != null) {
        // Log success
        AppLogger.info(
          'CreateFamilyCubit',
          'Family created successfully: ${result.family!.id}',
        );
        emit(CreateFamilySuccess(family: result.family!));
      } else {
        // Log failure from repository
        AppLogger.error(
          'CreateFamilyCubit',
          'Failed to create family: ${result.errorMessage}',
        );
        emit(CreateFamilyError(
          message: result.errorMessage ?? 'Failed to create family',
        ));
      }
    } catch (e, stackTrace) {
      // Log unexpected errors
      AppLogger.error(
        'CreateFamilyCubit',
        'Error creating family',
        error: e,
        stackTrace: stackTrace,
      );
      emit(CreateFamilyError(message: 'An unexpected error occurred'));
    }
  }

  /// Reset to initial state
  void reset() {
    emit(CreateFamilyInitial());
  }
}
