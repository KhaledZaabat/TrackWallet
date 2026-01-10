// core/network/api_client.dart
import 'package:dio/dio.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/storage/local_storage.dart';

class ApiClient {
  late final Dio _dio;
  final LocalStorage _localStorage;

  // Add these to prevent multiple simultaneous refresh attempts
  bool _isRefreshing = false;
  final List<Function> _requestQueue = [];

  ApiClient(this._localStorage) {
    _dio = Dio(BaseOptions(
      baseUrl: 'https://track.wallet.quizflow.online',
      connectTimeout: const Duration(seconds: 30),
      receiveTimeout: const Duration(seconds: 30),
    ));

    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _localStorage.getJwtToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
            AppLogger.info('ApiClient', 'Request to ${options.path} with token (${token.length} chars)');
          } else {
            AppLogger.info('ApiClient', 'Request to ${options.path} WITHOUT token');
          }
          return handler.next(options);
        },

        onError: (error, handler) async {
          final isRefreshCall =
              error.requestOptions.path.contains('/api/identity/refresh');

          if ((error.response?.statusCode == 401 || error.response?.statusCode == 403)&& !isRefreshCall) {
            // If already refreshing, queue this request
            if (_isRefreshing) {
              // Wait for refresh to complete, then retry
              await _waitForRefresh();
              try {
                final response = await _retry(error.requestOptions);
                return handler.resolve(response);
              } catch (e) {
                return handler.next(error);
              }
            }

            // Try to refresh token
            final refreshed = await _refreshToken();
            if (refreshed) {
              try {
                final response = await _retry(error.requestOptions);
                return handler.resolve(response);
              } catch (e) {
                return handler.next(error);
              }
            } else {
              // Refresh failed - clear tokens and reject
              await _localStorage.clearAuthTokens();
              return handler.next(error);
            }
          }

          return handler.next(error);
        },
      ),
    );
  }

  Future<void> _waitForRefresh() async {
    // Poll until refresh is complete
    while (_isRefreshing) {
      await Future.delayed(const Duration(milliseconds: 100));
    }
  }

  Future<bool> _refreshToken() async {
    if (_isRefreshing) return false;

    _isRefreshing = true;
    try {
      final refreshToken = await _localStorage.getRefreshToken();
      if (refreshToken == null) {
        return false;
      }

      // Get deviceId and fcmToken if stored
      final deviceId = await _localStorage.getDeviceId();
      final fcmToken = await _localStorage.getFcmToken();

      // Create a new Dio instance without interceptors for refresh call
      final refreshDio = Dio(BaseOptions(
        baseUrl: _dio.options.baseUrl,
        connectTimeout: _dio.options.connectTimeout,
        receiveTimeout: _dio.options.receiveTimeout,
      ));

      final response = await refreshDio.post(
        '/api/identity/refresh',
        data: {
          'refreshToken': refreshToken,
          if (deviceId != null) 'deviceId': deviceId,
          if (fcmToken != null) 'fcmToken': fcmToken,
        },
      );

      if (response.statusCode == 200) {
        final data = response.data;
        await _localStorage.saveAuthTokens(
          data['jwtToken']['token'],
          data['refreshToken']['token'],
        );
        return true;
      }
      return false;
    } catch (e) {
      // Clear tokens on refresh failure
      await _localStorage.clearAuthTokens();
      return false;
    } finally {
      _isRefreshing = false;
    }
  }

  Future<Response<dynamic>> _retry(RequestOptions requestOptions) async {
    final token = await _localStorage.getJwtToken();
    if (token == null) {
      throw DioException(
        requestOptions: requestOptions,
        error: 'No token available',
      );
    }

    final options = Options(
      method: requestOptions.method,
      headers: {
        ...requestOptions.headers,
        'Authorization': 'Bearer $token',
      },
    );

    return _dio.request<dynamic>(
      requestOptions.path,
      data: requestOptions.data,
      queryParameters: requestOptions.queryParameters,
      options: options,
    );
  }

  Dio get dio => _dio;
}
