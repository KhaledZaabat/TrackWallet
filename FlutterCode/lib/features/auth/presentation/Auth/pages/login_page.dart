import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_cubit.dart';
import 'package:famxpense/features/auth/presentation/Auth/cubit/auth_state.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/validation_patterns.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  void _showVerifyAccountDialog(BuildContext context) {
    final emailController = TextEditingController();

    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text(
          'Verify Account',
          style: TextStyle(
            color: Color(0xFF5B6B8C),
            fontWeight: FontWeight.w700,
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Enter your email to resend verification code',
              style: TextStyle(
                fontSize: 14,
                color: Color(0xFF5B6B8C),
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
                ),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              final email = emailController.text.trim();
              if (email.isNotEmpty) {
                Navigator.pop(dialogContext);
                context.push('/otp-verification', extra: email);
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF5B7CB5),
            ),
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    return Scaffold(
      backgroundColor: Colors.white,
      body: BlocConsumer<AuthCubit, AuthState>(
        listener: (context, state) {
          if (state is AuthError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          }
          if (state is AuthAuthenticated) {
            final familiesCount = state.families.length;

            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                duration: const Duration(seconds: 5),
                backgroundColor: Colors.green,
                content: Text(
                  '''
Logged in successfully 🎉

User ID: ${state.userId}
Name: ${state.fullName}
Email: ${state.email}
Families: $familiesCount
        ''',
                ),
              ),
            );

            //  context.go(Routes.createFamily);
          }
        },
        builder: (context, state) {
          final bool loading = state is AuthLoading;

          return SafeArea(
            child: SingleChildScrollView(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 32),
                child: Form(
                  key: _formKey,
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
                            color: const Color(0xFFE0E5EB),
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
                          color: Color(0xFF5B6B8C),
                          letterSpacing: -0.5,
                        ),
                      ),
                      const SizedBox(height: 12),
                      Text(
                        "Let's continue the journey.",
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          fontSize: 16,
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.6),
                          fontWeight: FontWeight.w400,
                        ),
                      ),

                      SizedBox(height: size.height * 0.06),

                      // Email label
                      Text(
                        "Email",
                        style: TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.8),
                        ),
                      ),
                      const SizedBox(height: 10),

                      // Email field
                      TextFormField(
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        validator: ValidationPatterns.validateEmail,
                        enabled: !loading,
                        decoration: InputDecoration(
                          hintText: "example@email.com",
                          hintStyle: TextStyle(
                            color:
                                const Color(0xFF5B6B8C).withValues(alpha: 0.3),
                            fontSize: 15,
                          ),
                          filled: true,
                          fillColor: Colors.white,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFFE0E5EB),
                              width: 1.5,
                            ),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFFE0E5EB),
                              width: 1.5,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFF5B7CB5),
                              width: 1.5,
                            ),
                          ),
                          errorBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Colors.red,
                              width: 1.5,
                            ),
                          ),
                          focusedErrorBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Colors.red,
                              width: 1.5,
                            ),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 18,
                          ),
                          errorStyle: const TextStyle(fontSize: 12),
                        ),
                      ),
                      const SizedBox(height: 24),

                      // Password label
                      Text(
                        "Password",
                        style: TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w600,
                          color: const Color(0xFF5B6B8C).withValues(alpha: 0.8),
                        ),
                      ),
                      const SizedBox(height: 10),

                      // Password field
                      TextFormField(
                        controller: _passwordController,
                        obscureText: _obscurePassword,
                        validator: ValidationPatterns.validatePassword,
                        enabled: !loading,
                        decoration: InputDecoration(
                          hintText: "************",
                          hintStyle: TextStyle(
                            color:
                                const Color(0xFF5B6B8C).withValues(alpha: 0.3),
                            fontSize: 15,
                          ),
                          filled: true,
                          fillColor: Colors.white,
                          suffixIcon: IconButton(
                            icon: Icon(
                              _obscurePassword
                                  ? Icons.visibility_off
                                  : Icons.visibility,
                              color: const Color(0xFF5B6B8C)
                                  .withValues(alpha: 0.5),
                            ),
                            onPressed: () {
                              setState(() {
                                _obscurePassword = !_obscurePassword;
                              });
                            },
                          ),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFFE0E5EB),
                              width: 1.5,
                            ),
                          ),
                          enabledBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFFE0E5EB),
                              width: 1.5,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Color(0xFF5B7CB5),
                              width: 1.5,
                            ),
                          ),
                          errorBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Colors.red,
                              width: 1.5,
                            ),
                          ),
                          focusedErrorBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(6),
                            borderSide: const BorderSide(
                              color: Colors.red,
                              width: 1.5,
                            ),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 18,
                          ),
                          errorStyle: const TextStyle(fontSize: 12),
                        ),
                      ),
                      Align(
                        alignment: Alignment.centerRight,
                        child: GestureDetector(
                          onTap: () => context.push('/forgot-password'),
                          child: const Text(
                            "Forgot Password?",
                            style: TextStyle(
                              fontSize: 14,
                              color: Color(0xFF5B7CB5),
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 32),
                      SizedBox(height: size.height * 0.05),

                      // Social row
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const SizedBox(width: 24),
                          _SocialCircle(
                            icon: Icons.g_mobiledata,
                            onTap: loading
                                ? null
                                : () {
                                    // TODO: Implement Google login
                                  },
                          ),
                        ],
                      ),

                      SizedBox(height: size.height * 0.05),

                      // Log In button
                      SizedBox(
                        height: 54,
                        child: ElevatedButton(
                          onPressed: loading
                              ? null
                              : () {
                                  if (_formKey.currentState!.validate()) {
                                    context.read<AuthCubit>().login(
                                          identifier:
                                              _emailController.text.trim(),
                                          password:
                                              _passwordController.text.trim(),
                                        );
                                  }
                                },
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF5B7CB5),
                            foregroundColor: Colors.white,
                            elevation: 0,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(6),
                            ),
                            disabledBackgroundColor:
                                const Color(0xFF5B7CB5).withValues(alpha: 0.6),
                          ),
                          child: loading
                              ? const SizedBox(
                                  width: 22,
                                  height: 22,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2.5,
                                    valueColor: AlwaysStoppedAnimation<Color>(
                                      Colors.white,
                                    ),
                                  ),
                                )
                              : const Text(
                                  "Log In",
                                  style: TextStyle(
                                    fontSize: 17,
                                    fontWeight: FontWeight.w600,
                                    letterSpacing: 0.3,
                                  ),
                                ),
                        ),
                      ),

                      const SizedBox(height: 24),

                      // Sign up text
                      Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            "Don't have an account? ",
                            style: TextStyle(
                              fontSize: 14,
                              color: const Color(0xFF5B6B8C)
                                  .withValues(alpha: 0.7),
                            ),
                          ),
                          GestureDetector(
                            onTap:
                                loading ? null : () => context.push('/signup'),
                            child: const Text(
                              "Sign Up",
                              style: TextStyle(
                                fontSize: 14,
                                color: Color(0xFF5B7CB5),
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
                              color: const Color(0xFF5B6B8C)
                                  .withValues(alpha: 0.7),
                            ),
                          ),
                          GestureDetector(
                            onTap: loading
                                ? null
                                : () {
                                    // Show dialog to enter email for OTP verification
                                    _showVerifyAccountDialog(context);
                                  },
                            child: const Text(
                              "Verify Account",
                              style: TextStyle(
                                fontSize: 14,
                                color: Color(0xFF5B7CB5),
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
            ),
          );
        },
      ),
    );
  }
}

class _SocialCircle extends StatelessWidget {
  final IconData icon;
  final VoidCallback? onTap;

  const _SocialCircle({
    required this.icon,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      customBorder: const CircleBorder(),
      child: Container(
        width: 52,
        height: 52,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          border: Border.all(
            color: const Color(0xFFD0D7E3),
            width: 1.5,
          ),
        ),
        child: Icon(
          icon,
          size: 26,
          color: onTap == null
              ? const Color(0xFF5B6B8C).withValues(alpha: 0.3)
              : const Color(0xFF5B6B8C),
        ),
      ),
    );
  }
}
