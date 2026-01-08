import 'package:equatable/equatable.dart';
import 'package:famxpense/models/Family/family_models.dart';

/// Abstract base class for all MyFamily states
abstract class MyFamilyState extends Equatable {
  const MyFamilyState();

  @override
  List<Object?> get props => [];
}

/// Initial state - no data loaded yet
class MyFamilyInitial extends MyFamilyState {
  const MyFamilyInitial();
}

/// Loading state - fetching family details from API
class MyFamilyLoading extends MyFamilyState {
  const MyFamilyLoading();
}

/// Loaded state - family details and members are available
///
/// This state holds:
/// - familyDetails: Complete family information including all members
///   - id: Family ID
///   - name: Family name
///   - currentBudget: Current family budget
///   - familyBio: Optional family description
///   - members: List of all family members with their profile info
class MyFamilyLoaded extends MyFamilyState {
  final FamilyDetails familyDetails;

  const MyFamilyLoaded({required this.familyDetails});

  @override
  List<Object?> get props => [familyDetails];
}

/// Error state - failure during any operation
///
/// This state is emitted when:
/// - Initial load fails
/// - API error occurs (401, 404, network error, etc.)
///
/// Message contains user-friendly error text to display in snackbar
class MyFamilyError extends MyFamilyState {
  final String message;

  const MyFamilyError(this.message);

  @override
  List<Object?> get props => [message];
}
