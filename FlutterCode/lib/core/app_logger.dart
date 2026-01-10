import 'package:flutter/foundation.dart';

class AppLogger {
  static void info(String tag, String message) {
    debugPrint('ℹ️ [$tag] $message');
  }

  static void debug(String tag, String message, {Object? data}) {
    debugPrint('🔍 [$tag] $message');
    if (data != null) debugPrint('   Data: $data');
  }

  static void warning(String tag, String message) {
    debugPrint('⚠️ [$tag] $message');
  }

  static void success(String tag, String message) {
    debugPrint('✅ [$tag] $message');
  }

  static void step(String tag, int stepNumber, String description) {
    debugPrint('📍 [$tag] Step $stepNumber: $description');
  }

  static void error(
    String tag,
    String message, {
    Object? error,
    StackTrace? stackTrace,
  }) {
    debugPrint('❌ [$tag] $message');
    if (error != null) debugPrint('   Error: $error');
    if (error != null) debugPrint('   Error Type: ${error.runtimeType}');
    if (stackTrace != null) debugPrint('   StackTrace: $stackTrace');
  }
}
