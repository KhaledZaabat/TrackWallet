import 'dart:ui';

import 'package:hydrated_bloc/hydrated_bloc.dart';

class LocaleState {
  final Locale locale;
  
  const LocaleState({required this.locale});
  
  factory LocaleState.initial() => const LocaleState(locale: Locale('en'));
  
  LocaleState copyWith({Locale? locale}) {
    return LocaleState(locale: locale ?? this.locale);
  }
  
  Map<String, dynamic> toJson() => {
    'languageCode': locale.languageCode,
  };
  
  factory LocaleState.fromJson(Map<String, dynamic> json) {
    return LocaleState(
      locale: Locale(json['languageCode'] as String? ?? 'en'),
    );
  }
}

class LocaleCubit extends HydratedCubit<LocaleState> {
  LocaleCubit() : super(LocaleState.initial());
  
  void setLocale(Locale locale) {
    emit(LocaleState(locale: locale));
  }
  
  Locale get currentLocale => state.locale;
  
  @override
  LocaleState? fromJson(Map<String, dynamic> json) {
    try {
      return LocaleState.fromJson(json);
    } catch (_) {
      return LocaleState.initial();
    }
  }
  
  @override
  Map<String, dynamic>? toJson(LocaleState state) {
    return state.toJson();
  }
}
