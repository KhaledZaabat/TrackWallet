import 'package:famxpense/models/Family/family_models.dart';

sealed class CreateFamilyState {}

class CreateFamilyInitial extends CreateFamilyState {}

class CreateFamilyLoading extends CreateFamilyState {}

class CreateFamilySuccess extends CreateFamilyState {
  final FamilyData family;

  CreateFamilySuccess({required this.family});
}

class CreateFamilyError extends CreateFamilyState {
  final String message;

  CreateFamilyError({required this.message});
}
