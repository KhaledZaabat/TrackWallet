import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:uuid/uuid.dart';

class LocalStorage {
  static const String _jwtTokenKey = 'jwt_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _userIdKey = 'user_id';
  static const String _selectedFamilyIdKey = 'selected_family_id';
  static const String _deviceIdKey = 'device_id';
  static const String _fcmTokenKey = 'fcm_token';
  static const String _onboardingKey = 'onboarding_completed';

  final FlutterSecureStorage _secureStorage;
  static const _uuid = Uuid();

  LocalStorage(this._secureStorage);

  factory LocalStorage.standard() {
    const secureStorage = FlutterSecureStorage();
    return LocalStorage(secureStorage);
  }

  Future<void> saveAuthTokens(String jwt, String refresh) async {
    await Future.wait([
      _secureStorage.write(key: _jwtTokenKey, value: jwt),
      _secureStorage.write(key: _refreshTokenKey, value: refresh),
    ]);
  }

  Future<String?> getJwtToken() async {
    return await _secureStorage.read(key: _jwtTokenKey);
  }

  Future<String?> getRefreshToken() async {
    return await _secureStorage.read(key: _refreshTokenKey);
  }

  Future<void> clearAuthTokens() async {
    await Future.wait([
      _secureStorage.delete(key: _jwtTokenKey),
      _secureStorage.delete(key: _refreshTokenKey),
    ]);
  }

  Future<void> saveUserId(String userId) async {
    await _secureStorage.write(key: _userIdKey, value: userId);
  }

  Future<String?> getUserId() async {
    return await _secureStorage.read(key: _userIdKey);
  }

  /// Gets or generates a unique device ID
  /// This ID persists across app launches but is deleted on app reinstall
  Future<String> getOrCreateDeviceId() async {
    String? deviceId = await _secureStorage.read(key: _deviceIdKey);

    if (deviceId == null || deviceId.isEmpty) {
      // Generate a new UUID v7 (time-based)
      deviceId = _uuid.v7();
      await _secureStorage.write(key: _deviceIdKey, value: deviceId);
    }

    return deviceId;
  }

  Future<String?> getDeviceId() async {
    return await _secureStorage.read(key: _deviceIdKey);
  }

  Future<void> saveDeviceId(String deviceId) async {
    await _secureStorage.write(key: _deviceIdKey, value: deviceId);
  }

  /// Saves FCM token and returns true if token was updated
  Future<bool> saveFcmToken(String fcmToken) async {
    final currentToken = await _secureStorage.read(key: _fcmTokenKey);

    // Only save if token is different
    if (currentToken != fcmToken) {
      await _secureStorage.write(key: _fcmTokenKey, value: fcmToken);
      return true; // Token was updated
    }

    return false; // Token unchanged
  }

  Future<String?> getFcmToken() async {
    return await _secureStorage.read(key: _fcmTokenKey);
  }

  Future<void> clearFcmToken() async {
    await _secureStorage.delete(key: _fcmTokenKey);
  }

  Future<void> saveSelectedFamilyId(String familyId) async {
    await _secureStorage.write(key: _selectedFamilyIdKey, value: familyId);
  }

  Future<String?> getSelectedFamilyId() async {
    return await _secureStorage.read(key: _selectedFamilyIdKey);
  }

  Future<void> clearSelectedFamilyId() async {
    await _secureStorage.delete(key: _selectedFamilyIdKey);
  }

  Future<void> setOnboardingCompleted(bool completed) async {
    await _secureStorage.write(
        key: _onboardingKey, value: completed.toString());
  }

  Future<bool> isOnboardingCompleted() async {
    final value = await _secureStorage.read(key: _onboardingKey);
    return value == 'true';
  }

  Future<bool> hasAuthTokens() async {
    final refresh = await getRefreshToken();
    return refresh != null;
  }

  Future<void> clearAll() async {
    await _secureStorage.deleteAll();
  }

  // Read all stored data (useful for debugging)
  Future<Map<String, String>> getAllData() async {
    return await _secureStorage.readAll();
  }
}
