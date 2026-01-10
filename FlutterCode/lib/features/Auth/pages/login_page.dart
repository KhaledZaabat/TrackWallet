import 'package:famxpense/core/app_logger.dart';
import 'package:famxpense/core/router/routes.dart';
import 'package:famxpense/features/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/Auth/cubit/auth_state.dart';
import 'package:famxpense/features/Auth/pages/google_sign_in_button.dart';
import 'package:famxpense/features/Auth/pages/login_form.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  static const String _tag = 'LoginPage';
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _handleEmailLogin(BuildContext context) {
    if (_formKey.currentState!.validate()) {
      context.read<AuthCubit>().login(
            identifier: _emailController.text.trim(),
            password: _passwordController.text.trim(),
          );
    }
  }

  void _handleGoogleLogin(BuildContext context) {
    AppLogger.info(_tag, '🔘 Google Sign-In button pressed');
    AppLogger.debug(_tag, 'Current auth state: ${context.read<AuthCubit>().state.runtimeType}');
    context.read<AuthCubit>().loginWithGoogle();
  }

  void _showVerifyAccountDialog(BuildContext context) {
    final emailController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        title: const Text(
          'Verify Account',
          style: TextStyle(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.w700,
            fontSize: 20,
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Enter your email to resend verification code',
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: emailController,
              keyboardType: TextInputType.emailAddress,
              decoration: InputDecoration(
                hintText: "example@email.com",
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.border,
                    width: 1.5,
                  ),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.border,
                    width: 1.5,
                  ),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.primary,
                    width: 1.5,
                  ),
                ),
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 14,
                ),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text(
              'Cancel',
              style: TextStyle(color: Color(0xFF5B6B8C)),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              final email = emailController.text.trim();
              if (email.isNotEmpty) {
                Navigator.pop(dialogContext);
                context.push(Routes.otpVerification, extra: email);
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(6),
              ),
            ),
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }

  void _showForgotPasswordDialog(BuildContext context) {
    final emailController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        title: const Text(
          'Reset Password',
          style: TextStyle(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.w700,
            fontSize: 20,
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Enter your email to receive password reset instructions',
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 16),
            TextField(
              controller: emailController,
              keyboardType: TextInputType.emailAddress,
              decoration: InputDecoration(
                hintText: "example@email.com",
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.border,
                    width: 1.5,
                  ),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.border,
                    width: 1.5,
                  ),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(6),
                  borderSide: const BorderSide(
                    color: AppColors.primary,
                    width: 1.5,
                  ),
                ),
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 14,
                  vertical: 14,
                ),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text(
              'Cancel',
              style: TextStyle(color: Color(0xFF5B6B8C)),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              final email = emailController.text.trim();
              if (email.isNotEmpty) {
                Navigator.pop(dialogContext);
                context.push(Routes.forgotPassword, extra: email);
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(6),
              ),
            ),
            child: const Text('Send Reset Link'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: BlocConsumer<AuthCubit, AuthState>(
        listener: (context, state) {
          AppLogger.info(_tag, '📊 Auth state changed: ${state.runtimeType}');
          
          // Show error messages
          if (state is AuthError) {
            AppLogger.error(_tag, '❌ AuthError received: ${state.message}');
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: AppColors.error,
                behavior: SnackBarBehavior.floating,
                margin: const EdgeInsets.all(16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                duration: const Duration(seconds: 4),
              ),
            );
          }

          // Navigate on successful authentication
          if (state is AuthAuthenticated) {
            AppLogger.success(_tag, '✅ AuthAuthenticated - Navigating to /select-family');
            AppLogger.debug(_tag, 'Authenticated user:', data: {
              'email': state.email,
              'userId': state.userId,
              'familiesCount': state.families.length,
            });
            context.go('/select-family');
          }
          
          if (state is AuthLoading) {
            AppLogger.info(_tag, '⏳ AuthLoading - Waiting for response...');
          }
          
          if (state is AuthUnauthenticated) {
            AppLogger.info(_tag, '🔓 AuthUnauthenticated - User not logged in');
          }
        },
        builder: (context, state) {
          final bool isLoading = state is AuthLoading;

          return SafeArea(
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 32),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    SizedBox(height: size.height * 0.08),

                    // Decorative top border
                    Container(
                      height: 4,
                      margin: const EdgeInsets.symmetric(horizontal: 60),
                      decoration: BoxDecoration(
                        border: Border.all(
                          color: AppColors.border,
                          width: 2,
                        ),
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),

                    const SizedBox(height: 32),

                    // Title
                    const Text(
                      "Good to see you!",
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 34,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textPrimary,
                        letterSpacing: -0.5,
                      ),
                    ),
                    const SizedBox(height: 12),
                    Text(
                      "Let's continue the journey.",
                      textAlign: TextAlign.center,
                      style: TextStyle(
                        fontSize: 16,
                        color: AppColors.textSecondary.withOpacity(0.6),
                        fontWeight: FontWeight.w400,
                      ),
                    ),

                    SizedBox(height: size.height * 0.06),

                    // Login Form
                    if (isLoading)
                      const Center(
                        child: Padding(
                          padding: EdgeInsets.all(32.0),
                          child: CircularProgressIndicator(),
                        ),
                      )
                    else
                      LoginForm(
                        emailController: _emailController,
                        passwordController: _passwordController,
                        formKey: _formKey,
                        onSubmit: () => _handleEmailLogin(context),
                        onForgotPassword: () =>
                            _showForgotPasswordDialog(context),
                        isEnabled: !isLoading,
                      ),

                    const SizedBox(height: 28),

                    // Divider with "OR"
                    Row(
                      children: [
                        Expanded(
                          child: Divider(
                            color: AppColors.border,
                            thickness: 1,
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          child: Text(
                            "OR",
                            style: TextStyle(
                              fontSize: 13,
                              color: AppColors.textSecondary.withOpacity(0.5),
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        Expanded(
                          child: Divider(
                            color: AppColors.border,
                            thickness: 1,
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 28),

                    // Google Sign In button
                    GoogleSignInButton(
                      onPressed: () => _handleGoogleLogin(context),
                      isLoading: isLoading,
                      isEnabled: !isLoading,
                    ),

                    const SizedBox(height: 32),

                    // Sign up text
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          "Don't have an account? ",
                          style: TextStyle(
                            fontSize: 14,
                            color: AppColors.textSecondary.withOpacity(0.7),
                          ),
                        ),
                        GestureDetector(
                          onTap: isLoading
                              ? null
                              : () => context.push(Routes.signup),
                          child: Text(
                            "Sign Up",
                            style: TextStyle(
                              fontSize: 14,
                              color: isLoading
                                  ? AppColors.primary.withOpacity(0.5)
                                  : AppColors.primary,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),

                    // Verify Account text
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text(
                          "Need to verify your account? ",
                          style: TextStyle(
                            fontSize: 14,
                            color: AppColors.textSecondary.withOpacity(0.7),
                          ),
                        ),
                        GestureDetector(
                          onTap: isLoading
                              ? null
                              : () => _showVerifyAccountDialog(context),
                          child: Text(
                            "Verify",
                            style: TextStyle(
                              fontSize: 14,
                              color: isLoading
                                  ? AppColors.primary.withOpacity(0.5)
                                  : AppColors.primary,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 32),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
