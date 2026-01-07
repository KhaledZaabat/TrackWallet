// data/repos/user_repository.dart

import 'dart:io';
import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/domain/entities/user.dart';

class UserRepository {
  final ApiClient _apiClient;

  UserRepository(this._apiClient);

  /// Get current user profile
  Future<User> getProfile() async {
    try {
      AppLogger.info('UserRepository', 'Fetching user profile...');

      final response = await _apiClient.dio.get('/api/users/profile');

      if (response.statusCode == 200) {
        AppLogger.info('UserRepository', 'Profile fetched successfully');
        return User.fromJson(response.data);
      }

      throw Exception('Failed to fetch profile: ${response.statusCode}');
    } on DioException catch (e) {
      AppLogger.error(
        'UserRepository',
        'Failed to fetch profile',
        error: e,
        stackTrace: e.stackTrace,
      );

      if (e.response?.statusCode == 401) {
        throw Exception('Unauthorized. Please login again.');
      } else if (e.response?.statusCode == 404) {
        throw Exception('User profile not found.');
      }

      throw Exception('Network error: ${e.message}');
    } catch (e, stackTrace) {
      AppLogger.error(
        'UserRepository',
        'Unexpected error fetching profile',
        error: e,
        stackTrace: stackTrace,
      );
      throw Exception('Failed to fetch profile: $e');
    }
  }

  /// Update user profile
  Future<void> updateProfile({
    required String fullName,
    required DateTime birthDate,
    required bool isMale,
    File? profileImage,
  }) async {
    try {
      AppLogger.info('UserRepository', 'Updating user profile...');

      final formData = FormData.fromMap({
        'FullName': fullName,
        'BirthDate': birthDate.toIso8601String().split('T')[0], // yyyy-MM-dd
        'IsMale': isMale,
      });

      if (profileImage != null) {
        AppLogger.info('UserRepository', 'Including profile image in update');
        formData.files.add(
          MapEntry(
            'ProfileImage',
            await MultipartFile.fromFile(
              profileImage.path,
              filename: profileImage.path.split('/').last,
            ),
          ),
        );
      }

      final response = await _apiClient.dio.put(
        '/api/users/profile',
        data: formData,
      );

      if (response.statusCode == 200) {
        AppLogger.info('UserRepository', 'Profile updated successfully');
        return;
      }

      throw Exception('Failed to update profile: ${response.statusCode}');
    } on DioException catch (e) {
      AppLogger.error(
        'UserRepository',
        'Failed to update profile',
        error: e,
        stackTrace: e.stackTrace,
      );

      if (e.response?.statusCode == 400) {
        final errorMessage = e.response?.data['message'] ?? 'Invalid data';
        throw Exception(errorMessage);
      } else if (e.response?.statusCode == 401) {
        throw Exception('Unauthorized. Please login again.');
      }

      throw Exception('Network error: ${e.message}');
    } catch (e, stackTrace) {
      AppLogger.error(
        'UserRepository',
        'Unexpected error updating profile',
        error: e,
        stackTrace: stackTrace,
      );
      throw Exception('Failed to update profile: $e');
    }
  }

  /// Update user password
  Future<void> updatePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    try {
      AppLogger.info('UserRepository', 'Updating password...');

      final response = await _apiClient.dio.post(
        '/api/users/update-password',
        data: {
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        },
      );

      if (response.statusCode == 200) {
        AppLogger.info('UserRepository', 'Password updated successfully');
        return;
      }

      throw Exception('Failed to update password: ${response.statusCode}');
    } on DioException catch (e) {
      AppLogger.error(
        'UserRepository',
        'Failed to update password',
        error: e,
        stackTrace: e.stackTrace,
      );

      if (e.response?.statusCode == 400) {
        throw Exception('Invalid current password or new password format.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Unauthorized. Please login again.');
      } else if (e.response?.statusCode == 409) {
        throw Exception('Password conflict. Please try a different password.');
      }

      throw Exception('Network error: ${e.message}');
    } catch (e, stackTrace) {
      AppLogger.error(
        'UserRepository',
        'Unexpected error updating password',
        error: e,
        stackTrace: stackTrace,
      );
      throw Exception('Failed to update password: $e');
    }
  }

  /// Update notification preferences
  Future<void> updateNotificationPreferences({
    required bool emailNotifications,
    required bool pushNotifications,
  }) async {
    try {
      AppLogger.info('UserRepository', 'Updating notification preferences...');

      final response = await _apiClient.dio.patch(
        '/api/notification-preferences',
        data: {
          'emailNotifications': emailNotifications,
          'pushNotifications': pushNotifications,
        },
      );

      if (response.statusCode == 200) {
        AppLogger.info('UserRepository', 'Notification preferences updated');
        return;
      }

      throw Exception('Failed to update preferences: ${response.statusCode}');
    } on DioException catch (e) {
      AppLogger.error(
        'UserRepository',
        'Failed to update notification preferences',
        error: e,
        stackTrace: e.stackTrace,
      );

      if (e.response?.statusCode == 400) {
        throw Exception('Invalid notification preferences data.');
      } else if (e.response?.statusCode == 401) {
        throw Exception('Unauthorized. Please login again.');
      }

      throw Exception('Network error: ${e.message}');
    } catch (e, stackTrace) {
      AppLogger.error(
        'UserRepository',
        'Unexpected error updating notification preferences',
        error: e,
        stackTrace: stackTrace,
      );
      throw Exception('Failed to update notification preferences: $e');
    }
  }
}
