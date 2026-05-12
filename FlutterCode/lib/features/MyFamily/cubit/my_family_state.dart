import 'package:equatable/equatable.dart';
import 'package:famxpense/models/Family/family_models.dart';

abstract class MyFamilyState extends Equatable {
  const MyFamilyState();

  @override
  List<Object?> get props => [];
}

class MyFamilyInitial extends MyFamilyState {
  const MyFamilyInitial();
}

class MyFamilyLoading extends MyFamilyState {
  const MyFamilyLoading();
}

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

class MyFamilyError extends MyFamilyState {
  final String message;

  const MyFamilyError(this.message);

  @override
  List<Object?> get props => [message];
}

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
