import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/reset_password_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/signup_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/forgot_password_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/login_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/new_password_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/otp_verification_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/reset_password_otp_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/signup_page.dart';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class AppRouter {
  static final _rootNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'root');

  static final GoRouter router = GoRouter(
    navigatorKey: _rootNavigatorKey,
    initialLocation: '/login',
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: '/signup',
        builder: (context, state) => BlocProvider(
          create: (_) => getIt<SignupCubit>(),
          child: const SignupPage(),
        ),
      ),
      GoRoute(
        path: '/otp-verification',
        builder: (context, state) {
          final email = state.extra as String;
          return BlocProvider(
            create: (_) => getIt<SignupCubit>(),
            child: OtpVerificationPage(email: email),
          );
        },
      ),

      // ========== Password Reset Routes ==========
      GoRoute(
        path: '/forgot-password',
        builder: (context, state) => BlocProvider(
          create: (_) => getIt<ResetPasswordCubit>(),
          child: const ForgotPasswordPage(),
        ),
      ),
      GoRoute(
        path: '/reset-password-otp',
        builder: (context, state) {
          final email = state.extra as String;
          return BlocProvider(
            create: (_) => getIt<ResetPasswordCubit>(),
            child: ResetPasswordOtpPage(email: email),
          );
        },
      ),
      GoRoute(
        path: '/reset-password-new',
        builder: (context, state) {
          final email = state.extra as String;
          return BlocProvider(
            create: (_) => getIt<ResetPasswordCubit>(),
            child: NewPasswordPage(email: email),
          );
        },
      ),
    ],
  );
}
