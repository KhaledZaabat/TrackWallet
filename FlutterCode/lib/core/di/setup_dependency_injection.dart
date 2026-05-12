import 'package:famxpense/core/Network/ApiClient.dart';
import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/services/category_service.dart';
import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/core/services/google_sign_in_service.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/data/repos/auth_repository.dart';
import 'package:famxpense/data/repos/dashboard_repo.dart';
import 'package:famxpense/data/repos/family_repository.dart';
import 'package:famxpense/data/repos/invitations_repository.dart';
import 'package:famxpense/data/repos/transaction_repository.dart';
import 'package:famxpense/data/repos/user_repository.dart';
import 'package:famxpense/features/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/Auth/cubit/reset_password_cubit.dart';
import 'package:famxpense/features/Auth/cubit/signup_cubit.dart';
import 'package:famxpense/features/Dashboard/cubit/dashboard_cubit.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/MyFamily/cubit/my_family_cubit.dart';
import 'package:famxpense/features/Families/Cubits/create_family_cubit.dart';
import 'package:famxpense/features/Families/Cubits/select_family_cubit.dart';
import 'package:famxpense/features/Profile/Cubits/profile_cubit.dart';
import 'package:famxpense/features/Settings/Cubits/settings_cubit.dart';
import 'package:famxpense/features/Settings/Cubits/locale_cubit.dart';
import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';

import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:get_it/get_it.dart';

final getIt = GetIt.instance;

Future<void> setupDependencyInjection() async {
  AppLogger.info('DI', 'Starting dependency injection setup...');

  getIt.registerLazySingleton<LocalStorage>(
    () => LocalStorage.standard(),
  );

  getIt.registerLazySingleton<FirebaseMessaging>(
    () => FirebaseMessaging.instance,
  );

  getIt.registerLazySingleton<DeviceManager>(
    () => DeviceManager(
      getIt<LocalStorage>(),
      getIt<FirebaseMessaging>(),
    ),
  );

  AppLogger.info('DI', 'Initializing device manager...');
  await getIt<DeviceManager>().initializeDevice();

  getIt.registerLazySingleton<ApiClient>(
    () => ApiClient(getIt<LocalStorage>()),
  );

  getIt<DeviceManager>().setApiClient(getIt<ApiClient>());

  getIt<DeviceManager>().listenToFcmTokenRefresh();

  getIt.registerLazySingleton<GoogleSignInService>(
    () => GoogleSignInService.production(),
  );
  AppLogger.info('DI', 'Google Sign-In service registered');

  getIt.registerLazySingleton<CategoryService>(
    () => CategoryService(getIt<ApiClient>()),
  );

  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepository(
      getIt<ApiClient>(),
      getIt<LocalStorage>(),
      getIt<DeviceManager>(),
    ),
  );

  getIt.registerLazySingleton<FamilyRepository>(
    () => FamilyRepository(
      getIt<ApiClient>(),
      getIt<LocalStorage>(),
      getIt<DeviceManager>(),
    ),
  );

  getIt.registerLazySingleton<DashboardRepository>(
    () => DashboardRepository(getIt<ApiClient>()),
  );

  getIt.registerLazySingleton<TransactionRepository>(
    () => TransactionRepository(getIt<ApiClient>()),
  );

  getIt.registerLazySingleton<UserRepository>(
    () => UserRepository(getIt<ApiClient>()),
  );

  getIt.registerLazySingleton<InvitationsRepository>(
    () => InvitationsRepository(getIt<ApiClient>()),
  );

  // Updated to include GoogleSignInService
  getIt.registerLazySingleton<AuthCubit>(
    () => AuthCubit(
      getIt<AuthRepository>(),
      getIt<GoogleSignInService>(),
    ),
  );

  getIt.registerLazySingleton<DashboardCubit>(
    () => DashboardCubit(getIt<DashboardRepository>()),
  );

  getIt.registerLazySingleton<InvitationsCubit>(
    () => InvitationsCubit(getIt<InvitationsRepository>()),
  );

  getIt.registerLazySingleton<MyFamilyCubit>(
    () => MyFamilyCubit(getIt<FamilyRepository>(), getIt<LocalStorage>(), getIt<DashboardCubit>()),
  );

  getIt.registerLazySingleton<TransactionCubit>(
    () => TransactionCubit(getIt<TransactionRepository>()),
  );

  getIt.registerLazySingleton<ProfileCubit>(
    () => ProfileCubit(getIt<UserRepository>(), getIt<DashboardCubit>(), getIt<MyFamilyCubit>()),
  );

  getIt.registerLazySingleton<SettingsCubit>(
    () => SettingsCubit(getIt<UserRepository>()),
  );

  getIt.registerLazySingleton<LocaleCubit>(
    () => LocaleCubit(),
  );

  getIt.registerFactory<SignupCubit>(
    () => SignupCubit(getIt<AuthRepository>()),
  );

  getIt.registerFactory<ResetPasswordCubit>(
    () => ResetPasswordCubit(getIt<AuthRepository>()),
  );

  getIt.registerFactory<SelectFamilyCubit>(
    () => SelectFamilyCubit(getIt<FamilyRepository>()),
  );

  getIt.registerFactory<CreateFamilyCubit>(
    () => CreateFamilyCubit(getIt<FamilyRepository>()),
  );

  AppLogger.info('DI', 'Dependency injection setup complete!');
}

Future<void> initializeCategories() async {
  try {
    AppLogger.info('DI', 'Initializing categories...');
    final categoryService = getIt<CategoryService>();

    if (!categoryService.isInitialized) {
      await categoryService.initialize();
      AppLogger.info('DI', 'Categories initialized successfully');
    } else {
      AppLogger.info('DI', 'Categories already initialized');
    }
  } catch (e, stackTrace) {
    AppLogger.error('DI', 'Failed to initialize categories',
        error: e, stackTrace: stackTrace);
  }
}
