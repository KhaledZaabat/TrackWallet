// core/network/api_client.dart
import 'package:dio/dio.dart';
import 'package:famxpense/core/storage/local_storage.dart';

class ApiClient {
  late final Dio _dio;
  final LocalStorage _localStorage;

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
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            // Try to refresh token
            final refreshed = await _refreshToken();
            if (refreshed) {
              // Retry the request
              return handler.resolve(await _retry(error.requestOptions));
            }
          }
          return handler.next(error);
        },
      ),
    );
  }

  Future<bool> _refreshToken() async {
    try {
      final refreshToken = await _localStorage.getRefreshToken();
      if (refreshToken == null) return false;

      // Get deviceId and fcmToken if stored
      final deviceId = await _localStorage.getDeviceId();
      final fcmToken = await _localStorage.getFcmToken();

      final response = await _dio.post(
        '/api/identity/refresh',
        data: {
          'refreshToken': refreshToken,
          'deviceId': deviceId,
          'fcmToken': fcmToken,
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
      return false;
    }
  }

  Future<Response<dynamic>> _retry(RequestOptions requestOptions) async {
    final token = await _localStorage.getJwtToken();
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
