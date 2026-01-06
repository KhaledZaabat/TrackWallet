import 'package:flutter/foundation.dart';

class AppLogger {
  static void info(String tag, String message) {
    debugPrint('ℹ️ [$tag] $message');
  }

  static void error(
    String tag,
    String message, {
    Object? error,
    StackTrace? stackTrace,
  }) {
    debugPrint('❌ [$tag] $message');
    if (error != null) debugPrint('   Error: $error');
    if (stackTrace != null) debugPrint('   StackTrace: $stackTrace');
  }
}
