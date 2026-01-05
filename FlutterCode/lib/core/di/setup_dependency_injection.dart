// core/di/dependency_injection.dart

import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/data/database/repositories/concrete/auth_repository.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/reset_password_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/signup_cubit.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:get_it/get_it.dart';

final getIt = GetIt.instance;

Future<void> setupDependencyInjection() async {
  // ========== Core Services ==========

  // LocalStorage (Singleton)
  getIt.registerLazySingleton<LocalStorage>(
    () => LocalStorage.standard(),
  );

  // Firebase Messaging (Singleton)
  getIt.registerLazySingleton<FirebaseMessaging>(
    () => FirebaseMessaging.instance,
  );

  // Device Manager (Singleton)
  getIt.registerLazySingleton<DeviceManager>(
    () => DeviceManager(
      getIt<LocalStorage>(),
      getIt<FirebaseMessaging>(),
    ),
  );

  // Initialize device and get device info
  await getIt<DeviceManager>().initializeDevice();

  // API Client (Singleton)
  getIt.registerLazySingleton<ApiClient>(
    () => ApiClient(getIt<LocalStorage>()),
  );

  // Set API client in device manager (for backend sync)
  getIt<DeviceManager>().setApiClient(getIt<ApiClient>());

  // Start listening to FCM token refresh
  getIt<DeviceManager>().listenToFcmTokenRefresh();

  // ========== Repositories ==========

  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepository(
      getIt<ApiClient>(),
      getIt<LocalStorage>(),
      getIt<DeviceManager>(),
    ),
  );

  // ========== Cubits (Factory - new instance each time) ==========

  getIt.registerFactory<AuthCubit>(
    () => AuthCubit(getIt<AuthRepository>()),
  );

  getIt.registerFactory<SignupCubit>(
    () => SignupCubit(getIt<AuthRepository>()),
  );

  getIt.registerFactory<ResetPasswordCubit>(
    () => ResetPasswordCubit(getIt<AuthRepository>()),
  );
}
