import 'package:famxpense/core/services/device_manager.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_cubit.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';

import 'package:famxpense/core/services/notifications_service.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/app_router.dart';

///  Background FCM handler
@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp();
}

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  await Firebase.initializeApp();

  FirebaseMessaging.onBackgroundMessage(
    _firebaseMessagingBackgroundHandler,
  );

  await NotificationService.initialize();
  await setupDependencyInjection();

  final deviceManager = getIt<DeviceManager>();
  await deviceManager.initializeDevice();
  deviceManager.listenToFcmTokenRefresh();

  NotificationService.setupForegroundNotifications();
  await NotificationService.setupInteractedMessage();

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<AuthCubit>()..checkAuthStatus(),
      child: MaterialApp.router(
        title: 'FamXpense',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
          useMaterial3: true,
        ),
        routerConfig: AppRouter.router,
      ),
    );
  }
}
