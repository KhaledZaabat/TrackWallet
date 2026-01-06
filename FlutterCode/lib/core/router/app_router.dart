import 'dart:async';
import 'dart:developer' as developer;

import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/splash_screen.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_state.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/reset_password_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/signup_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/forgot_password_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/login_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/new_password_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/otp_verification_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/reset_password_otp_page.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/signup_page.dart';
import 'package:famxpense/features/auth/presentation/Dashboard/page/dashboard_page.dart';
import 'package:famxpense/features/auth/presentation/Families/pages/select_family_page.dart';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class AppRouter {
  static final _rootNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'root');

  static GoRouter createRouter(AuthCubit authCubit) => GoRouter(
        navigatorKey: _rootNavigatorKey,
        initialLocation: '/splash',
        refreshListenable: GoRouterRefreshStream(authCubit.stream),
        redirect: (context, state) async {
          final authState = authCubit.state;
          final currentPath = state.matchedLocation;

          // Always allow splash screen to show ONLY during initial app load
          if (currentPath == '/splash') {
            // If still checking or initial, stay on splash
            if (authState is AuthInitial || authState is AuthChecking) {
              return null;
            }

            // Auth check complete, redirect based on result
            if (authState is AuthAuthenticated) {
              final selectedFamilyId =
                  await getIt<LocalStorage>().getSelectedFamilyId();
              final destination =
                  selectedFamilyId != null ? '/dashboard' : '/select-family';

              return destination;
            }

            if (authState is AuthUnauthenticated) {
              return '/login';
            }
          }

          // ===== If authenticated =====
          if (authState is AuthAuthenticated) {
            // Prevent authenticated users from accessing auth pages
            final isAuthRoute = currentPath.startsWith('/login') ||
                currentPath.startsWith('/signup') ||
                currentPath.startsWith('/otp-verification') ||
                currentPath.startsWith('/forgot-password') ||
                currentPath.startsWith('/reset-password');

            if (isAuthRoute) {
              final selectedFamilyId =
                  await getIt<LocalStorage>().getSelectedFamilyId();
              final destination =
                  selectedFamilyId != null ? '/dashboard' : '/select-family';

              return destination;
            }

            return null; // Allow navigation to authenticated routes
          }

          // ===== If unauthenticated =====
          if (authState is AuthUnauthenticated) {
            // Allow access to auth routes
            final isAuthRoute = currentPath.startsWith('/login') ||
                currentPath.startsWith('/signup') ||
                currentPath.startsWith('/otp-verification') ||
                currentPath.startsWith('/forgot-password') ||
                currentPath.startsWith('/reset-password');

            if (isAuthRoute) {
              return null;
            }

            return '/login';
          }

          // If AuthLoading or AuthError, stay on current page
          // This prevents redirects during login/logout operations
          if (authState is AuthLoading || authState is AuthError) {
            return null;
          }

          return '/splash';
        },
        routes: [
          GoRoute(
            path: '/splash',
            builder: (context, state) {
              developer.log('📱 Building splash screen', name: 'AppRouter');
              return const SplashScreen();
            },
          ),
          GoRoute(
            path: '/login',
            builder: (context, state) {
              developer.log('📱 Building login page', name: 'AppRouter');
              return const LoginPage();
            },
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
          GoRoute(
            path: '/select-family',
            builder: (context, state) {
              developer.log('📱 Building select family page',
                  name: 'AppRouter');
              return const SelectFamilyPage();
            },
          ),
          GoRoute(
            path: '/dashboard',
            builder: (context, state) {
              return const DashboardPage();
            },
          ),
        ],
      );
}

// Helper class to refresh GoRouter when auth state changes
class GoRouterRefreshStream extends ChangeNotifier {
  GoRouterRefreshStream(Stream<dynamic> stream) {
    notifyListeners();
    _subscription = stream.asBroadcastStream().listen(
      (dynamic state) {
        notifyListeners();
      },
    );
  }

  late final StreamSubscription<dynamic> _subscription;

  @override
  void dispose() {
    _subscription.cancel();
    super.dispose();
  }
}
