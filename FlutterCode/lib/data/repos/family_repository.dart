// data/repositories/family_repository.dart

import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/models/Family/family_models.dart';

class FamilyRepository {
  final ApiClient _apiClient;
  final LocalStorage _localStorage;
  final DeviceManager _deviceManager;

  FamilyRepository(
    this._apiClient,
    this._localStorage,
    this._deviceManager,
  );

  /// Get all user families
  Future<FamilyListResult> getUserFamilies() async {
    try {
      final response = await _apiClient.dio.get('/api/families');

      if (response.statusCode == 200) {
        final families =
            (response.data as List).map((f) => FamilyData.fromJson(f)).toList();

        return FamilyListResult.success(families: families);
      }

      return FamilyListResult.failure('Failed to load families');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        return FamilyListResult.failure('Authentication required');
      }
      return FamilyListResult.failure('Network error. Please try again.');
    } catch (e) {
      return FamilyListResult.failure('An unexpected error occurred');
    }
  }

  /// Create a new family
  Future<CreateFamilyResult> createFamily({
    required String name,
    required double initialBudget,
    String? familyBio,
  }) async {
    try {
      AppLogger.info(
        'FamilyRepository',
        'Creating family via API: $name, budget: $initialBudget',
      );

      final response = await _apiClient.dio.post(
        '/api/families',
        data: {
          'name': name,
          'initialBudget': initialBudget,
          if (familyBio != null && familyBio.isNotEmpty) 'familyBio': familyBio,
        },
      );
      AppLogger.info(
        'FamilyRepository',
        'Status Code is : ${response.statusCode}',
      );

      AppLogger.info(
        'FamilyRepository',
        'Response is   : $response',
      );
      if (response.statusCode == 200 || response.statusCode == 201) {
        AppLogger.info('FamilyRepository', 'Family created successfully');
        final familyData = FamilyData.fromJson(response.data);
        return CreateFamilyResult.success(family: familyData);
      }

      return CreateFamilyResult.failure('Failed to create family');
    } on DioException catch (e) {
      AppLogger.error(
        'FamilyRepository',
        'DioException creating family',
        error: e,
      );

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return CreateFamilyResult.failure(error);
      } else if (e.response?.statusCode == 401) {
        return CreateFamilyResult.failure('Authentication required');
      }
      return CreateFamilyResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error(
        'FamilyRepository',
        'Error creating family',
        error: e,
        stackTrace: stackTrace,
      );
      return CreateFamilyResult.failure('An unexpected error occurred');
    }
  }

  /// Select a family - only saves the selection and updates tokens
  /// Does NOT return dashboard data
  Future<SelectFamilyResult> selectFamily(String familyId) async {
    try {
      final deviceInfo = await _deviceManager.getDeviceInfo();

      final response = await _apiClient.dio.post(
        '/api/families/select',
        data: {
          'familyId': familyId,
          'deviceId': deviceInfo.deviceId,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        // Save new auth tokens
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );

        // Save selected family ID
        await _localStorage.saveSelectedFamilyId(familyId);

        return SelectFamilyResult.success();
      }

      return SelectFamilyResult.failure('Failed to select family');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return SelectFamilyResult.failure('Family not found');
      } else if (e.response?.statusCode == 401) {
        return SelectFamilyResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return SelectFamilyResult.failure(error);
      }
      return SelectFamilyResult.failure('Network error. Please try again.');
    } catch (e) {
      return SelectFamilyResult.failure('An unexpected error occurred');
    }
  }

  /// Get selected family ID from local storage
  Future<String?> getSelectedFamilyId() async {
    return await _localStorage.getSelectedFamilyId();
  }
}

// ========== Result Models ==========

class CreateFamilyResult {
  final bool isSuccess;
  final String? errorMessage;
  final FamilyData? family;

  CreateFamilyResult._({
    required this.isSuccess,
    this.errorMessage,
    this.family,
  });

  factory CreateFamilyResult.success({required FamilyData family}) {
    return CreateFamilyResult._(
      isSuccess: true,
      family: family,
    );
  }

  factory CreateFamilyResult.failure(String message) {
    return CreateFamilyResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}

class SelectFamilyResult {
  final bool isSuccess;
  final String? errorMessage;

  SelectFamilyResult._({
    required this.isSuccess,
    this.errorMessage,
  });

  factory SelectFamilyResult.success() {
    return SelectFamilyResult._(isSuccess: true);
  }

  factory SelectFamilyResult.failure(String message) {
    return SelectFamilyResult._(
      isSuccess: false,
      errorMessage: message,
    );
  }
}
