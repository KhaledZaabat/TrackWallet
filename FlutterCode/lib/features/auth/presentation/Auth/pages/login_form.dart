import 'package:flutter/material.dart';
import 'package:famxpense/features/auth/presentation/Auth/pages/validation_patterns.dart';

/// Reusable login form widget containing email and password fields
class LoginForm extends StatefulWidget {
  final TextEditingController emailController;
  final TextEditingController passwordController;
  final GlobalKey<FormState> formKey;
  final VoidCallback onSubmit;
  final VoidCallback onForgotPassword;
  final bool isEnabled;

  const LoginForm({
    Key? key,
    required this.emailController,
    required this.passwordController,
    required this.formKey,
    required this.onSubmit,
    required this.onForgotPassword,
    this.isEnabled = true,
  }) : super(key: key);

  @override
  State<LoginForm> createState() => _LoginFormState();
}

class _LoginFormState extends State<LoginForm> {
  bool _obscurePassword = true;

  @override
  Widget build(BuildContext context) {
    return Form(
      key: widget.formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          // Email label
          Text(
            "Email",
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: const Color(0xFF5B6B8C).withOpacity(0.8),
            ),
          ),
          const SizedBox(height: 8),

          // Email field
          TextFormField(
            controller: widget.emailController,
            keyboardType: TextInputType.emailAddress,
            validator: ValidationPatterns.validateEmail,
            enabled: widget.isEnabled,
            style: const TextStyle(fontSize: 15),
            decoration: _buildInputDecoration(
              hintText: "example@email.com",
            ),
          ),
          const SizedBox(height: 20),

          // Password label
          Text(
            "Password",
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: const Color(0xFF5B6B8C).withOpacity(0.8),
            ),
          ),
          const SizedBox(height: 8),

          // Password field
          TextFormField(
            controller: widget.passwordController,
            obscureText: _obscurePassword,
            validator: ValidationPatterns.validatePassword,
            enabled: widget.isEnabled,
            style: const TextStyle(fontSize: 15),
            decoration: _buildInputDecoration(
              hintText: "************",
              suffixIcon: IconButton(
                icon: Icon(
                  _obscurePassword ? Icons.visibility_off : Icons.visibility,
                  color: const Color(0xFF5B7CB5),
                  size: 20,
                ),
                onPressed: () {
                  setState(() {
                    _obscurePassword = !_obscurePassword;
                  });
                },
              ),
            ),
          ),

          const SizedBox(height: 12),

          // Forgot Password
          Align(
            alignment: Alignment.centerRight,
            child: GestureDetector(
              onTap: widget.isEnabled ? widget.onForgotPassword : null,
              child: Text(
                "Forgot Password?",
                style: TextStyle(
                  fontSize: 13,
                  color: widget.isEnabled
                      ? const Color(0xFF5B7CB5)
                      : const Color(0xFF5B7CB5).withOpacity(0.5),
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ),

          const SizedBox(height: 32),

          // Log In button
          SizedBox(
            height: 52,
            child: ElevatedButton(
              onPressed: widget.isEnabled ? widget.onSubmit : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF5B7CB5),
                foregroundColor: Colors.white,
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(6),
                ),
                disabledBackgroundColor:
                    const Color(0xFF5B7CB5).withOpacity(0.6),
              ),
              child: const Text(
                "Log In",
                style: TextStyle(
                  fontSize: 17,
                  fontWeight: FontWeight.w600,
                  letterSpacing: 0.3,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  InputDecoration _buildInputDecoration({
    required String hintText,
    Widget? suffixIcon,
  }) {
    return InputDecoration(
      hintText: hintText,
      hintStyle: TextStyle(
        color: const Color(0xFF5B6B8C).withOpacity(0.3),
        fontSize: 15,
      ),
      filled: true,
      fillColor: Colors.white,
      suffixIcon: suffixIcon,
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
        vertical: 16,
      ),
      errorStyle: const TextStyle(fontSize: 12),
    );
  }
}
