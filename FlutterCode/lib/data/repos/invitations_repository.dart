import 'package:dio/dio.dart';
import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/models/Invitations/invitation_model.dart';

/// ApiResult pattern for consistent error handling across repository methods
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

/// Repository for managing family invitations API calls
class InvitationsRepository {
  final ApiClient _apiClient;

  InvitationsRepository(this._apiClient);

  /// Extract error message from DioException response following auth_repository pattern
  String _extractErrorMessage(DioException e) {
    try {
      final detail = e.response?.data['detail'] as String?;
      if (detail != null) return detail;

      final message = e.response?.data['message'] as String?;
      if (message != null) return message;

      return e.message ?? 'An error occurred';
    } catch (_) {
      return e.message ?? 'An error occurred';
    }
  }

  /// Send invitation to user by email
  /// POST /api/invitations
  /// Returns: Invitation object (201 Created)
  Future<ApiResult<Invitation>> sendInvitation({
    required String email,
    required bool isParent,
  }) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/invitations',
        data: {
          'email': email,
          'isParent': isParent,
        },
      );

      if (response.statusCode == 201 || response.statusCode == 200) {
        final invitation = Invitation.fromJson(response.data as Map<String, dynamic>);
        return ApiResult.success(invitation);
      }

      return ApiResult.error('Failed to send invitation: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        final error = _extractErrorMessage(e);
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('User not found with that email');
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Get invitations where current user is recipient (all users can access)
  /// GET /api/invitations/received
  /// Optional status filter: Pending, Accepted, Declined, Cancelled
  /// Returns: List<Invitation>
  Future<ApiResult<List<Invitation>>> getReceivedInvitations({
    String? status,
  }) async {
    try {
      final queryParams = <String, dynamic>{};
      if (status != null && status.isNotEmpty) {
        queryParams['status'] = status;
      }

      print('DEBUG: Fetching received invitations');
      final response = await _apiClient.dio.get(
        '/api/invitations/received',
        queryParameters: queryParams.isNotEmpty ? queryParams : null,
      );
      print('DEBUG: Received response: ${response.statusCode} - ${response.data}');

      if (response.statusCode == 200) {
        final invitations = (response.data as List)
            .map((inv) => Invitation.fromJson(inv as Map<String, dynamic>))
            .toList();
        print('DEBUG: Parsed ${invitations.length} invitations');
        return ApiResult.success(invitations);
      }

      return ApiResult.error('Failed to load received invitations: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Get invitations sent from current family (parents only - 403 if not parent)
  /// GET /api/invitations/sent
  /// Optional status filter: Pending, Accepted, Declined, Cancelled
  /// Returns: List<Invitation>
  Future<ApiResult<List<Invitation>>> getSentInvitations({
    String? status,
  }) async {
    try {
      final queryParams = <String, dynamic>{};
      if (status != null && status.isNotEmpty) {
        queryParams['status'] = status;
      }

      final response = await _apiClient.dio.get(
        '/api/invitations/sent',
        queryParameters: queryParams.isNotEmpty ? queryParams : null,
      );

      if (response.statusCode == 200) {
        final invitations = (response.data as List)
            .map((inv) => Invitation.fromJson(inv as Map<String, dynamic>))
            .toList();
        return ApiResult.success(invitations);
      }

      return ApiResult.error('Failed to load sent invitations: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 403) {
        // Parent-only action - user-friendly error message
        return ApiResult.error('You must be a family parent to view sent invitations');
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Accept a received invitation
  /// POST /api/invitations/{invitationId}/accept
  /// Returns: void (200 OK with no body)
  Future<ApiResult<void>> acceptInvitation(String invitationId) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/invitations/$invitationId/accept',
      );

      if (response.statusCode == 200) {
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to accept invitation: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return ApiResult.error('Invitation not found');
      } else if (e.response?.statusCode == 400) {
        final error = _extractErrorMessage(e);
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Decline a received invitation
  /// POST /api/invitations/{invitationId}/decline
  /// Returns: void (200 OK with no body)
  Future<ApiResult<void>> declineInvitation(String invitationId) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/invitations/$invitationId/decline',
      );

      if (response.statusCode == 200) {
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to decline invitation: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        return ApiResult.error('Invitation not found');
      } else if (e.response?.statusCode == 400) {
        final error = _extractErrorMessage(e);
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }

  /// Cancel a sent invitation (parents only - 403 if not parent)
  /// POST /api/invitations/{invitationId}/cancel
  /// Returns: void (200 OK with no body)
  Future<ApiResult<void>> cancelInvitation(String invitationId) async {
    try {
      final response = await _apiClient.dio.post(
        '/api/invitations/$invitationId/cancel',
      );

      if (response.statusCode == 200) {
        return ApiResult.success(null);
      }

      return ApiResult.error('Failed to cancel invitation: ${response.statusCode}');
    } on DioException catch (e) {
      if (e.response?.statusCode == 403) {
        // Parent-only action - user-friendly error message
        return ApiResult.error('Only family parents can cancel invitations');
      } else if (e.response?.statusCode == 404) {
        return ApiResult.error('Invitation not found');
      } else if (e.response?.statusCode == 400) {
        final error = _extractErrorMessage(e);
        return ApiResult.error(error);
      } else if (e.response?.statusCode == 401) {
        return ApiResult.error('Authentication required');
      }
      return ApiResult.error(_extractErrorMessage(e));
    } catch (e) {
      return ApiResult.error('An unexpected error occurred');
    }
  }
}
