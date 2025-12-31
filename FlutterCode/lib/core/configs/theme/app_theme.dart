import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

class AppTheme {
  static final lightTheme = ThemeData(
    primaryColor: AppColors.primary,
    brightness: Brightness.light,
    fontFamily: GoogleFonts.inter().fontFamily,
    textTheme: TextTheme(
      bodyLarge: TextStyle(
        color: AppColors.mainBlackShade,
        fontSize: 16,
      ),
      bodyMedium: TextStyle(
        color: AppColors.mainBlackShade,
        fontSize: 14,
      ),
      bodySmall: TextStyle(
        color: AppColors.mainBlackShade,
        fontSize: 12,
      ),
      labelLarge: TextStyle(
        color: AppColors.mainBlackShade,
        fontSize: 14,
        fontWeight: FontWeight.bold,
      ),
      labelSmall: TextStyle(
        color: AppColors.mainBlackShade,
        fontSize: 12,
        fontWeight: FontWeight.bold,
      ),
    ),
    scaffoldBackgroundColor: AppColors.backgroundColor,
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.backgroundColor,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(10),
        ),
        padding: const EdgeInsets.symmetric(
            vertical: 16, horizontal: 24),
        textStyle: const TextStyle(
          fontSize: 16,
          fontWeight: FontWeight.bold,
        ),
      ),
    ),
  );
}
