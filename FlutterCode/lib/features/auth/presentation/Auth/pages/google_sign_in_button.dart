import 'package:flutter/material.dart';

/// Reusable Google Sign-In button widget
class GoogleSignInButton extends StatelessWidget {
  final VoidCallback onPressed;
  final bool isLoading;
  final bool isEnabled;

  const GoogleSignInButton({
    Key? key,
    required this.onPressed,
    this.isLoading = false,
    this.isEnabled = true,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 52,
      child: OutlinedButton.icon(
        onPressed: (isEnabled && !isLoading) ? onPressed : null,
        style: OutlinedButton.styleFrom(
          foregroundColor: const Color(0xFF5B6B8C),
          side: BorderSide(
            color: const Color(0xFFE0E5EB),
            width: 1.5,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(6),
          ),
          disabledForegroundColor: const Color(0xFF5B6B8C).withOpacity(0.3),
        ),
        icon: isLoading
            ? const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation<Color>(
                    Color(0xFF5B6B8C),
                  ),
                ),
              )
            : Image.asset(
                'assets/images/google_logo.png', // Add Google logo to assets
                width: 24,
                height: 24,
                errorBuilder: (context, error, stackTrace) {
                  // Fallback to icon if image not found
                  return Icon(
                    Icons.g_mobiledata,
                    size: 28,
                    color: (isEnabled && !isLoading)
                        ? const Color(0xFF5B6B8C)
                        : const Color(0xFF5B6B8C).withOpacity(0.3),
                  );
                },
              ),
        label: Text(
          "Continue with Google",
          style: TextStyle(
            fontSize: 15,
            fontWeight: FontWeight.w600,
            color: (isEnabled && !isLoading)
                ? const Color(0xFF5B6B8C)
                : const Color(0xFF5B6B8C).withOpacity(0.3),
          ),
        ),
      ),
    );
  }
}
