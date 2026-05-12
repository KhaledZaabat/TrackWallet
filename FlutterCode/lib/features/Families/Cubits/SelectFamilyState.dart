

abstract class SelectFamilyState {}

class SelectFamilyInitial extends SelectFamilyState {}

class SelectFamilyLoading extends SelectFamilyState {}

class SelectFamilyFamiliesLoaded extends SelectFamilyState {
  final List<dynamic> families;

  SelectFamilyFamiliesLoaded({required this.families});
}

class SelectFamilySuccess extends SelectFamilyState {
  SelectFamilySuccess();
}

class SelectFamilyError extends SelectFamilyState {
  final String message;

  SelectFamilyError({required this.message});
}
