import 'package:flutter/material.dart';

class AppColors {
  static const Color black = Color(0xFF101010);
  static const Color darkGrey = Color(0xFF2C2C2E);
  static const Color grey = Color(0xFF8E8E93);
  static const Color lightGrey = Color(0xFFD1D1D6);
  static const Color extraLightGrey = Color(0xFFF2F2F7);
  static const Color white = Color(0xFFFFFFFF);

  // Semantic Colors (still need red for Expense, green for Income, but desaturated/modern)
  static const Color error = Color(0xFFFF453A); // Modern Red
  static const Color success = Color(0xFF32D74B); // Modern Green
  static const Color warning = Color(0xFFFF9F0A); // Modern Orange

  static const Color primary = black;
  static const Color background = Color(0xFFF5F5F5); // Slightly off-white for depth
  static const Color surface = white;
  static const Color textPrimary = black;
  static const Color textSecondary = Color(0xFF6C6C70);
  static const Color border = Color(0xFFE5E5EA);

  static const LinearGradient wolfGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [
      Color(0xFF2C2C2E),
      Color(0xFF000000),
    ],
  );
}
