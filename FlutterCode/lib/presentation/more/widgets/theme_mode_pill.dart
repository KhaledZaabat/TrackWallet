import 'package:famxpense/core/configs/theme/app_colors.dart';
import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class ThemeModePill extends StatelessWidget {
  final String value;
  final VoidCallback onTap;

  const ThemeModePill({
    super.key,
    required this.value,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(10),
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(
            horizontal: 16, vertical: 8),
        decoration: BoxDecoration(
          color: AppColors
              .secondary, // light bluish background
          borderRadius: BorderRadius.circular(10),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              value,
              style: TextStyle(
                fontSize: 14,
                fontFamily: GoogleFonts.inter().fontFamily,
                color: AppColors.mainBlackShade,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(width: 6),
            const Icon(
              Icons.arrow_drop_down,
              size: 18,
              color: AppColors.mainBlackShade,
            ),
          ],
        ),
      ),
    );
  }
}
