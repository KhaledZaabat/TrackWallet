import 'package:famxpense/common/widgets/scaffold_with_nested_navigation.dart';
import 'package:famxpense/core/di/setup_dependency_injection.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/core/storage/local_storage.dart';
import 'package:famxpense/features/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/Auth/cubit/auth_state.dart';
import 'package:famxpense/features/Auth/cubit/reset_password_cubit.dart';
import 'package:famxpense/features/Auth/cubit/signup_cubit.dart';
import 'package:famxpense/features/Auth/pages/forgot_password_page.dart';
import 'package:famxpense/features/Auth/pages/login_page.dart';
import 'package:famxpense/features/Auth/pages/new_password_page.dart';
import 'package:famxpense/features/Auth/pages/otp_verification_page.dart';
import 'package:famxpense/features/Auth/pages/reset_password_otp_page.dart';
import 'package:famxpense/features/Auth/pages/signup_page.dart';
import 'package:famxpense/features/Dashboard/page/dashboard_page.dart';
import 'package:famxpense/features/Families/pages/create_family_page.dart';
import 'package:famxpense/features/Families/pages/select_family_page.dart';
import 'package:famxpense/features/Profile/Cubits/profile_cubit.dart';
import 'package:famxpense/features/Profile/pages/profile_page.dart';
import 'package:famxpense/features/Settings/Cubits/settings_cubit.dart';
import 'package:famxpense/features/Settings/pages/settings_page.dart';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import 'package:famxpense/features/Transactions/Cubits/transaction_cubit.dart';
import 'package:famxpense/features/Transactions/Pages/transaction_form_page.dart';
import 'package:famxpense/features/Transactions/Pages/transactions_list_page.dart';
import 'package:famxpense/models/Transactions/transaction_models.dart';
import 'package:famxpense/features/Invitations/cubit/invitations_cubit.dart';
import 'package:famxpense/features/Invitations/pages/invitations_page.dart';
import 'package:famxpense/features/Invitations/pages/invitations_to_join_page.dart';
import 'package:famxpense/features/MyFamily/pages/my_family_page.dart';

class AppRouter {
  static final _rootNavigatorKey =
      GlobalKey<NavigatorState>(debugLabel: 'root');

  static GoRouter createRouter(AuthCubit authCubit) => GoRouter(
        navigatorKey: _rootNavigatorKey,
        initialLocation: Routes.login,
        redirect: (context, state) async {
          final authState = authCubit.state;
          final currentPath = state.matchedLocation;

          final isAuthRoute = currentPath.startsWith(Routes.login) ||
              currentPath.startsWith(Routes.signup) ||
              currentPath.startsWith(Routes.otpVerification) ||
              currentPath.startsWith(Routes.forgotPassword) ||
              currentPath.startsWith(Routes.resetPasswordOtp) ||
              currentPath.startsWith(Routes.resetPasswordNew);

          final isFamilyRoute = currentPath.startsWith(Routes.selectFamily) ||
              currentPath.startsWith(Routes.createFamily);

          final requiresFamilySelection = currentPath.startsWith(Routes.dashboard) ||
              currentPath.startsWith(Routes.transactions) ||
              currentPath.startsWith(Routes.settings) ||
              currentPath.startsWith(Routes.myFamily) ||
              currentPath.startsWith(Routes.profile);

          // If authenticated
          if (authState is AuthAuthenticated) {
            // If on auth route, redirect to dashboard or selectFamily
            if (isAuthRoute) {
              final selectedFamilyId =
                  await getIt<LocalStorage>().getSelectedFamilyId();
              return selectedFamilyId != null
                  ? Routes.dashboard
                  : Routes.selectFamily;
            }

            // If route requires family selection, check if family is selected
            if (requiresFamilySelection) {
              final selectedFamilyId =
                  await getIt<LocalStorage>().getSelectedFamilyId();
              // If no family selected, redirect to selectFamily
              if (selectedFamilyId == null || selectedFamilyId.isEmpty) {
                return Routes.selectFamily;
              }
            }

            return null;
          }

          // If not authenticated
          if (authState is AuthUnauthenticated) {
            if (!isAuthRoute) {
              return Routes.login;
            }
            return null;
          }

          return null;
        },
        routes: [
          // Auth Routes
          GoRoute(
            path: Routes.login,
            builder: (context, state) => const LoginPage(),
          ),
          GoRoute(
            path: Routes.signup,
            builder: (context, state) => BlocProvider(
              create: (_) => getIt<SignupCubit>(),
              child: const SignupPage(),
            ),
          ),
          GoRoute(
            path: Routes.otpVerification,
            builder: (context, state) {
              final email = state.extra as String;
              return BlocProvider(
                create: (_) => getIt<SignupCubit>(),
                child: OtpVerificationPage(email: email),
              );
            },
          ),
          GoRoute(
            path: Routes.forgotPassword,
            builder: (context, state) => BlocProvider(
              create: (_) => getIt<ResetPasswordCubit>(),
              child: const ForgotPasswordPage(),
            ),
          ),
          GoRoute(
            path: Routes.resetPasswordOtp,
            builder: (context, state) {
              final email = state.extra as String;
              return BlocProvider(
                create: (_) => getIt<ResetPasswordCubit>(),
                child: ResetPasswordOtpPage(email: email),
              );
            },
          ),
          GoRoute(
            path: Routes.resetPasswordNew,
            builder: (context, state) {
              final email = state.extra as String;
              return BlocProvider(
                create: (_) => getIt<ResetPasswordCubit>(),
                child: NewPasswordPage(email: email),
              );
            },
          ),

          // Family Routes (outside shell - though SelectFamily is also used in NoFamily shell, wait.
          // In "No Family" mode, navigation is: SelectFamily <-> Invitations.
          // In "Family" mode, navigation is: Dashboard <-> Invitations <-> Transactions <-> MyFamily <-> Settings.
          //
          // We can put SelectFamily in the shell too, as a branch.
          
          GoRoute(
            path: Routes.createFamily,
            builder: (context, state) => const CreateFamilyPage(),
          ),
          
          GoRoute(
            path: Routes.manageFamilies,
            parentNavigatorKey: _rootNavigatorKey,
            builder: (context, state) => const SelectFamilyPage(),
          ),

          // Stateful Shell Route
          StatefulShellRoute.indexedStack(
            builder: (context, state, navigationShell) {
              return ScaffoldWithNestedNavigation(
                  navigationShell: navigationShell);
            },
            branches: [
              // Branch 0: Dashboard
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.dashboard,
                    builder: (context, state) => const DashboardPage(),
                  ),
                ],
              ),
              // Branch 1: Invitations (Shared)
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.invitations,
                    builder: (context, state) => BlocProvider.value(
                      value: getIt<InvitationsCubit>(),
                      child: const InvitationsPage(),
                    ),
                    routes: [
                      // Sub-routes if any
                      GoRoute(
                        path: 'to-join', // relative path
                        // matches /invitations/to-join
                        name: 'invitationsToJoin',
                        parentNavigatorKey: _rootNavigatorKey,
                        builder: (context, state) =>
                            const InvitationsToJoinPage(),
                      ),
                    ],
                  ),
                ],
              ),
              // Branch 2: Transactions
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.transactions,
                    builder: (context, state) => BlocProvider.value(
                      value:
                          getIt<TransactionCubit>()..loadTransactions(),
                      child: const TransactionsListPage(),
                    ),
                     routes: [
                       GoRoute(
                        path: 'add',
                        parentNavigatorKey: _rootNavigatorKey,
                        builder: (context, state) => BlocProvider.value(
                          value: getIt<TransactionCubit>(),
                          child: const TransactionFormPage(),
                        ),
                      ),
                      GoRoute(
                        path: 'edit',
                        parentNavigatorKey: _rootNavigatorKey,
                        builder: (context, state) {
                          final transaction = state.extra as TransactionItem;
                          return BlocProvider.value(
                            value: getIt<TransactionCubit>(),
                            child: TransactionFormPage(
                                existingTransaction: transaction),
                          );
                        },
                      ),
                     ]
                  ),
                ],
              ),
              // Branch 3: MyFamily
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.myFamily,
                    builder: (context, state) => const MyFamilyPage(),
                  ),
                ],
              ),
              // Branch 4: Settings
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.settings,
                    builder: (context, state) => BlocProvider.value(
                      value: getIt<SettingsCubit>(),
                      child: const SettingsPage(),
                    ),
                  ),
                ],
              ),
               // Branch 5: Select Family
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.selectFamily,
                    builder: (context, state) => const SelectFamilyPage(),
                  ),
                ],
              ),
               // Branch 6: Invitations (Guest/No-Family Mode)
              StatefulShellBranch(
                routes: [
                  GoRoute(
                    path: Routes.invitationsGuest,
                    builder: (context, state) => BlocProvider.value(
                      value: getIt<InvitationsCubit>(),
                      child: const InvitationsPage(forceGuestMode: true),
                    ),
                  ),
                ],
              ),
            ],
          ),
          
          // Profile as a standalone route or inside Settings shell usually?
          // Keeping it standalone for now as previous impl
           GoRoute(
            path: Routes.profile,
            builder: (context, state) => BlocProvider.value(
              value: getIt<ProfileCubit>(),
              child: const ProfilePage(),
            ),
          ),
        ],
      );
}
