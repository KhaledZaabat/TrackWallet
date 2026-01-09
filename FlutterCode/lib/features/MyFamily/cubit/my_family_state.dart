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
/// - isCurrentUserParent: Whether the current user can perform admin actions
/// - operationInProgress: ID of member being kicked (null if no operation)
class MyFamilyLoaded extends MyFamilyState {
  final FamilyDetails familyDetails;
  final bool isCurrentUserParent;
  final String? operationInProgress; // userId being operated on

  const MyFamilyLoaded({
    required this.familyDetails,
    this.isCurrentUserParent = false,
    this.operationInProgress,
  });

  MyFamilyLoaded copyWith({
    FamilyDetails? familyDetails,
    bool? isCurrentUserParent,
    String? operationInProgress,
    bool clearOperation = false,
  }) {
    return MyFamilyLoaded(
      familyDetails: familyDetails ?? this.familyDetails,
      isCurrentUserParent: isCurrentUserParent ?? this.isCurrentUserParent,
      operationInProgress: clearOperation ? null : (operationInProgress ?? this.operationInProgress),
    );
  }

  @override
  List<Object?> get props => [familyDetails, isCurrentUserParent, operationInProgress];
}

/// Error state - failure during any operation
class MyFamilyError extends MyFamilyState {
  final String message;

  const MyFamilyError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Operation success state - used to trigger snackbar feedback
class MyFamilyOperationSuccess extends MyFamilyState {
  final String message;
  final FamilyDetails familyDetails;
  final bool isCurrentUserParent;

  const MyFamilyOperationSuccess({
    required this.message,
    required this.familyDetails,
    this.isCurrentUserParent = false,
  });

  @override
  List<Object?> get props => [message, familyDetails, isCurrentUserParent];
}
