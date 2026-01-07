import 'package:famxpense/domain/entities/user.dart';

abstract class SettingsState {
  const SettingsState();
}

class SettingsInitial extends SettingsState {
  const SettingsInitial();
}

class SettingsLoading extends SettingsState {
  const SettingsLoading();
}

class SettingsLoaded extends SettingsState {
  final User user;

  const SettingsLoaded(this.user);
}

class SettingsUpdating extends SettingsState {
  final User user;
  final String action;

  const SettingsUpdating(this.user, this.action);
}

class SettingsUpdateSuccess extends SettingsState {
  final User user;
  final String message;

  const SettingsUpdateSuccess(this.user, this.message);
}

class SettingsError extends SettingsState {
  final String error;
  final User? user;

  const SettingsError(this.error, {this.user});
}
