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

  factory GoogleSignInService.production() {
    return GoogleSignInService(
      clientId:
          '838822935448-6q27jc3t5vhnckv7bk4fvt9maetenabv.apps.googleusercontent.com',
      serverClientId:
          '838822935448-6q27jc3t5vhnckv7bk4fvt9maetenabv.apps.googleusercontent.com',
    );
  }

  /// Returns null if sign-in was cancelled or failed
  Future<String?> signIn() async {
    try {
      AppLogger.info(_tag, 'Initiating Google Sign-In');

      // Sign out first to force account picker every time
      await _signOutSilently();

      final GoogleSignInAccount? account = await _googleSignIn.signIn();

      if (account == null) {
        AppLogger.info(_tag, 'User cancelled Google Sign-In');
        return null;
      }

      AppLogger.info(_tag, 'User signed in: ${account.email}');

      final GoogleSignInAuthentication auth = await account.authentication;
      final String? idToken = auth.idToken;

      if (idToken == null) {
        AppLogger.error(_tag, 'Failed to retrieve ID token');
        throw GoogleSignInException('Failed to retrieve authentication token');
      }

      AppLogger.info(_tag, 'Successfully retrieved ID token');
      return idToken;
    } on Exception catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Google Sign-In failed',
        error: e,
        stackTrace: stackTrace,
      );
      rethrow;
    }
  }

  Future<void> _signOutSilently() async {
    try {
      await _googleSignIn.signOut();
      AppLogger.info(_tag, 'Signed out silently');
    } catch (e) {
      // Ignore sign-out errors as they're not critical
      AppLogger.info(_tag, 'Silent sign-out skipped or failed (non-critical)');
    }
  }

  Future<void> disconnect() async {
    try {
      await _googleSignIn.disconnect();
      AppLogger.info(_tag, 'Disconnected Google account');
    } catch (e) {
      AppLogger.error(_tag, 'Failed to disconnect Google account', error: e);
    }
  }

  Future<bool> isSignedIn() async {
    return await _googleSignIn.isSignedIn();
  }

  GoogleSignInAccount? get currentUser => _googleSignIn.currentUser;
}

class GoogleSignInException implements Exception {
  final String message;

  GoogleSignInException(this.message);

  @override
  String toString() => 'GoogleSignInException: $message';
}
