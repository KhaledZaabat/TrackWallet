import 'package:famxpense/l10n/app_localizations.dart';
import 'package:flutter/material.dart';

class GoogleSignInButton extends StatelessWidget {
  final VoidCallback onPressed;
  final bool isLoading;
  final bool isEnabled;

  const GoogleSignInButton({
    super.key,
    required this.onPressed,
    this.isLoading = false,
    this.isEnabled = true,
  });

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    
    return SizedBox(
      height: 50,
      child: OutlinedButton(
        onPressed: isEnabled ? onPressed : null,
        style: OutlinedButton.styleFrom(
          backgroundColor: Colors.white,
          side: const BorderSide(
            color: Color(0xFFE0E5EB),
            width: 1.5,
          ),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(6),
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Image.asset(
              'assets/images/google_icon.png',
              width: 20,
              height: 20,
            ),
            const SizedBox(width: 12),
            Text(
              l10n.signInWithGoogle,
              style: TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w600,
                color: isEnabled
                    ? const Color(0xFF5B6B8C)
                    : const Color(0xFF5B6B8C).withOpacity(0.5),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
