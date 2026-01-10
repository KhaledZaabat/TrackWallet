import 'package:famxpense/core/language/language_state.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class LanguageCubit extends Cubit<LanguageState> {
  final LocalStorage _localStorage;

  LanguageCubit(this._localStorage) : super(const LanguageState(locale: Locale('en')));

  /// Initialize language from stored preference
  Future<void> initialize() async {
    final languageCode = await _localStorage.getLanguage();
    if (languageCode != null) {
      emit(LanguageState(locale: Locale(languageCode)));
    }
  }

  /// Change the app language
  Future<void> changeLanguage(String languageCode) async {
    await _localStorage.saveLanguage(languageCode);
    emit(LanguageState(locale: Locale(languageCode)));
  }

  /// Get current language code
  String get currentLanguageCode => state.locale.languageCode;

  /// Check if current language is French
  bool get isFrench => state.locale.languageCode == 'fr';

  /// Check if current language is English
  bool get isEnglish => state.locale.languageCode == 'en';
}
