import 'package:equatable/equatable.dart';
import 'package:famxpense/domain/entities/family.dart';

class FamilyState extends Equatable {
  final List<Family> families;
  final String? selectedFamilyId;
  final bool isLoading;
  final bool isSaving;
  final String? error;

  const FamilyState({
    this.families = const [],
    this.selectedFamilyId,
    this.isLoading = false,
    this.isSaving = false,
    this.error,
  });

  Family? get selectedFamily {
    if (selectedFamilyId == null) return null;
    try {
      return families
          .firstWhere((f) => f.id == selectedFamilyId);
    } catch (_) {
      return null;
    }
  }

  FamilyState copyWith({
    List<Family>? families,
    String? selectedFamilyId,
    bool? isLoading,
    bool? isSaving,
    String? error,
  }) {
    return FamilyState(
      families: families ?? this.families,
      selectedFamilyId:
          selectedFamilyId ?? this.selectedFamilyId,
      isLoading: isLoading ?? this.isLoading,
      isSaving: isSaving ?? this.isSaving,
      error: error,
    );
  }

  @override
  List<Object?> get props =>
      [families, selectedFamilyId, isLoading, isSaving, error];
}
