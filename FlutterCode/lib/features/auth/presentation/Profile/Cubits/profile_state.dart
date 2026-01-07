import 'package:famxpense/domain/entities/user.dart';

abstract class ProfileState {
  const ProfileState();
}

class ProfileInitial extends ProfileState {
  const ProfileInitial();
}

class ProfileLoading extends ProfileState {
  const ProfileLoading();
}

class ProfileLoaded extends ProfileState {
  final User user;

  const ProfileLoaded(this.user);
}

class ProfileUpdating extends ProfileState {
  final User user;

  const ProfileUpdating(this.user);
}

class ProfileUpdateSuccess extends ProfileState {
  final User user;
  final String message;

  const ProfileUpdateSuccess(this.user, this.message);
}

class ProfileError extends ProfileState {
  final String error;
  final User? user;

  const ProfileError(this.error, {this.user});
}
