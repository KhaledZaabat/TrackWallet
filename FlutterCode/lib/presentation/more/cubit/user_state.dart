import 'package:equatable/equatable.dart';
import 'package:famxpense/domain/entities/user.dart';

class UserState extends Equatable {
  final User? user;
  final bool isLoading;
  final String? error;
  final bool isSaving;
  final String? successMessage;

  const UserState({
    this.user,
    this.isLoading = false,
    this.error,
    this.isSaving = false,
    this.successMessage,
  });

  UserState copyWith({
    User? user,
    bool? isLoading,
    String? error,
    bool? isSaving,
    String? successMessage,
  }) {
    return UserState(
      user: user ?? this.user,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      isSaving: isSaving ?? this.isSaving,
      successMessage: successMessage,
    );
  }

  @override
  List<Object?> get props =>
      [user, isLoading, error, isSaving, successMessage];
}
