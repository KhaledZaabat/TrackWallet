import 'dart:developer' as developer;

import 'package:famxpense/core/di/setup_dependency_injection.dart';
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
import 'package:famxpense/features/auth/presentation/Families/pages/create_family_page.dart';
import 'package:famxpense/features/auth/presentation/Families/pages/select_family_page.dart';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/features/auth/presentation/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transaction_form_page.dart';
import 'package:famxpense/features/auth/presentation/Transactions/Pages/transactions_list_page.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';

class AppRouter {
  static final _rootNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'root');

  static GoRouter createRouter(AuthCubit authCubit) => GoRouter(
        navigatorKey: _rootNavigatorKey,
        initialLocation: '/login',
        redirect: (context, state) async {
          final authState = authCubit.state;
          final currentPath = state.matchedLocation;

          final isAuthRoute = currentPath.startsWith('/login') ||
              currentPath.startsWith('/signup') ||
              currentPath.startsWith('/otp-verification') ||
              currentPath.startsWith('/forgot-password') ||
              currentPath.startsWith('/reset-password');

          // If authenticated
          if (authState is AuthAuthenticated) {
            if (isAuthRoute) {
              final selectedFamilyId =
                  await getIt<LocalStorage>().getSelectedFamilyId();
              return selectedFamilyId != null
                  ? '/transactions'
                  : '/select-family';
            }
            return null;
          }

          // If not authenticated
          if (authState is AuthUnauthenticated) {
            if (!isAuthRoute) {
              return '/login';
            }
            return null;
          }

          return null;
        },
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
            builder: (context, state) => const SelectFamilyPage(),
          ),
          GoRoute(
            path: '/create-family',
            builder: (context, state) => const CreateFamilyPage(),
          ),
          GoRoute(
            path: '/dashboard',
            builder: (context, state) => const DashboardPage(),
          ),
          GoRoute(
            path: '/transactions',
            builder: (context, state) => BlocProvider(
              create: (_) => getIt<TransactionCubit>()..loadTransactions(),
              child: const TransactionsListPage(),
            ),
          ),
          GoRoute(
            path: '/transactions/add',
            builder: (context, state) => BlocProvider(
              create: (_) => getIt<TransactionCubit>(),
              child: const TransactionFormPage(),
            ),
          ),
          GoRoute(
            path: '/transactions/edit',
            builder: (context, state) {
              final transaction = state.extra as TransactionItem;
              return BlocProvider(
                create: (_) => getIt<TransactionCubit>(),
                child: TransactionFormPage(existingTransaction: transaction),
              );
            },
          ),
        ],
      );
}
