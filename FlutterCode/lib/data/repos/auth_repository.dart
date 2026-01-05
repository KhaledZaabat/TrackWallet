// data/repositories/auth_repository.dart

import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/models/Auth/AuthResult.dart';
import 'package:famxpense/models/Auth/otp_result.dart';
import 'package:famxpense/models/Auth/register_result.dart';
import 'package:famxpense/models/Family/FamilyInfo.dart';
import 'package:famxpense/models/Family/family_models.dart';

class AuthRepository {
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

      if (response.statusCode == 200) {
        return RegisterResult.success(email: email);
      }

      return RegisterResult.failure('Registration failed');
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid registration data';
        return RegisterResult.failure(error);
      } else if (e.response?.statusCode == 409) {
        return RegisterResult.failure('Email or username already exists');
      }
      return RegisterResult.failure('Network error. Please try again.');
    } catch (e) {
      return RegisterResult.failure('An unexpected error occurred');
    }
  }

  /// Resend confirmation OTP
  Future<OtpResult> resendConfirmationOtp({
    required String email,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/identity/confirm-account/otp/resend',
        data: {
          'email': email,
        },
      );

      if (response.statusCode == 200) {
        return OtpResult.success(message: 'OTP sent successfully');
      }

      return OtpResult.failure('Failed to send OTP');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      } else if (e.response?.statusCode == 409) {
        return OtpResult.failure('Please wait before requesting another OTP');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return OtpResult.failure(error);
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e) {
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Confirm account with OTP
  Future<OtpResult> confirmAccount({
    required String email,
    required String otp,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/identity/confirm-account',
        data: {
          'email': email,
          'otp': otp,
        },
      );

      if (response.statusCode == 200) {
        return OtpResult.success(message: 'Account confirmed successfully');
      }

      return OtpResult.failure('Failed to confirm account');
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        return OtpResult.failure('Invalid or expired OTP');
      } else if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e) {
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Login with email/username and password
  Future<AuthResult> login({
    required String identifier,
    required String password,
  }) async {
    try {
      // Get device info
      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/identity/login',
        data: {
          'email': identifier,
          'password': password,
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        // Save auth tokens
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        // Save user ID
        await _localStorage.saveUserId(data['userId']);

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

      return AuthResult.failure('Login failed');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        return AuthResult.failure('Invalid email or password');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return AuthResult.failure(error);
      }
      return AuthResult.failure('Network error. Please try again.');
    } catch (e) {
      return AuthResult.failure('An unexpected error occurred');
    }
  }

  /// Google login (mobile)
  Future<AuthResult> loginWithGoogle(String idToken) async {
    try {
      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/identity/login/google/mobile',
        data: {
          'idToken': idToken,
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        await _localStorage.saveUserId(data['userId']);

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

      return AuthResult.failure('Google login failed');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        return AuthResult.failure('Invalid Google token');
      }
      return AuthResult.failure('Network error. Please try again.');
    } catch (e) {
      return AuthResult.failure('An unexpected error occurred');
    }
  }

  /// Logout
  Future<bool> logout() async {
    try {
      final deviceInfo = await _deviceManager.getDeviceInfo();

      await _apiClient.dio.post(
        '/api/identity/logout',
        data: {
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      // Clear local storage
      await _localStorage.clearAuthTokens();
      await _localStorage.clearFcmToken();

      return true;
    } catch (e) {
      // Still clear local storage even if API call fails
      await _localStorage.clearAuthTokens();
      await _localStorage.clearFcmToken();
      return false;
    }
  }

  Future<OtpResult> sendResetPasswordOtp({
    required String email,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/identity/reset-password/otp/send',
        data: {
          'email': email,
        },
      );

      if (response.statusCode == 200) {
        return OtpResult.success(message: 'OTP sent successfully');
      }

      return OtpResult.failure('Failed to send OTP');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      } else if (e.response?.statusCode == 409) {
        return OtpResult.failure('Please wait before requesting another OTP');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return OtpResult.failure(error);
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e) {
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Verify reset password OTP
  Future<OtpResult> verifyResetPasswordOtp({
    required String email,
    required String otp,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/identity/reset-password/otp/verify',
        data: {
          'email': email,
          'otp': otp,
        },
      );

      if (response.statusCode == 200) {
        return OtpResult.success(message: 'OTP verified successfully');
      }

      return OtpResult.failure('Failed to verify OTP');
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        return OtpResult.failure('Invalid or expired OTP');
      } else if (e.response?.statusCode == 404) {
        return OtpResult.failure('Account not found');
      }
      return OtpResult.failure('Network error. Please try again.');
    } catch (e) {
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Reset password
  Future<OtpResult> resetPassword({
    required String email,
    required String newPassword,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/identity/reset-password',
        data: {
          'email': email,
          'newPassword': newPassword,
        },
      );

      if (response.statusCode == 200) {
        return OtpResult.success(message: 'Password reset successfully');
      }

      return OtpResult.failure('Failed to reset password');
    } on DioException catch (e) {
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
    } catch (e) {
      return OtpResult.failure('An unexpected error occurred');
    }
  }

  /// Check if user is authenticated
  Future<bool> isAuthenticated() async {
    return await _localStorage.hasAuthTokens();
  }

  /// Get current user ID
  Future<String?> getCurrentUserId() async {
    return await _localStorage.getUserId();
  }

  Future<AuthResult> refreshToken() async {
    try {
      final refreshToken = await _localStorage.getRefreshToken();
      if (refreshToken == null) {
        return AuthResult.failure('No refresh token available');
      }

      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/identity/refresh',
        data: {
          'refreshToken': refreshToken,
          'deviceId': deviceInfo.deviceId,
          'fcmToken': deviceInfo.fcmToken,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        // Save new tokens
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        // Save user ID
        await _localStorage.saveUserId(data['userId']);

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

      return AuthResult.failure('Failed to refresh token');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        // Refresh token is invalid, clear storage
        await _localStorage.clearAuthTokens();
        return AuthResult.failure('Session expired');
      }
      return AuthResult.failure('Network error. Please try again.');
    } catch (e) {
      return AuthResult.failure('An unexpected error occurred');
    }
  }
}

// ========== Result Models ==========
