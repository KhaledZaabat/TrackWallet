import 'package:famxpense/models/Family/family_models.dart';

sealed class CreateFamilyState {}

/// Initial state
class CreateFamilyInitial extends CreateFamilyState {}

/// Loading state while creating family
class CreateFamilyLoading extends CreateFamilyState {}

/// Family created successfully
class CreateFamilySuccess extends CreateFamilyState {
  final FamilyData family;

  CreateFamilySuccess({required this.family});
}

/// Error occurred during family creation
class CreateFamilyError extends CreateFamilyState {
  final String message;

  CreateFamilyError({required this.message});
}
