// data/repositories/auth_repository.dart

import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/models/Auth/AuthResult.dart';
import 'package:famxpense/models/Auth/otp_result.dart';
import 'package:famxpense/models/Auth/register_result.dart';
import 'package:famxpense/models/Family/FamilyInfo.dart';


class AuthRepository {
  static const String _tag = 'AuthRepository';

  final ApiClient _apiClient;
  final LocalStorage _localStorage;
  final DeviceManager _deviceManager;

  AuthRepository(
    this._apiClient,
    this._localStorage,
    this._deviceManager,
  );

  /// Register new user
  Future<RegisterResult> register({
    required String email,
    required String password,
    required String username,
    required String fullName,
    required DateTime birthDate,
    required bool isMale,
    String? profileImagePath,
  }) async {
    try {
      AppLogger.info(_tag, 'Starting user registration for email: $email');
      final deviceInfo = await _deviceManager.getDeviceInfo();

      // Format birth date as yyyy-MM-dd
      final formattedDate =
          '${birthDate.year}-${birthDate.month.toString().padLeft(2, '0')}-${birthDate.day.toString().padLeft(2, '0')}';

      // Create FormData
      final formData = FormData.fromMap({
        'Email': email,
        'Password': password,
        'UserName': username,
        'FullName': fullName,
        'BirthDate': formattedDate,
        'IsMale': isMale,
      });

      // Add profile image if provided
      if (profileImagePath != null) {
        formData.files.add(
          MapEntry(
            'ProfileImage',
            await MultipartFile.fromFile(
              profileImagePath,
              filename: profileImagePath.split('/').last,
            ),
          ),
        );
        AppLogger.info(_tag,
            'Profile image attached: ${profileImagePath.split('/').last}');
      }

      final response = await _apiClient.dio.post(
        '/api/identity/register',
        data: formData,
        options: Options(
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        ),
      );

      AppLogger.info(
          _tag, 'Registration response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Registration response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(_tag, 'Registration successful for: $email');
        return RegisterResult.success(email: email);
      }

      AppLogger.error(
          _tag, 'Registration failed with status: ${response.statusCode}');
      return RegisterResult.failure('Registration failed');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Registration DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid registration data';
        return RegisterResult.failure(error);
      } else if (e.response?.statusCode == 409) {
        return RegisterResult.failure('Email or username already exists');
      }
      return RegisterResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Registration unexpected error',
          error: e, stackTrace: stackTrace);
      return RegisterResult.failure('An unexpected error occurred');
    }
  }

  /// Resend confirmation OTP
  Future<OtpResult> resendConfirmationOtp({
    required String email,
  }) async {
    try {
      AppLogger.info(_tag, 'Resending confirmation OTP for: $email');

      final response = await _apiClient.dio.post(
        '/api/identity/confirm-account/otp/resend',
        data: {
          'email': email,
        },
      );

      AppLogger.info(
          _tag, 'Resend OTP response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Resend OTP response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(_tag, 'OTP resent successfully to: $email');
        return OtpResult.success(message: 'OTP sent successfully');
      }

      AppLogger.error(
          _tag, 'Failed to resend OTP - Status: ${response.statusCode}');
      return OtpResult.failure('Failed to send OTP');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Resend OTP DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      } else if (e.response?.statusCode == 409) {
        return OtpResult.failure('Please wait before requesting another OTP');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return OtpResult.failure(error);
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Resend OTP unexpected error',
          error: e, stackTrace: stackTrace);
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Confirm account with OTP
  Future<OtpResult> confirmAccount({
    required String email,
    required String otp,
  }) async {
    try {
      AppLogger.info(_tag, 'Confirming account for: $email');

      final response = await _apiClient.dio.post(
        '/api/identity/confirm-account',
        data: {
          'email': email,
          'otp': otp,
        },
      );

      AppLogger.info(
          _tag, 'Confirm account response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Confirm account response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(_tag, 'Account confirmed successfully for: $email');
        return OtpResult.success(message: 'Account confirmed successfully');
      }

      AppLogger.error(
          _tag, 'Failed to confirm account - Status: ${response.statusCode}');
      return OtpResult.failure('Failed to confirm account');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Confirm account DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 400) {
        return OtpResult.failure('Invalid or expired OTP');
      } else if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Confirm account unexpected error',
          error: e, stackTrace: stackTrace);
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Login with email/username and password
  Future<AuthResult> login({
    required String identifier,
    required String password,
  }) async {
    try {
      AppLogger.info(_tag, 'Starting login for: $identifier');

      // Get device info
      final deviceInfo = await _deviceManager.getDeviceInfo();
      AppLogger.info(_tag, 'Device ID: ${deviceInfo.deviceId}');

      final response = await _apiClient.dio.post(
        '/api/identity/login',
        data: {
          'email': identifier,
          'password': password,
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      AppLogger.info(_tag, 'Login response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Login response - Data: ${response.data}');

      if (response.statusCode == 200) {
        final data = response.data;

        // Save auth tokens
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        // Save user ID
        await _localStorage.saveUserId(data['userId']);

        AppLogger.info(_tag, 'Login successful - User ID: ${data['userId']}');
        AppLogger.info(_tag,
            'User has ${(data['families'] as List?)?.length ?? 0} families');

        return AuthResult.success(
          userId: data['userId'],
          email: data['email'],
          fullName: data['fullName'],
          profileImageUrl: data['profileImageUrl'],
          families: (data['families'] as List?)
              ?.map((f) => FamilyInfo.fromJson(f))
              .toList(),
        );
      }

      AppLogger.error(_tag, 'Login failed - Status: ${response.statusCode}');
      return AuthResult.failure('Login failed');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Login DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 401) {
        return AuthResult.failure('Invalid email or password');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return AuthResult.failure(error);
      }
      return AuthResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Login unexpected error',
          error: e, stackTrace: stackTrace);
      return AuthResult.failure('An unexpected error occurred');
    }
  }

  /// Google login (mobile)
  Future<AuthResult> loginWithGoogle(String idToken) async {
    try {
      AppLogger.step(_tag, 1, '🔐 AUTH REPO: Starting Google login API call');
      AppLogger.debug(_tag, 'ID Token info:', data: {
        'length': idToken.length,
        'preview': '${idToken.substring(0, 50)}...',
      });

      AppLogger.step(_tag, 2, 'Getting device info');
      final deviceInfo = await _deviceManager.getDeviceInfo();
      AppLogger.debug(_tag, 'Device info:', data: {
        'deviceId': deviceInfo.deviceId,
        'hasFcmToken': deviceInfo.fcmToken != null,
        'fcmTokenLength': deviceInfo.fcmToken?.length ?? 0,
      });

      final requestData = {
        'idToken': idToken,
        'deviceId': deviceInfo.deviceId,
        'fcmToken': deviceInfo.fcmToken,
      };

      AppLogger.step(_tag, 3, 'Sending POST to /api/identity/login/google/mobile');
      AppLogger.debug(_tag, 'Request payload (excluding token):', data: {
        'deviceId': deviceInfo.deviceId,
        'hasFcmToken': deviceInfo.fcmToken != null,
      });

      final response = await _apiClient.dio.post(
        '/api/identity/login/google/mobile',
        data: requestData,
      );

      AppLogger.step(_tag, 4, 'Response received');
      AppLogger.debug(_tag, 'Response details:', data: {
        'statusCode': response.statusCode,
        'hasData': response.data != null,
        'dataType': response.data?.runtimeType.toString(),
      });

      if (response.statusCode == 200) {
        final data = response.data;
        AppLogger.success(_tag, '🎉 Google login API call SUCCESSFUL');
        
        AppLogger.step(_tag, 5, 'Saving auth tokens');
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );
        AppLogger.success(_tag, 'Auth tokens saved');

        await _localStorage.saveUserId(data['userId']);
        AppLogger.success(_tag, 'User ID saved: ${data['userId']}');

        AppLogger.debug(_tag, 'User data from backend:', data: {
          'userId': data['userId'],
          'email': data['email'],
          'fullName': data['fullName'],
          'hasProfileImage': data['profileImageUrl'] != null,
          'familiesCount': (data['families'] as List?)?.length ?? 0,
        });

        return AuthResult.success(
          userId: data['userId'],
          email: data['email'],
          fullName: data['fullName'],
          profileImageUrl: data['profileImageUrl'],
          families: (data['families'] as List?)
              ?.map((f) => FamilyInfo.fromJson(f))
              .toList(),
        );
      }

      AppLogger.error(_tag, '❌ Google login failed - Status: ${response.statusCode}');
      AppLogger.debug(_tag, 'Response body: ${response.data}');
      return AuthResult.failure('Google login failed');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(_tag, '❌ DioException during Google login', error: e, stackTrace: stackTrace);
      AppLogger.debug(_tag, 'DioException details:', data: {
        'statusCode': e.response?.statusCode,
        'type': e.type.toString(),
        'message': e.message,
      });
      AppLogger.debug(_tag, 'Response data: ${e.response?.data}');
      AppLogger.debug(_tag, 'Response headers: ${e.response?.headers}');
      
      // Decode error message from backend
      String errorMessage = 'Network error. Please try again.';
      if (e.response?.statusCode == 401) {
        errorMessage = 'Invalid Google token - backend could not verify';
        AppLogger.warning(_tag, 'HINT: Check if backend is using the correct Google Client ID');
      } else if (e.response?.statusCode == 400) {
        errorMessage = e.response?.data['detail'] ?? 'Invalid request';
        AppLogger.warning(_tag, 'HINT: Request payload may be malformed');
      } else if (e.response?.statusCode == 500) {
        errorMessage = 'Server error during Google authentication';
        AppLogger.warning(_tag, 'HINT: Check backend logs for the error');
        if (e.response?.data != null) {
          AppLogger.debug(_tag, 'Server error details: ${e.response?.data}');
        }
      }
      
      return AuthResult.failure(errorMessage);
    } catch (e, stackTrace) {
      AppLogger.error(_tag, '❌ Unexpected error during Google login', error: e, stackTrace: stackTrace);
      return AuthResult.failure('An unexpected error occurred');
    }
  }

  /// Logout
  Future<bool> logout() async {
    try {
      AppLogger.info(_tag, 'Starting logout');
      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/identity/logout',
        data: {
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      AppLogger.info(_tag, 'Logout response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Logout response - Data: ${response.data}');

      // Clear local storage (keep FCM token for future logins)
      await _localStorage.clearAuthTokens();
      await _localStorage.clearSelectedFamilyId();

      AppLogger.info(_tag, 'Logout successful');
      return true;
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Logout error', error: e, stackTrace: stackTrace);
      // Still clear local storage even if API call fails (keep FCM token)
      await _localStorage.clearAuthTokens();
      await _localStorage.clearSelectedFamilyId();
      return false;
    }
  }

  Future<OtpResult> sendResetPasswordOtp({
    required String email,
  }) async {
    try {
      AppLogger.info(_tag, 'Sending reset password OTP for: $email');

      final response = await _apiClient.dio.post(
        '/api/identity/reset-password/otp/send',
        data: {
          'email': email,
        },
      );

      AppLogger.info(
          _tag, 'Send reset OTP response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Send reset OTP response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(_tag, 'Reset password OTP sent successfully to: $email');
        return OtpResult.success(message: 'OTP sent successfully');
      }

      AppLogger.error(
          _tag, 'Failed to send reset OTP - Status: ${response.statusCode}');
      return OtpResult.failure('Failed to send OTP');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Send reset OTP DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      } else if (e.response?.statusCode == 409) {
        return OtpResult.failure('Please wait before requesting another OTP');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return OtpResult.failure(error);
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Send reset OTP unexpected error',
          error: e, stackTrace: stackTrace);
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Verify reset password OTP
  Future<OtpResult> verifyResetPasswordOtp({
    required String email,
    required String otp,
  }) async {
    try {
      AppLogger.info(_tag, 'Verifying reset password OTP for: $email');

      final response = await _apiClient.dio.post(
        '/api/identity/reset-password/otp/verify',
        data: {
          'email': email,
          'otp': otp,
        },
      );

      AppLogger.info(
          _tag, 'Verify reset OTP response - Status: ${response.statusCode}');
      AppLogger.info(
          _tag, 'Verify reset OTP response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(
            _tag, 'Reset password OTP verified successfully for: $email');
        return OtpResult.success(message: 'OTP verified successfully');
      }

      AppLogger.error(
          _tag, 'Failed to verify reset OTP - Status: ${response.statusCode}');
      return OtpResult.failure('Failed to verify OTP');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Verify reset OTP DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 400) {
        return OtpResult.failure('Invalid or expired OTP');
      } else if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Verify reset OTP unexpected error',
          error: e, stackTrace: stackTrace);
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Reset password
  Future<OtpResult> resetPassword({
    required String email,
    required String newPassword,
  }) async {
    try {
      AppLogger.info(_tag, 'Resetting password for: $email');

      final response = await _apiClient.dio.post(
        '/api/identity/reset-password',
        data: {
          'email': email,
          'newPassword': newPassword,
        },
      );

      AppLogger.info(
          _tag, 'Reset password response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Reset password response - Data: ${response.data}');

      if (response.statusCode == 200) {
        AppLogger.info(_tag, 'Password reset successfully for: $email');
        return OtpResult.success(message: 'Password reset successfully');
      }

      AppLogger.error(
          _tag, 'Failed to reset password - Status: ${response.statusCode}');
      return OtpResult.failure('Failed to reset password');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Reset password DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return OtpResult.failure(error);
      } else if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      } else if (e.response?.statusCode == 409) {
        return OtpResult.failure(
            'New password cannot be the same as old password');
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Reset password unexpected error',
          error: e, stackTrace: stackTrace);
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Check if user is authenticated
  Future<bool> isAuthenticated() async {
    final result = await _localStorage.hasAuthTokens();
    AppLogger.info(_tag, 'User authenticated: $result');
    return result;
  }

  /// Get current user ID
  Future<String?> getCurrentUserId() async {
    final userId = await _localStorage.getUserId();
    AppLogger.info(_tag, 'Current user ID: ${userId ?? "None"}');
    return userId;
  }

  Future<AuthResult> refreshToken() async {
    try {
      AppLogger.info(_tag, 'Starting token refresh');

      final refreshToken = await _localStorage.getRefreshToken();
      if (refreshToken == null) {
        AppLogger.error(_tag, 'No refresh token available');
        return AuthResult.failure('No refresh token available');
      }

      AppLogger.info(
          _tag, 'Refresh token present, length: ${refreshToken.length}');

      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/identity/refresh',
        data: {
          'refreshToken': refreshToken,
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      AppLogger.info(
          _tag, 'Token refresh response - Status: ${response.statusCode}');
      AppLogger.info(_tag, 'Token refresh response - Data: ${response.data}');

      if (response.statusCode == 200) {
        final data = response.data;

        // Save new tokens
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        // Save user ID
        await _localStorage.saveUserId(data['userId']);

        AppLogger.info(_tag,
            '✅ Token refreshed successfully - User ID: ${data['userId']}');

        return AuthResult.success(
          userId: data['userId'],
          email: data['email'],
          fullName: data['fullName'],
          profileImageUrl: data['profileImageUrl'],
          families: (data['families'] as List?)
              ?.map((f) => FamilyInfo.fromJson(f))
              .toList(),
        );
      }

      AppLogger.error(
          _tag, 'Failed to refresh token - Status: ${response.statusCode}');
      return AuthResult.failure('Failed to refresh token');
    } on DioException catch (e, stackTrace) {
      AppLogger.error(
        _tag,
        'Token refresh DioException - Status: ${e.response?.statusCode}',
        error: e,
        stackTrace: stackTrace,
      );
      AppLogger.error(_tag, 'Response data: ${e.response?.data}');

      if (e.response?.statusCode == 401) {
        // Refresh token is invalid, clear storage
        await _localStorage.clearAuthTokens();
        AppLogger.info(_tag, 'Session expired, tokens cleared');
        return AuthResult.failure('Session expired');
      }
      return AuthResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(_tag, 'Token refresh unexpected error',
          error: e, stackTrace: stackTrace);
      return AuthResult.failure('An unexpected error occurred');
    }
  }
}

// ========== Result Models ==========
