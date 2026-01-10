// data/repositories/family_repository.dart

import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/models/Family/family_models.dart';

// ========== ApiResult Pattern ==========

/// Generic result wrapper for API calls following consistent error handling pattern
class ApiResult<T> {
  final bool isSuccess;
  final T? data;
  final String? errorMessage;

  ApiResult.success(this.data)
      : isSuccess = true,
        errorMessage = null;

  ApiResult.error(this.errorMessage)
      : isSuccess = false,
        data = null;
}

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

      AppLogger.info('FamilyRepository', 'Selecting family: $familyId');

      final response = await _apiClient.dio.post(
        '/api/families/select',
        data: {
          'familyId': familyId,
          'deviceId': deviceInfo.deviceId,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;

        final newJwt = data['jwtToken']['token'] as String;
        final newRefresh = data['refreshToken']['token'] as String;

        AppLogger.info('FamilyRepository', 'Received new JWT token (${newJwt.length} chars)');

        // Save new auth tokens
        await _localStorage.saveAuthTokens(newJwt, newRefresh);

        // Verify token was saved correctly
        final savedToken = await _localStorage.getJwtToken();
        AppLogger.info('FamilyRepository', 'Verified saved JWT (${savedToken?.length ?? 0} chars), match: ${savedToken == newJwt}');

        // Save selected family ID
        await _localStorage.saveSelectedFamilyId(familyId);

        return SelectFamilyResult.success();
      }

      return SelectFamilyResult.failure('Failed to select family');
    } on DioException catch (e) {
      AppLogger.error('FamilyRepository', 'DioException selecting family', error: e);
      if (e.response?.statusCode == 404) {
        return SelectFamilyResult.failure('Family not found');
      } else if (e.response?.statusCode == 401) {
        return SelectFamilyResult.failure('Authentication required');
      } else if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return SelectFamilyResult.failure(error);
      }
      return SelectFamilyResult.failure('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error('FamilyRepository', 'Error selecting family', error: e, stackTrace: stackTrace);
      return SelectFamilyResult.failure('An unexpected error occurred');
    }
  }


  /// Get selected family ID from local storage
  Future<String?> getSelectedFamilyId() async {
    return await _localStorage.getSelectedFamilyId();
  }

  /// Get current family details with all members
  /// GET /api/families/me
  /// Returns: FamilyDetails with all members
  /// Used by MyFamily page to display family info and member list
  Future<ApiResult<FamilyDetails>> getFamilyDetails() async {
    try {
      final response = await _apiClient.dio.get('/api/families/me');

      if (response.statusCode == 200) {
        final familyDetails = FamilyDetails.fromJson(response.data as Map<String, dynamic>);
        return ApiResult.success(familyDetails);
      }

      return ApiResult.error('Failed to load family details');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Family not found');
      }
      return ApiResult.error(e.message ?? 'Network error. Please try again.');
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Kick a member from the family
  /// DELETE /api/families/members/{userId}
  /// Only parents can kick non-parent members. Cannot kick self or other parents.
  Future<ApiResult<void>> kickMember(String userId) async {
    try {
      AppLogger.info('FamilyRepository', 'Kicking member: $userId');

      final response = await _apiClient.dio.delete(
        '/api/families/members/$userId',
      );

      if (response.statusCode == 200) {
        AppLogger.info('FamilyRepository', 'Member kicked successfully');
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to remove member');
    } on DioException catch (e) {
      AppLogger.error('FamilyRepository', 'DioException kicking member', error: e);

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Cannot remove this member';
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      } else if (e.response?.statusCode == 403) {
        return ApiResult.error('Only parents can remove members');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Member not found');
      }
      return ApiResult.error('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error('FamilyRepository', 'Error kicking member', error: e, stackTrace: stackTrace);
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Update family information (name and/or bio)
  /// PUT /api/families/select
  /// Only parents can update family information.
  Future<ApiResult<void>> updateFamily({
    String? name,
    String? bio,
  }) async {
    try {
      AppLogger.info('FamilyRepository', 'Updating family: name=$name, bio=$bio');

      final data = <String, dynamic>{};
      if (name != null && name.isNotEmpty) data['name'] = name;
      if (bio != null) data['familyBio'] = bio;

      if (data.isEmpty) {
        return ApiResult.error('No changes to update');
      }

      final response = await _apiClient.dio.put(
        '/api/families',
        data: data,
      );

      if (response.statusCode == 200) {
        AppLogger.info('FamilyRepository', 'Family updated successfully');
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to update family');
    } on DioException catch (e) {
      AppLogger.error('FamilyRepository', 'DioException updating family', error: e);

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Invalid request';
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      } else if (e.response?.statusCode == 403) {
        return ApiResult.error('Only parents can update family information');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Family not found');
      }
      return ApiResult.error('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error('FamilyRepository', 'Error updating family', error: e, stackTrace: stackTrace);
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Leave the current family
  /// DELETE /api/families/leave
  /// The last parent cannot leave if other members exist.
  /// User's transactions remain with the family.
  Future<ApiResult<void>> leaveFamily() async {
    try {
      AppLogger.info('FamilyRepository', 'Leaving family');

      final response = await _apiClient.dio.delete('/api/families/leave');

      if (response.statusCode == 200) {
        AppLogger.info('FamilyRepository', 'Left family successfully');
        // Clear selected family since user is no longer in it
        await _localStorage.clearSelectedFamilyId();
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to leave family');
    } on DioException catch (e) {
      AppLogger.error('FamilyRepository', 'DioException leaving family', error: e);

      if (e.response?.statusCode == 400) {
        final error = e.response?.data['detail'] ?? 'Cannot leave family';
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      } else if (e.response?.statusCode == 403) {
        return ApiResult.error('You cannot leave as the last parent while other members exist');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Family not found');
      }
      return ApiResult.error('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error('FamilyRepository', 'Error leaving family', error: e, stackTrace: stackTrace);
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Delete a family
  /// DELETE /api/families/{familyId}
  /// Only parents can delete. All members removed, invitations cancelled.
  /// Transactions preserved for historical data.
  Future<ApiResult<void>> deleteFamily(String familyId) async {
    try {
      AppLogger.info('FamilyRepository', 'Deleting family: $familyId');

      final response = await _apiClient.dio.delete('/api/families/$familyId');

      if (response.statusCode == 200) {
        AppLogger.info('FamilyRepository', 'Family deleted successfully');
        // Clear selected family if it was the deleted one
        final currentFamilyId = await _localStorage.getSelectedFamilyId();
        if (currentFamilyId == familyId) {
          await _localStorage.clearSelectedFamilyId();
        }
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to delete family');
    } on DioException catch (e) {
      AppLogger.error('FamilyRepository', 'DioException deleting family', error: e);

      if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      } else if (e.response?.statusCode == 403) {
        return ApiResult.error('Only parents can delete this family');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Family not found');
      }
      return ApiResult.error('Network error. Please try again.');
    } catch (e, stackTrace) {
      AppLogger.error('FamilyRepository', 'Error deleting family', error: e, stackTrace: stackTrace);
      return ApiResult.error('An unexpected error occurred');
    }
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
