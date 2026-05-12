import 'package:flutter/material.dart';
import 'package:famxpense/core/theme/app_colors.dart';
import 'package:famxpense/features/Auth/pages/validation_patterns.dart';
import 'package:famxpense/l10n/app_localizations.dart';

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
    final l10n = AppLocalizations.of(context)!;
    
    return Form(
      key: widget.formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            l10n.email,
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: 8),

          TextFormField(
            controller: widget.emailController,
            keyboardType: TextInputType.emailAddress,
            validator: ValidationPatterns.validateEmail,
            enabled: widget.isEnabled,
            style: const TextStyle(fontSize: 15),
            decoration: _buildInputDecoration(
              hintText: l10n.emailPlaceholder,
            ),
          ),
          const SizedBox(height: 20),

          Text(
            l10n.password,
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: 8),

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
                  color: AppColors.primary,
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

          Align(
            alignment: Alignment.centerRight,
            child: GestureDetector(
              onTap: widget.isEnabled ? widget.onForgotPassword : null,
              child: Text(
                l10n.forgotPassword,
                style: TextStyle(
                  fontSize: 13,
                  color: widget.isEnabled
                      ? AppColors.primary
                      : AppColors.primary.withOpacity(0.5),
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ),

          const SizedBox(height: 32),

          SizedBox(
            height: 52,
            child: ElevatedButton(
              onPressed: widget.isEnabled ? widget.onSubmit : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(6),
                ),
                disabledBackgroundColor:
                    AppColors.primary.withOpacity(0.6),
              ),
              child: Text(
                l10n.logIn,
                style: const TextStyle(
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
        color: AppColors.textSecondary.withOpacity(0.3),
        fontSize: 15,
      ),
      filled: true,
      fillColor: AppColors.surface,
      suffixIcon: suffixIcon,
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
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(6),
        borderSide: const BorderSide(
          color: AppColors.error,
          width: 1.5,
        ),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(6),
        borderSide: const BorderSide(
          color: AppColors.error,
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
