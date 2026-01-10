import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';

/// Extension to easily access localized strings
extension LocalizationExtension on BuildContext {
  /// Get the AppLocalizations instance
  AppLocalizations get l10n => AppLocalizations.of(this)!;
}
