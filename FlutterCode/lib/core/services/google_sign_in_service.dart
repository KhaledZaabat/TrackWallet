import 'package:famxpense/core/app_logger.dart';
import 'package:google_sign_in/google_sign_in.dart';

class GoogleSignInService {
  static const String _tag = 'GoogleSignInService';

  late final GoogleSignIn _googleSignIn;

  GoogleSignInService({
    required String clientId,
    required String serverClientId,
  }) {
    _googleSignIn = GoogleSignIn(
      scopes: ['email', 'profile'],
      serverClientId: serverClientId,
      clientId: clientId,
    );
  }

  /// Factory constructor for production use
  factory GoogleSignInService.production() {
    return GoogleSignInService(
      clientId:
          '838822935448-6q27jc3t5vhnckv7bk4fvt9maetenabv.apps.googleusercontent.com',
      serverClientId:
          '838822935448-6q27jc3t5vhnckv7bk4fvt9maetenabv.apps.googleusercontent.com',
    );
  }

  /// Sign in with Google and return ID token
  /// Returns null if sign-in was cancelled or failed
  Future<String?> signIn() async {
    try {
      AppLogger.step(_tag, 1, '🚀 STARTING Google Sign-In Flow');
      AppLogger.debug(_tag, 'Configuration check:', data: {
        'serverClientId': _googleSignIn.serverClientId,
        'scopes': _googleSignIn.scopes,
      });

      // Sign out first to force account picker every time
      AppLogger.step(_tag, 2, 'Signing out silently to force account picker');
      await _signOutSilently();
      AppLogger.success(_tag, 'Silent sign-out completed');

      // Trigger sign-in flow
      AppLogger.step(_tag, 3, 'Triggering Google Sign-In dialog');
      AppLogger.info(_tag, 'Waiting for user to select account...');
      
      final GoogleSignInAccount? account = await _googleSignIn.signIn();

      if (account == null) {
        AppLogger.warning(_tag, 'User cancelled Google Sign-In or dialog failed');
        AppLogger.debug(_tag, 'Possible reasons: User pressed back, dialog was dismissed, or configuration error');
        return null;
      }

      AppLogger.step(_tag, 4, 'Account selected successfully');
      AppLogger.success(_tag, 'User signed in: ${account.email}');
      AppLogger.debug(_tag, 'Account details:', data: {
        'email': account.email,
        'displayName': account.displayName,
        'id': account.id,
        'photoUrl': account.photoUrl,
      });

      // Get authentication tokens
      AppLogger.step(_tag, 5, 'Requesting authentication tokens from Google');
      final GoogleSignInAuthentication auth = await account.authentication;
      
      AppLogger.debug(_tag, 'Authentication response received:', data: {
        'hasIdToken': auth.idToken != null,
        'idTokenLength': auth.idToken?.length ?? 0,
        'hasAccessToken': auth.accessToken != null,
        'accessTokenLength': auth.accessToken?.length ?? 0,
      });

      final String? idToken = auth.idToken;

      if (idToken == null) {
        AppLogger.error(_tag, '❌ CRITICAL: ID Token is NULL!');
        AppLogger.error(_tag, 'This usually means serverClientId is incorrect or not configured');
        AppLogger.debug(_tag, 'Current serverClientId: ${_googleSignIn.serverClientId}');
        throw GoogleSignInException('Failed to retrieve authentication token - ID Token is null');
      }

      AppLogger.step(_tag, 6, 'ID Token retrieved successfully');
      AppLogger.success(_tag, '✅ Google Sign-In COMPLETE');
      AppLogger.debug(_tag, 'ID Token preview: ${idToken.substring(0, 50)}...');
      AppLogger.info(_tag, 'ID Token length: ${idToken.length} characters');
      
      return idToken;
    } on Exception catch (e, stackTrace) {
      AppLogger.error(_tag, '❌ GOOGLE SIGN-IN FAILED', error: e, stackTrace: stackTrace);
      AppLogger.debug(_tag, 'Exception type: ${e.runtimeType}');
      
      // Provide specific guidance based on error type
      if (e.toString().contains('sign_in_canceled')) {
        AppLogger.info(_tag, 'Hint: User cancelled the sign-in');
      } else if (e.toString().contains('network_error')) {
        AppLogger.info(_tag, 'Hint: Network connectivity issue');
      } else if (e.toString().contains('sign_in_failed')) {
        AppLogger.info(_tag, 'Hint: Check SHA-1 fingerprint and OAuth configuration in Google Cloud Console');
      }
      
      rethrow;
    }
  }

  /// Sign out silently (no UI)
  Future<void> _signOutSilently() async {
    try {
      await _googleSignIn.signOut();
      AppLogger.info(_tag, 'Signed out silently');
    } catch (e) {
      // Ignore sign-out errors as they're not critical
      AppLogger.info(_tag, 'Silent sign-out skipped or failed (non-critical)');
    }
  }

  /// Disconnect Google account completely
  Future<void> disconnect() async {
    try {
      await _googleSignIn.disconnect();
      AppLogger.info(_tag, 'Disconnected Google account');
    } catch (e) {
      AppLogger.error(_tag, 'Failed to disconnect Google account', error: e);
    }
  }

  /// Check if user is currently signed in
  Future<bool> isSignedIn() async {
    return await _googleSignIn.isSignedIn();
  }

  /// Get current signed-in account
  GoogleSignInAccount? get currentUser => _googleSignIn.currentUser;
}

/// Custom exception for Google Sign-In errors
class GoogleSignInException implements Exception {
  final String message;

  GoogleSignInException(this.message);

  @override
  String toString() => 'GoogleSignInException: $message';
}
