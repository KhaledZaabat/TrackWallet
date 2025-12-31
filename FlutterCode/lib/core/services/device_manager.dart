// core/services/device_manager.dart
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import '../storage/local_storage.dart';

class DeviceManager {
  final LocalStorage _localStorage;
  final FirebaseMessaging _firebaseMessaging;
  ApiClient? _apiClient;

  DeviceManager(this._localStorage, this._firebaseMessaging);

  void setApiClient(ApiClient apiClient) {
    _apiClient = apiClient;
  }

  /// Initialize device - generate device ID and request FCM token
  Future<DeviceInfo> initializeDevice() async {
    // Get or create device ID
    final deviceId = await _localStorage.getOrCreateDeviceId();

    // Request FCM token
    String? fcmToken;
    try {
      // Request permission for notifications
      final settings = await _firebaseMessaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );

      if (settings.authorizationStatus == AuthorizationStatus.authorized) {
        fcmToken = await _firebaseMessaging.getToken();
        if (fcmToken != null) {
          await _localStorage.saveFcmToken(fcmToken);
        }
      }
    } catch (e) {
      // FCM token generation failed, continue without it
      print('FCM token generation failed: $e');
    }

    return DeviceInfo(
      deviceId: deviceId,
      fcmToken: fcmToken,
    );
  }

  /// Listen to FCM token refresh
  void listenToFcmTokenRefresh() {
    _firebaseMessaging.onTokenRefresh.listen((newToken) async {
      final wasUpdated = await _localStorage.saveFcmToken(newToken);

      if (wasUpdated) {
        // Notify backend about token update
        await updateFcmTokenOnBackend(newToken);
      }
    });
  }

  /// Update FCM token on backend
  Future<bool> updateFcmTokenOnBackend(String fcmToken) async {
    if (_apiClient == null) {
      print('ApiClient not set. Cannot update FCM token on backend.');
      return false;
    }

    try {
      final response = await _apiClient!.dio.post(
        '/api/user-device/upsert',
        data: {
          'fcmToken': fcmToken,
        },
      );

      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      print('Failed to update FCM token on backend: $e');
      return false;
    }
  }

  /// Get current device info
  Future<DeviceInfo> getDeviceInfo() async {
    final deviceId = await _localStorage.getDeviceId();
    final fcmToken = await _localStorage.getFcmToken();

    return DeviceInfo(
      deviceId: deviceId ?? await _localStorage.getOrCreateDeviceId(),
      fcmToken: fcmToken,
    );
  }
}

class DeviceInfo {
  final String deviceId;
  final String? fcmToken;

  DeviceInfo({
    required this.deviceId,
    this.fcmToken,
  });

  Map<String, dynamic> toJson() => {
        'deviceId': deviceId,
        'fcmToken': fcmToken,
      };
}
